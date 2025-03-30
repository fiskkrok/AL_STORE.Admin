import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface DashboardStats {
    totalProducts: number;
    lowStockCount: number;
    totalCategories: number;
    activeProductCount: number;
    totalRevenue?: number;
    revenueChange?: number;
    totalOrders?: number;
    ordersChange?: number;
}

@Injectable({
    providedIn: 'root'
})
export class StatisticsService {
    private apiUrl = `${environment.apiUrls.admin.baseUrl}/statistics`;

    // Mock data for development/demo purposes
    private mockStats: DashboardStats = {
        totalProducts: 145,
        lowStockCount: 12,
        totalCategories: 8,
        activeProductCount: 132,
        totalRevenue: 24680,
        revenueChange: 12.5,
        totalOrders: 389,
        ordersChange: 8.3
    };

    private categoryCount = 8;

    constructor(private http: HttpClient) { }

    /**
     * Get statistics data from the API
     * @param timeRange - Optional time range filter (day, week, month)
     * @returns Observable of statistics data
     */
    getStatistics(timeRange: string = 'day'): Observable<DashboardStats> {
        // In a production environment, you would use the HTTP client to fetch data from your API
        // return this.http.get<DashboardStats>(`${this.apiUrl}?timeRange=${timeRange}`)
        //   .pipe(
        //     catchError(error => {
        //       console.error('Error fetching statistics:', error);
        //       return of(this.mockStats);
        //     })
        //   );

        // For demo purposes, return mock data with slight variations based on the time range
        const mockVariation = timeRange === 'day' ? 1 :
            timeRange === 'week' ? 1.5 : 2;

        return of({
            ...this.mockStats,
            totalRevenue: Math.round((this.mockStats.totalRevenue ?? 0) * mockVariation),
            totalOrders: Math.round((this.mockStats.totalOrders ?? 0) * mockVariation),
            revenueChange: (this.mockStats.revenueChange ?? 0) * (mockVariation * 0.8),
            ordersChange: (this.mockStats.ordersChange ?? 0) * (mockVariation * 0.9)
        });
    }

    /**
     * Get the count of categories
     * @returns Number of categories
     */
    getCategoryCount(): number {
        return this.categoryCount;
    }

    /**
     * Get products with low stock
     * @returns Observable of products with low stock
     */
    getLowStockProducts(): Observable<any[]> {
        // In production, use:
        // return this.http.get<any[]>(`${this.apiUrl}/low-stock`);

        // Mock data
        return of([
            { id: 1, name: 'Wireless Headphones', stock: 3, minStock: 5 },
            { id: 2, name: 'Smart Watch', stock: 2, minStock: 10 },
            { id: 3, name: 'Bluetooth Speaker', stock: 4, minStock: 8 }
        ]);
    }

    /**
     * Get top selling products
     * @param metric - The metric to sort by (sales or revenue)
     * @param limit - Maximum number of products to return
     * @returns Observable of top products
     */
    getTopProducts(metric: 'sales' | 'revenue' = 'sales', limit: number = 5): Observable<any[]> {
        // In production, use:
        // return this.http.get<any[]>(`${this.apiUrl}/top-products?metric=${metric}&limit=${limit}`);

        // Mock data
        const products = [
            { id: 1, name: 'Wireless Headphones', category: 'Electronics', sales: 145, revenue: 1245.50, change: 12.5 },
            { id: 2, name: 'Smart Watch', category: 'Wearables', sales: 98, revenue: 945.20, change: 8.3 },
            { id: 3, name: 'Smartphone Case', category: 'Accessories', sales: 312, revenue: 625.40, change: -2.1 },
            { id: 4, name: 'USB-C Cable', category: 'Accessories', sales: 254, revenue: 510.30, change: 5.2 },
            { id: 5, name: 'Bluetooth Speaker', category: 'Electronics', sales: 87, revenue: 870.00, change: 3.7 }
        ];

        // Sort by the specified metric
        const sortedProducts = [...products].sort((a, b) => b[metric] - a[metric]);

        return of(sortedProducts.slice(0, limit));
    }

    /**
     * Get recent activity items
     * @param limit - Maximum number of activity items to return
     * @returns Observable of recent activity items
     */
    getRecentActivity(limit: number = 5): Observable<any[]> {
        // In production, use:
        // return this.http.get<any[]>(`${this.apiUrl}/recent-activity?limit=${limit}`);

        // Mock data
        const activities = [
            { id: 1, type: 'product_add', title: 'New product added: Wireless Headphones', user: 'John Smith', timestamp: new Date(Date.now() - 2 * 60 * 60 * 1000) },
            { id: 2, type: 'order_ship', title: 'Order #12345 shipped', description: 'Shipped to Jane Doe', timestamp: new Date(Date.now() - 5 * 60 * 60 * 1000) },
            { id: 3, type: 'stock_update', title: 'Product stock updated: Smart Watch', user: 'Sarah Connor', timestamp: new Date(Date.now() - 24 * 60 * 60 * 1000) },
            { id: 4, type: 'category_add', title: 'New category added: Accessories', user: 'John Smith', timestamp: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000) }
        ];

        return of(activities.slice(0, limit));
    }
}