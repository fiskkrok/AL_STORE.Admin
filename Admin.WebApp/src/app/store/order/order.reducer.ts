// src/app/store/order/order.reducer.ts
import { createReducer, on } from '@ngrx/store';
import { OrderActions } from './order.actions';
import { initialOrderState } from './order.state';
import { OrderStatus } from 'src/app/shared/models/order.model';

export const orderReducer = createReducer(
    initialOrderState,

    // Load Orders
    on(OrderActions.loadOrders, (state) => ({
        ...state,
        loading: true,
        error: null
    })),

    on(OrderActions.loadOrdersSuccess, (state, { orders, totalItems }) => ({
        ...state,
        orders,
        loading: false,
        error: null,
        pagination: {
            ...state.pagination,
            totalItems
        }
    })),

    on(OrderActions.loadOrdersFailure, (state, { error }) => ({
        ...state,
        loading: false,
        error
    })),

    // Update Status
    on(OrderActions.updateStatus, (state) => ({
        ...state,
        loading: true,
        error: null
    })),

    on(OrderActions.updateStatusSuccess, (state, { order }) => ({
        ...state,
        orders: state.orders.map(o => o.id === order.id ? order : o),
        loading: false,
        error: null
    })),

    on(OrderActions.updateStatusFailure, (state, { error }) => ({
        ...state,
        loading: false,
        error
    })),

    // Real-time Updates
    on(OrderActions.orderUpdated, (state, { order }) => ({
        ...state,
        orders: state.orders.map(o => o.id === order.id ? order : o)
    })),

    on(OrderActions.orderStatusChanged, (state, { orderId, newStatus }) => ({
        ...state,
        orders: state.orders.map(o =>
            o.id === orderId
                ? { ...o, status: newStatus as OrderStatus }
                : o
        )
    })),

    // Filters
    on(OrderActions.setFilters, (state, { filters }) => ({
        ...state,
        filters: {
            ...state.filters,
            ...filters
        }
    })),

    on(OrderActions.resetFilters, (state) => ({
        ...state,
        filters: initialOrderState.filters
    }))
);