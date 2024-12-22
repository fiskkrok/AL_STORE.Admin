// src/app/store/order/order.actions.ts
import { createActionGroup, props, emptyProps } from '@ngrx/store';
import { Order } from 'src/app/shared/models/orders/order.model';
import { OrderListParams } from '../../core/services/order.service';

export const OrderActions = createActionGroup({
    source: 'Order',
    events: {
        // Load Orders
        'Load Orders': props<{ params: OrderListParams }>(),
        'Load Orders Success': props<{ orders: Order[]; totalItems: number }>(),
        'Load Orders Failure': props<{ error: string }>(),

        // Update Order Status
        'Update Status': props<{ orderId: string; newStatus: string }>(),
        'Update Status Success': props<{ order: Order }>(),
        'Update Status Failure': props<{ error: string }>(),

        // Add Payment
        'Add Payment': props<{ orderId: string; payment: any }>(),
        'Add Payment Success': props<{ order: Order }>(),
        'Add Payment Failure': props<{ error: string }>(),

        // Update Shipping
        'Update Shipping': props<{ orderId: string; shipping: any }>(),
        'Update Shipping Success': props<{ order: Order }>(),
        'Update Shipping Failure': props<{ error: string }>(),

        // Real-time Updates
        'Order Updated': props<{ order: Order }>(),
        'Order Status Changed': props<{ orderId: string; newStatus: string }>(),

        // Filters
        'Set Filters': props<{ filters: Partial<OrderListParams> }>(),
        'Reset Filters': emptyProps(),
    }
});