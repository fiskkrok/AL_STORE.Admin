// src/app/store/dashboard/dashboard.actions.ts
import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { DashboardStats } from '../../core/services/dashboard.service';

export const DashboardActions = createActionGroup({
    source: 'Dashboard',
    events: {
        'Load Stats': emptyProps(),
        'Load Stats Success': props<{ stats: DashboardStats }>(),
        'Load Stats Failure': props<{ error: string }>(),
        'Reset Stats': emptyProps()
    }
});