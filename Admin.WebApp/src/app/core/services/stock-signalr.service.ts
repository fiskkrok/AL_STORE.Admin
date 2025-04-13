// src/app/core/services/stock-signalr.service.ts
import { Injectable, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { StockActions } from '../../store/stock/stock.actions';
import { BaseSignalRService } from './signalr-service.base';
import { environment } from '../../../environments/environment';
import { StockItem } from '../../shared/models/stock.model';
import { Subject } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class StockSignalRService extends BaseSignalRService {
    protected override hubUrl = environment.signalR.stock;
    private readonly store = inject(Store);

    // Optional: Add specific subjects for stock events
    private readonly stockUpdateSubject = new Subject<StockItem>();
    readonly stockUpdate$ = this.stockUpdateSubject.asObservable();

    /**
     * Register specific event handlers for stock hub
     */
    protected override registerEventHandlers(): void {
        if (!this.hubConnection) return;

        // Handle stock updates
        this.hubConnection.on('StockUpdated', (stockItem: StockItem) => {
            console.log('Stock updated via SignalR:', stockItem.productId);

            // Emit to internal subject
            this.stockUpdateSubject.next(stockItem);

            // Dispatch to NgRx store
            this.store.dispatch(StockActions.stockUpdated({ stock: stockItem }));
        });

        // Handle low stock alerts
        this.hubConnection.on('LowStockAlert', (stockItem: StockItem) => {
            console.log('Low stock alert via SignalR:', stockItem.productId);
            this.store.dispatch(StockActions.lowStockAlert({ stock: stockItem }));
        });

        // Handle out of stock alerts
        this.hubConnection.on('OutOfStockAlert', (stockItem: StockItem) => {
            console.log('Out of stock alert via SignalR:', stockItem.productId);
            this.store.dispatch(StockActions.outOfStockAlert({ stock: stockItem }));
        });
    }

    /**
     * Subscribe to product stock updates
     * @param productId ID of product to subscribe to
     */
    subscribeToProductStock(productId: string): void {
        if (!this.isConnected()) {
            console.warn('Cannot subscribe to product stock: not connected');
            return;
        }

        this.hubConnection!.invoke('SubscribeToProductStock', productId)
            .catch(err => console.error(`Error subscribing to product stock ${productId}:`, err));
    }

    /**
     * Unsubscribe from product stock updates
     * @param productId ID of product to unsubscribe from
     */
    unsubscribeFromProductStock(productId: string): void {
        if (!this.isConnected()) {
            return;
        }

        this.hubConnection!.invoke('UnsubscribeFromProductStock', productId)
            .catch(err => console.error(`Error unsubscribing from product stock ${productId}:`, err));
    }
}