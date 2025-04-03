// src/app/core/services/stock.service.ts
import { inject, Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subscription, of, throwError, timer } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import * as signalR from '@microsoft/signalr';
import { Store } from '@ngrx/store';
import { StockActions } from '../../store/stock/stock.actions';
import { BatchStockAdjustment, StockAdjustment, StockItem } from 'src/app/shared/models/stock.model';
import { SignalRFactoryService } from './signalr-factory.service';
import { BehaviorSubject } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
    providedIn: 'root'
})
export class StockService implements OnDestroy {
    private readonly apiUrl = environment.apiUrls.admin.stock;
    private readonly stockHubUrl = environment.signalR.stock;
    private readonly authService = inject(AuthService); // Inject AuthService
    private hubConnection?: signalR.HubConnection;
    private connectionSubscription?: Subscription;
    private pollingSubscription?: Subscription;

    // Connection status observable
    private readonly connectionStatusSubject = new BehaviorSubject<string>('disconnected');
    public readonly connectionStatus$ = this.connectionStatusSubject.asObservable();

    constructor(
        private http: HttpClient,
        private store: Store,
        private signalRFactory: SignalRFactoryService
    ) {
        this.initializeSignalR();
    }

    ngOnDestroy(): void {
        if (this.connectionSubscription) {
            this.connectionSubscription.unsubscribe();
        }
        this.stopPolling();
    }
    private initializeSignalR() {
        this.authService.getAccessToken().subscribe(token => {
            if (!token) {
                console.warn('No auth token available for Stock SignalR connection');
                return;
            }

            this.hubConnection = new signalR.HubConnectionBuilder()
                .withUrl(environment.signalR.category, {
                    accessTokenFactory: () => token
                })
                .withAutomaticReconnect()
                .build();

            this.hubConnection.start().catch(err => console.error('Error starting SignalR:', err));


        });
        // Recreate connection when auth state changes
        this.authService.authState$.subscribe(state => {
            if (state.isAuthenticated && state.accessToken && (!this.hubConnection || this.hubConnection.state === signalR.HubConnectionState.Disconnected)) {
                this.createConnection(state.accessToken);
            }
        });
    }
    private createConnection(token: string) {
        // Close existing connection if open
        if (this.hubConnection) {
            this.hubConnection.stop().catch(err => console.error('Error stopping connection:', err));
        }

        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(environment.signalR.stock, {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        this.setupHubListeners();
        this.hubConnection.start().catch(err => console.error('Error starting SignalR:', err));
    }
    // private initializeSignalR(): void {
    //     // Get connection from factory
    //     this.connectionSubscription = this.signalRFactory.getHubConnection(this.stockHubUrl)
    //         .pipe(
    //             tap(connection => {
    //                 this.hubConnection = connection;
    //                 this.setupHubListeners();
    //                 this.connectionStatusSubject.next('connected');
    //                 this.stopPolling(); // Stop polling when connected
    //             }),
    //             catchError(error => {
    //                 console.error('Error getting stock hub connection:', error);
    //                 this.connectionStatusSubject.next('error');
    //                 this.startPolling(); // Start polling when connection fails
    //                 return of(null);
    //             })
    //         )
    //         .subscribe();

    //     // Also monitor connection status
    //     this.signalRFactory.getConnectionStatus(this.stockHubUrl)
    //         .subscribe(status => {
    //             this.connectionStatusSubject.next(status);
    //             if (status === 'disconnected' || status === 'error') {
    //                 this.startPolling();
    //             } else if (status === 'connected') {
    //                 this.stopPolling();
    //             }
    //         });
    // }

    // Subscribe to stock updates for a specific product
    subscribeToProductStock(productId: string): void {
        if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
            this.hubConnection.invoke('SubscribeToProductStock', productId)
                .catch(err => console.error(`Error subscribing to product stock ${productId}:`, err));
        }
    }

    // Unsubscribe from stock updates for a specific product
    unsubscribeFromProductStock(productId: string): void {
        if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
            this.hubConnection.invoke('UnsubscribeFromProductStock', productId)
                .catch(err => console.error(`Error unsubscribing from product stock ${productId}:`, err));
        }
    }

