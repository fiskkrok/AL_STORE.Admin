// src/app/store/dashboard/dashboard.effects.ts
import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, mergeMap, catchError, tap } from 'rxjs/operators';
import { DashboardService } from '../../core/services/dashboard.service';
import { DashboardActions } from './dashboard.actions';
import { ErrorService } from '../../core/services/error.service';
import { LoadingService } from '../../core/services/loading.service';

@Injectable()
export class DashboardEffects {
    constructor(
        private readonly actions$: Actions,
        private readonly dashboardService: DashboardService,
        private readonly errorService: ErrorService,
        private readonly loadingService: LoadingService
    ) { }

    loadStats$ = createEffect(() => {
        return this.actions$.pipe(
            ofType(DashboardActions.loadStats),
            tap(() => this.loadingService.show()),
            mergeMap(() =>
                this.dashboardService.getDashboardStats().pipe(
                    map(stats => {
                        this.loadingService.hide();
                        return DashboardActions.loadStatsSuccess({ stats });
                    }),
                    catchError(error => {
                        this.loadingService.hide();
                        this.errorService.addError({
                            message: 'Failed to load dashboard stats',
                            code: error.code || 'LOAD_ERROR',
                            severity: 'error'
                        });
                        return of(DashboardActions.loadStatsFailure({
                            error: error.message || 'Failed to load dashboard stats'
                        }));
                    })
                )
            )
        );
    });
}
