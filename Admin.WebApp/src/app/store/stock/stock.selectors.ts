// src/app/store/stock/stock.selectors.ts
import { createFeatureSelector, createSelector } from '@ngrx/store';
import { StockState } from './stock.state';

export const selectStockState = createFeatureSelector<StockState>('stock');

export const selectAllStockItems = createSelector(
    selectStockState,
    (state) => state.items
);

export const selectStockForProduct = (productId: string) => createSelector(
    selectAllStockItems,
    (items) => items[productId]
);

export const selectStockLoading = createSelector(
    selectStockState,
    (state) => state.loading
);

export const selectStockError = createSelector(
    selectStockState,
    (state) => state.error
);

export const selectLowStockAlerts = createSelector(
    selectStockState,
    (state) => state.lowStockAlerts
);

export const selectOutOfStockAlerts = createSelector(
    selectStockState,
    (state) => state.outOfStockAlerts
);