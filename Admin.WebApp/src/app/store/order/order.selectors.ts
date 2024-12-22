// src/app/store/order/order.selectors.ts
import { createFeatureSelector, createSelector } from '@ngrx/store';
import { OrderState } from './order.state';

export const selectOrderState = createFeatureSelector<OrderState>('orders');

export const selectAllOrders = createSelector(
    selectOrderState,
    state => state.orders
);

export const selectOrdersLoading = createSelector(
    selectOrderState,
    state => state.loading
);

export const selectOrdersError = createSelector(
    selectOrderState,
    state => state.error
);

export const selectOrderFilters = createSelector(
    selectOrderState,
    state => state?.filters
);

export const selectOrderPagination = createSelector(
    selectOrderState,
    state => state.pagination
);