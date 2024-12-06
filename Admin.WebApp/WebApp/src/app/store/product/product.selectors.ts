import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ProductState } from './product.state';

export const selectProductState = createFeatureSelector<ProductState>('product');

export const selectAllProducts = createSelector(
    selectProductState,
    (state) => state.products
);

export const selectSelectedProduct = createSelector(
    selectProductState,
    (state) => state.selectedProduct
);

export const selectProductsLoading = createSelector(
    selectProductState,
    (state) => state.loading
);

export const selectProductsError = createSelector(
    selectProductState,
    (state) => state.error
);

export const selectProductFilters = createSelector(
    selectProductState,
    (state) => state.filters
);

export const selectProductPagination = createSelector(
    selectProductState,
    (state) => state.pagination
);
