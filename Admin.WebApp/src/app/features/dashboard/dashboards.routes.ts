import { Routes } from "@angular/router";
import { StockAlertsComponent } from "./components/stock-alerts/stock-alerts.component";
import { DashboardComponent } from "./dashboard.component";

export const DASHBOARD_ROUTES: Routes = [
    {
        path: '',
        component: DashboardComponent,
        children: [
            {
                path: 'stockalerts',
                component: StockAlertsComponent,
                title: 'Stock Alerts'
            },
        ]
    }
];