import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { map, mergeMap, catchError, withLatestFrom } from 'rxjs/operators';
import { ProductService } from '../../core/services/product.service';
import { ProductActions } from './product.actions';
import { selectProductFilters } from './product.selectors';

@Injectable()
export class ProductEffects {
    loadProducts$ = createEffect(() =>
        this.actions$.pipe(
            ofType(ProductActions.loadProducts),
            withLatestFrom(this.store.select(selectProductFilters)),
            mergeMap(([action, stateFilters]) =>
                this.productService.getProducts({ ...stateFilters, ...action.filters }).pipe(
                    map(response => ProductActions.loadProductsSuccess({
                        products: response.items,
                        totalItems: response.totalCount
                    })),
                    catchError(error => of(ProductActions.loadProductsFailure({ error: error.message })))
                )
            )
        )
    );

    addProduct$ = createEffect(() =>
        this.actions$.pipe(
            ofType(ProductActions.addProduct),
            mergeMap(action =>
                this.productService.addProduct(action.product).pipe(
                    map(product => ProductActions.addProductSuccess({ product })),
                    catchError(error => of(ProductActions.addProductFailure({ error: error.message })))
                )
            )
        )
    );

    updateProduct$ = createEffect(() =>
        this.actions$.pipe(
            ofType(ProductActions.updateProduct),
            mergeMap(action =>
                this.productService.updateProduct(action.id, action.product).pipe(
                    map(() => ProductActions.updateProductSuccess({
                        product: { id: action.id, ...action.product } as Product
                    })),
                    catchError(error => of(ProductActions.updateProductFailure({ error: error.message })))
                )
            )
        )
    );

    deleteProduct$ = createEffect(() =>
        this.actions$.pipe(
            ofType(ProductActions.deleteProduct),
            mergeMap(action =>
                this.productService.deleteProduct(action.id).pipe(
                    map(() => ProductActions.deleteProductSuccess({ id: action.id })),
                    catchError(error => of(ProductActions.deleteProductFailure({ error: error.message })))
                )
            )
        )
    );

    constructor(
        private actions$: Actions,
        private store: Store,
        private productService: ProductService
    ) { }
}