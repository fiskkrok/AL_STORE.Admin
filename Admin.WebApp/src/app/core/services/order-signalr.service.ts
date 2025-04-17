// src/app/core/services/order-signalr.service.ts
import { Injectable, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { OrderActions } from '../../store/order/order.actions';
import { BaseSignalRService } from './signalr-service.base';
import { environment } from '../../../environments/environment';
import { Order } from '../../shared/models/order.model';
import { Subject } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class OrderSignalRService extends BaseSignalRService {
    protected override hubUrl = environment.signalR.order;
    private readonly store = inject(Store);

    // Event subjects
    private readonly orderCreatedSubject = new Subject<Order>();
    readonly orderCreated$ = this.orderCreatedSubject.asObservable();

    private readonly orderUpdatedSubject = new Subject<Order>();
    readonly orderUpdated$ = this.orderUpdatedSubject.asObservable();

    private readonly orderStatusChangedSubject = new Subject<{ orderId: string, newStatus: string }>();
    readonly orderStatusChanged$ = this.orderStatusChangedSubject.asObservable();

    /**
     * Register SignalR event handlers for order hub
     */
    protected override registerEventHandlers(): void {
        if (!this.hubConnection) return;

        // Handle order created
        this.hubConnection.on('OrderCreated', (order: Order) => {
            console.log('Order created via SignalR:', order.id);
            this.orderCreatedSubject.next(order);
            // No need to dispatch to store as new orders usually aren't in the current view
        });

        // Handle order updated
        this.hubConnection.on('OrderUpdated', (order: Order) => {
            console.log('Order updated via SignalR:', order.id);
            this.orderUpdatedSubject.next(order);
            this.store.dispatch(OrderActions.orderUpdated({ order }));
        });

        // Handle order status changed
        this.hubConnection.on('OrderStatusChanged', (orderId: string, newStatus: string) => {
            console.log(`Order ${orderId} status changed to ${newStatus} via SignalR`);

            const statusUpdate = { orderId, newStatus };
            this.orderStatusChangedSubject.next(statusUpdate);
            this.store.dispatch(OrderActions.orderStatusChanged(statusUpdate));
        });
    }

    /**
     * Subscribe to updates for a specific order
     * @param orderId Order ID to watch
     */
    watchOrder(orderId: string): void {
        if (!this.isConnected()) {
            console.warn('Cannot watch order: not connected');
            return;
        }

        this.hubConnection!.invoke('WatchOrder', orderId)
            .catch(err => console.error(`Error subscribing to order ${orderId}:`, err));
    }

    /**
     * Stop watching updates for a specific order
     * @param orderId Order ID to unwatch
     */
    unwatchOrder(orderId: string): void {
        if (!this.isConnected()) return;

        this.hubConnection!.invoke('UnwatchOrder', orderId)
            .catch(err => console.error(`Error unsubscribing from order ${orderId}:`, err));
    }
}