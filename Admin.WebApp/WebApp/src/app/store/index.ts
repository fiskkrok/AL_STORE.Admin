// src/app/store/index.ts
import { isDevMode } from '@angular/core';
import { ActionReducerMap, MetaReducer } from '@ngrx/store';
import { productReducer } from './product/product.reducer';
import { ProductState } from './product/product.state';

export interface AppState {
    product: ProductState;
    // Add other feature states here
}

export const reducers: ActionReducerMap<AppState> = {
    product: productReducer,
    // Add other reducers here
};

export const metaReducers: MetaReducer<AppState>[] = isDevMode() ? [] : [];