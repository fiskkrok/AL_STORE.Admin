// src/app/store/dashboard/dashboard.state.ts
import { DashboardStats } from '../../core/services/dashboard.service';

export interface DashboardState {
    stats: DashboardStats | null;
    loading: boolean;
    error: string | null;
    lastUpdated: number | null;
}

export const initialDashboardState: DashboardState = {
    stats: null,
    loading: false,
    error: null,
    lastUpdated: null
};