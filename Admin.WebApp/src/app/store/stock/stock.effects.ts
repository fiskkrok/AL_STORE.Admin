// src/app/store/stock/stock.effects.ts
import { inject, Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { map, mergeMap, catchError, tap } from 'rxjs/operators';
import { StockService } from 'src/app/core/services/stock.service';
import { StockActions } from './stock.actions';
import { ErrorService } from '../../core/services/error.service';
import { LoadingService } from '../../core/services/loading.service';


@Injectable()
export class StockEffects {

    private readonly actions$ = inject(Actions);
    private readonly stockService = inject(StockService);
    private readonly errorService = inject(ErrorService);
    private readonly loadingService = inject(LoadingService);
    private readonly store = inject(Store)

    loadStock$ = createEffect(() => {
        return this.actions$.pipe(
            ofType(StockActions.loadStock),
            tap(() => this.loadingService.show()),
            mergeMap(action =>
                this.stockService.getStockLevel(action.productId).pipe(
                    map(stock => {
                        this.loadingService.hide();
                        return StockActions.loadStockSuccess({ stock });
                    }),
                    catchError(error => {
                        this.loadingService.hide();
                        this.errorService.addError({
                            message: 'Failed to load stock information',
                            code: error.code || 'LOAD_ERROR',
                            severity: 'error'
                        });
                        return of(StockActions.loadStockFailure({
                            error: error.message || 'Failed to load stock information'
                        }));
                    })
                )
            )
        );
    });

    adjustStock$ = createEffect(() => {
        return this.actions$.pipe(
            ofType(StockActions.adjustStock),
            tap(() => this.loadingService.show()),
            mergeMap(action =>
                this.stockService.adjustStock(action.adjustment).pipe(
                    mergeMap(() =>
                        this.stockService.getStockLevel(action.adjustment.productId).pipe(
                            map(stock => {
                                this.loadingService.hide();
                                return StockActions.adjustStockSuccess({ stock });
                            })
                        )
                    ),
                    catchError(error => {
                        this.loadingService.hide();
                        this.errorService.addError({
                            message: 'Failed to adjust stock',
                            code: error.code || 'ADJUST_ERROR',
                            severity: 'error'
                        });
                        return of(StockActions.adjustStockFailure({
                            error: error.message || 'Failed to adjust stock'
                        }));
                    })
                )
            )
        );
    });

    loadLowStockItems$ = createEffect(() => {
        return this.actions$.pipe(
            ofType(StockActions.loadLowStockItems),
            mergeMap(() =>
                this.stockService.getLowStockItems().pipe(
                    map(items => StockActions.loadLowStockItemsSuccess({ items })),
                    catchError(error => of(StockActions.loadLowStockItemsFailure({
                        error: error.message || 'Failed to load low stock items'
                    })))
                )
            )
        );
    });

    loadOutOfStockItems$ = createEffect(() => {
        return this.actions$.pipe(
            ofType(StockActions.loadOutOfStockItems),
            mergeMap(() =>
                this.stockService.getOutOfStockItems().pipe(
                    map(items => StockActions.loadOutOfStockItemsSuccess({ items })),
                    catchError(error => of(StockActions.loadOutOfStockItemsFailure({
                        error: error.message || 'Failed to load out of stock items'
                    })))
                )
            )
        );
    });
}