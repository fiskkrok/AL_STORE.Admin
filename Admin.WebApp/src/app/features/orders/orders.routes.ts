// src/app/features/orders/orders.routes.ts
import { Routes } from '@angular/router';
import { OrderListComponent } from './order-list/order-list.component';
import { provideState } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { orderReducer } from '../../store/order/order.reducer';
import { OrderEffects } from '../../store/order/order.effects';
import { authGuard } from '../../core/guards/auth.guard';

export const ORDER_ROUTES: Routes = [
    {
        path: '',
        providers: [
            provideState('orders', orderReducer),
            provideEffects(OrderEffects)
        ],
        children: [
            {
                path: '',
                component: OrderListComponent,
                canActivate: [authGuard],
                title: 'Orders'
            }
        ]
    }
];