    // Setup SignalR event listeners
    private setupHubListeners(): void {
        if (!this.hubConnection) return;

        // Handle stock updates
        this.hubConnection.on('StockUpdated', (stockItem: StockItem) => {
            if (stockItem) {
                this.store.dispatch(StockActions.stockUpdated({ stock: stockItem }));
            } else {
                // Null means "refresh all stock data"
                this.refreshAllStockData();
            }
        });

        // Handle low stock alerts
        this.hubConnection.on('LowStockAlert', (stockItem: StockItem) => {
            this.store.dispatch(StockActions.lowStockAlert({ stock: stockItem }));
        });

        // Handle out of stock alerts
        this.hubConnection.on('OutOfStockAlert', (stockItem: StockItem) => {
            // Add this action if not already in your store
            this.store.dispatch(StockActions.outOfStockAlert({ stock: stockItem }));
        });
    }

    // Start polling as a fallback when SignalR is disconnected
    private startPolling(): void {
        if (this.pollingSubscription) return;

        this.pollingSubscription = timer(0, 30000) // Initial delay 0, then every 30 seconds
            .pipe(
                switchMap(() => this.refreshAllStockData())
            )
            .subscribe();
    }

    // Stop polling when SignalR is connected
    private stopPolling(): void {
        if (this.pollingSubscription) {
            this.pollingSubscription.unsubscribe();
            this.pollingSubscription = undefined;
        }
    }

    // Refresh all stock data
    private refreshAllStockData(): Observable<any> {
        return this.getLowStockItems().pipe(
            tap(items => {
                this.store.dispatch(StockActions.loadLowStockItemsSuccess({ items }));
            }),
            switchMap(() => this.getOutOfStockItems()),
            tap(items => {
                this.store.dispatch(StockActions.loadOutOfStockItemsSuccess({ items }));
            }),
            catchError(error => {
                console.error('Error refreshing stock data:', error);
                return of(null);
            })
        );
    }

    // API methods
    getStockLevel(productId: string): Observable<StockItem> {
        return this.http.get<StockItem>(`${this.apiUrl}/${productId}`).pipe(
            catchError(error => {
                if (error.status === 404) {
                    // If stock item is not found, return a default one
                    console.warn(`Stock item not found for product ${productId}, using default`);
                    const defaultStock: StockItem = {
                        id: '00000000-0000-0000-0000-000000000000', // Placeholder ID
                        productId: productId,
                        productName: 'Unknown Product',
                        currentStock: 0,
                        reservedStock: 0,
                        availableStock: 0,
                        lowStockThreshold: 5,
                        trackInventory: true,
                        isLowStock: true,
                        isOutOfStock: true,
                        reservations: []
                    };
                    return of(defaultStock);
                }
                return throwError(() => error);
            })
        );
    }

    getLowStockItems(): Observable<StockItem[]> {
        return this.http.get<any[]>(`${this.apiUrl}/low-stock`).pipe(
            map(items => items.map(this.mapStockItemFromApi))
        );
    }

    getOutOfStockItems(): Observable<StockItem[]> {
        return this.http.get<any[]>(`${this.apiUrl}/out-of-stock`).pipe(
            map(items => items.map(this.mapStockItemFromApi))
        );
    }

    adjustStock(adjustment: StockAdjustment): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/${adjustment.productId}/adjust`, adjustment);
    }

    batchAdjustStock(adjustments: BatchStockAdjustment): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/batch-adjust`, adjustments);
    }

    private mapStockItemFromApi(item: any): StockItem {
        return {
            id: item.id,
            productId: item.productId,
            productName: item.productName || 'Unknown Product',
            currentStock: item.currentStock,
            reservedStock: item.reservedStock || 0,
            availableStock: item.availableStock,
            lowStockThreshold: item.lowStockThreshold,
            trackInventory: item.trackInventory,
            isLowStock: item.isLowStock,
            isOutOfStock: item.isOutOfStock,
            reservations: item.reservations?.map((r: any) => ({
                id: r.id,
                orderId: r.orderId,
                quantity: r.quantity,
                status: r.status,
                expiresAt: r.expiresAt,
                confirmedAt: r.confirmedAt,
                cancelledAt: r.cancelledAt
            })) || []
        };
    }
}