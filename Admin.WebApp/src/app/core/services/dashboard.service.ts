// src/app/core/services/dashboard.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface DashboardStats {
    revenue: {
        total: number;
        trend: Array<{
            date: string;
            amount: number;
        }>;
    };
    orders: {
        total: number;
        trend: Array<{
            date: string;
            count: number;
        }>;
        byStatus: Array<{
            status: string;
            value: number;
            color: string;
        }>;
    };
    inventory: {
        lowStock: number;
        outOfStock: number;
        items: Array<{
            id: string;
            name: string;
            sku: string;
            currentStock: number;
            minimumStock: number;
        }>;
    };
    shipping: {
        pending: number;
        late: number;
        pendingOrders: Array<{
            id: string;
            orderNumber: string;
            createdAt: string;
            status: string;
        }>;
    };
}

@Injectable({
    providedIn: 'root'
})
export class DashboardService {
    private readonly apiUrl = environment.apiUrls.admin.dashboard;

    constructor(private readonly http: HttpClient) { }

    // Get complete dashboard stats
    getDashboardStats(): Observable<DashboardStats> {
        return combineLatest([
            this.getRevenueStats(),
            this.getOrderStats(),
            this.getInventoryStats(),
            this.getShippingStats()
        ]).pipe(
            map(([revenue, orders, inventory, shipping]) => ({
                revenue,
                orders,
                inventory,
                shipping
            }))
        );
    }

    // Get revenue statistics
    private getRevenueStats(): Observable<DashboardStats['revenue']> {
        return this.http.get<DashboardStats['revenue']>(`${this.apiUrl}/revenue`);
    }

    // Get order statistics
    private getOrderStats(): Observable<DashboardStats['orders']> {
        return this.http.get<DashboardStats['orders']>(`${this.apiUrl}/orders`).pipe(
            map(stats => ({
                ...stats,
                byStatus: stats.byStatus.map(status => ({
                    ...status,
                    color: this.getStatusColor(status.status)
                }))
            }))
        );
    }

    // Get inventory alerts
    private getInventoryStats(): Observable<DashboardStats['inventory']> {
        return this.http.get<DashboardStats['inventory']>(`${this.apiUrl}/inventory`);
    }

    // Get shipping statistics
    private getShippingStats(): Observable<DashboardStats['shipping']> {
        return this.http.get<DashboardStats['shipping']>(`${this.apiUrl}/shipping`);
    }

    // Helper method to get consistent colors for order statuses
    private getStatusColor(status: string): string {
        const colorMap: { [key: string]: string } = {
            'pending': '#FCD34D',   // Yellow
            'confirmed': '#60A5FA', // Blue
            'processing': '#818CF8', // Indigo
            'shipped': '#34D399',   // Green
            'delivered': '#10B981', // Emerald
            'cancelled': '#EF4444', // Red
            'refunded': '#F87171'   // Light Red
        };

        return colorMap[status.toLowerCase()] || '#6B7280'; // Gray default
    }
}