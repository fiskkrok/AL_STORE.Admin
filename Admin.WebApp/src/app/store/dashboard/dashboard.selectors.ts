// src/app/store/dashboard/dashboard.selectors.ts
import { createFeatureSelector, createSelector } from '@ngrx/store';
import { DashboardState } from './dashboard.state';

export const selectDashboardState = createFeatureSelector<DashboardState>('dashboard');

export const selectDashboardStats = createSelector(
    selectDashboardState,
    state => state.stats
);

export const selectDashboardLoading = createSelector(
    selectDashboardState,
    state => state.loading
);

export const selectDashboardError = createSelector(
    selectDashboardState,
    state => state.error
);

export const selectLastUpdated = createSelector(
    selectDashboardState,
    state => state.lastUpdated
);