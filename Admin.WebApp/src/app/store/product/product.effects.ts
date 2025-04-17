import { inject, Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { map, mergeMap, catchError, withLatestFrom, tap } from 'rxjs/operators';
import { ProductService } from '../../core/services/product.service';
import { ProductActions } from './product.actions';
import { selectProductFilters } from './product.selectors';
import { ErrorService } from '../../core/services/error.service';
import { LoadingService } from '../../core/services/loading.service';

@Injectable()
export class ProductEffects {
    private readonly actions$ = inject(Actions);
    private readonly store = inject(Store);
    private readonly productService = inject(ProductService);
    private readonly errorService = inject(ErrorService);
    private readonly loadingService = inject(LoadingService);


    loadProducts$ = createEffect(() => {
        return this.actions$.pipe(
            ofType(ProductActions.loadProducts),
            tap(() => this.loadingService.show()),
            withLatestFrom(this.store.select(selectProductFilters)),
            mergeMap(([action, stateFilters]) =>
                this.productService.getPaged({ ...stateFilters, ...action.filters }).pipe(
                    map(response => {
                        this.loadingService.hide();
                        return ProductActions.loadProductsSuccess({
                            products: response.items,
                            totalItems: response.totalCount
                        });
                    }),
                    catchError(error => {
                        this.loadingService.hide();
                        this.errorService.addError({
                            code: '',
                            message: 'Failed to load products: ' + error.message,
                            severity: 'error'
                        });
                        return of(ProductActions.loadProductsFailure({ error: error.message }));
                    })
                )
            )
        );
    });

    addProduct$ = createEffect(() =>
        this.actions$.pipe(
            ofType(ProductActions.addProduct),
            tap(() => this.loadingService.show()),
            mergeMap(action =>
                this.productService.createProduct({
                    ...action.product, categoryId: action.product.category.id
                }).pipe(
                    map(product => {
                        this.loadingService.hide();
                        return ProductActions.addProductSuccess({ product });
                    }),
                    catchError(error => {
                        this.loadingService.hide();
                        this.errorService.addError({
                            code: '',
                            message: 'Failed to add product: ' + error.message,
                            severity: 'error'
                        });
                        return of(ProductActions.addProductFailure({ error: error.message }));
                    })
                )
            )
        )
    );

    updateProduct$ = createEffect(() =>
        this.actions$.pipe(
            ofType(ProductActions.updateProduct),
            tap(() => this.loadingService.show()),
            mergeMap(action => {
                this.store.dispatch(ProductActions.optimisticUpdateProduct({
                    id: action.id,
                    changes: action.product
                }));

                return this.productService.updateProduct({ ...action.product, id: action.id }).pipe(
                    map(product => {
                        this.loadingService.hide();
                        return ProductActions.updateProductSuccess({ product });
                    }),
                    catchError(error => {
                        this.loadingService.hide();
                        this.store.dispatch(ProductActions.revertOptimisticUpdate());
                        this.errorService.addError({
                            code: '',
                            message: 'Failed to update product: ' + error.message,
                            severity: 'error'
                        });
                        return of(ProductActions.updateProductFailure({ error: error.message }));
                    })
                );
            })
        )
    );

    deleteProduct$ = createEffect(() => {
        return this.actions$.pipe(
            ofType(ProductActions.deleteProduct),
            tap(() => this.loadingService.show()),
            mergeMap(action =>
                this.productService.deleteProduct(action.id).pipe(
                    map(() => {
                        this.loadingService.hide();
                        return ProductActions.deleteProductSuccess({ id: action.id });
                    }),
                    catchError(error => {
                        this.loadingService.hide();
                        this.errorService.addError({
                            code: '',
                            message: 'Failed to delete product: ' + error.message,
                            severity: 'error'
                        });
                        return of(ProductActions.deleteProductFailure({ error: error.message }));
                    })
                )
            )
        );
    });
}
