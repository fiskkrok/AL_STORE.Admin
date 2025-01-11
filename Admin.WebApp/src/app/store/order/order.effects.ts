// src/app/store/order/order.effects.ts
import { inject, Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { map, mergeMap, catchError, withLatestFrom, tap } from 'rxjs/operators';
import { OrderService } from '../../core/services/order.service';
import { OrderActions } from './order.actions';
import { selectOrderFilters } from './order.selectors';
import { ErrorService } from '../../core/services/error.service';
import { LoadingService } from '../../core/services/loading.service';
import { OrderStatus } from 'src/app/shared/models/order.model';

@Injectable()
export class OrderEffects {
    // constructor(
    //     private readonly actions$: Actions,
    //     private readonly store: Store,
    //     private readonly orderService: OrderService,
    //     private readonly errorService: ErrorService,
    //     private readonly loadingService: LoadingService
    // ) { }
    private readonly actions$ = inject(Actions);
    private readonly store = inject(Store);
    private readonly orderService = inject(OrderService);
    private readonly errorService = inject(ErrorService);
    private readonly loadingService = inject(LoadingService);

    loadOrders$ = createEffect(() => {
        return this.actions$.pipe(
            ofType(OrderActions.loadOrders),
            tap(() => this.loadingService.show()),
            withLatestFrom(this.store.select(selectOrderFilters)),
            mergeMap(([action, stateFilters]) =>
                this.orderService.getOrders({ ...stateFilters, ...action.params }).pipe(
                    map(response => {
                        this.loadingService.hide();
                        return OrderActions.loadOrdersSuccess({
                            orders: response.items,
                            totalItems: response.totalCount
                        });
                    }),
                    catchError(error => {
                        this.loadingService.hide();
                        this.errorService.addError({
                            message: 'Failed to load orders: ' + error.message,
                            code: error.code || 'LOAD_ERROR',
                            severity: 'error'
                        });
                        return of(OrderActions.loadOrdersFailure({
                            error: error.message
                        }));
                    })
                )
            )
        );
    });

    updateStatus$ = createEffect(() => {
        return this.actions$.pipe(
            ofType(OrderActions.updateStatus),
            mergeMap(action =>
                this.orderService.updateOrderStatus(action.orderId, {
                    newStatus: action.newStatus as OrderStatus
                }).pipe(
                    map(() => OrderActions.updateStatusSuccess({
                        order: { id: action.orderId, status: action.newStatus } as any
                    })),
                    catchError(error => {
                        this.errorService.addError({
                            message: 'Failed to update order status: ' + error.message,
                            code: error.code || 'UPDATE_ERROR',
                            severity: 'error'
                        });
                        return of(OrderActions.updateStatusFailure({
                            error: error.message
                        }));
                    })
                )
            )
        );
    });
}
