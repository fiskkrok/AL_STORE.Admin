// src/app/core/services/stock.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import * as signalR from '@microsoft/signalr';
import { Store } from '@ngrx/store';
import { StockActions } from '../../store/stock/stock.actions';
import { BatchStockAdjustment, StockAdjustment, StockItem } from 'src/app/shared/models/stock.model';
import { AuthService } from './auth.service';

@Injectable({
    providedIn: 'root'
})
export class StockService {
    private readonly apiUrl = environment.apiUrls.admin.products;
    private hubConnection: signalR.HubConnection | undefined;
    private readonly authService = inject(AuthService);

    constructor(
        private readonly http: HttpClient,
        private readonly store: Store
    ) {
        this.initializeSignalR();
    }

    private initializeSignalR() {
        this.authService.getAccessToken().subscribe(token => {
            if (!token) {
                console.warn('No auth token available for Stock SignalR connection');
                return;
            }

            this.createConnection(token);
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
            .withUrl(environment.signalR.product, {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        this.setupHubListeners();
        this.hubConnection.start().catch(err => console.error('Error starting SignalR:', err));
    }

    private setupHubListeners() {
        if (!this.hubConnection) return;

        this.hubConnection.on('StockUpdated', (stock: StockItem) => {
            this.store.dispatch(StockActions.stockUpdated({ stock }));
        });

        this.hubConnection.on('LowStockAlert', (stock: StockItem) => {
            this.store.dispatch(StockActions.lowStockAlert({ stock }));
        });
    }

    getStockLevel(productId: string): Observable<StockItem> {
        return this.http.get<StockItem>(`${this.apiUrl}/${productId}`);
    }

    // Clean up SignalR connection
    ngOnDestroy() {
        if (this.hubConnection) {
            this.hubConnection.stop();
        }
    }

    getStockItem(productId: string): Observable<StockItem> {
        return this.http.get<any>(`${this.apiUrl}/stock/${productId}`).pipe(
            map(this.mapStockItemFromApi)
        );
    }

    getLowStockItems(): Observable<StockItem[]> {
        return this.http.get<any[]>(`${this.apiUrl}/stock/low-stock`).pipe(
            map(items => items.map(this.mapStockItemFromApi))
        );
    }

    getOutOfStockItems(): Observable<StockItem[]> {
        return this.http.get<any[]>(`${this.apiUrl}/stock/out-of-stock`).pipe(
            map(items => items.map(this.mapStockItemFromApi))
        );
    }

    adjustStock(adjustment: StockAdjustment): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/stock/${adjustment.productId}/adjust`, adjustment);
    }

    batchAdjustStock(adjustments: BatchStockAdjustment): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/stock/batch-adjust`, adjustments);
    }

    private mapStockItemFromApi(item: any): StockItem {
        return {
            id: item.id,
            productId: item.productId,
            productName: item.productName || 'Unknown Product',
            currentStock: item.currentStock,
            reservedStock: item.reservedStock,
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