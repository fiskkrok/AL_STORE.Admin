// src/app/store/stock/stock.reducer.ts
import { createReducer, on } from '@ngrx/store';
import { StockActions } from './stock.actions';
import { initialStockState } from './stock.state';

export const stockReducer = createReducer(
    initialStockState,

    // Load Stock
    on(StockActions.loadStock, (state) => ({
        ...state,
        loading: true,
        error: null
    })),

    on(StockActions.loadStockSuccess, (state, { stock }) => ({
        ...state,
        items: {
            ...state.items,
            [stock.productId]: stock
        },
        loading: false
    })),

    on(StockActions.loadStockFailure, (state, { error }) => ({
        ...state,
        loading: false,
        error
    })),

    // Real-time Updates
    on(StockActions.stockUpdated, (state, { stock }) => ({
        ...state,
        items: {
            ...state.items,
            [stock.productId]: stock
        }
    })),
    // Real-time Updates
    on(StockActions.stockUpdated, (state, { stock }) => ({
        ...state,
        items: {
            ...state.items,
            [stock.productId]: stock
        }
    })),

    on(StockActions.lowStockAlert, (state, { stock }) => ({
        ...state,
        lowStockAlerts: [
            ...state.lowStockAlerts.filter(item => item.productId !== stock.productId),
            stock
        ]
    })),

    on(StockActions.outOfStockAlert, (state, { stock }) => ({
        ...state,
        outOfStockAlerts: [
            ...state.outOfStockAlerts.filter(item => item.productId !== stock.productId),
            stock
        ]
    })),
    on(StockActions.lowStockAlert, (state, { stock }) => ({
        ...state,
        lowStockAlerts: [
            ...state.lowStockAlerts.filter(item => item.productId !== stock.productId),
            stock
        ]
    })),

    // Load Alerts
    on(StockActions.loadLowStockItemsSuccess, (state, { items }) => ({
        ...state,
        lowStockAlerts: items
    })),

    on(StockActions.loadOutOfStockItemsSuccess, (state, { items }) => ({
        ...state,
        outOfStockAlerts: items
    }))
);