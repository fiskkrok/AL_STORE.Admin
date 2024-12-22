// src/app/core/services/bulk-actions.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Order, OrderStatus } from '../../shared/models/order.model';

@Injectable({
    providedIn: 'root'
})
export class BulkActionsService {
    private readonly apiUrl = environment.apiUrls.admin.orders;

    constructor(private readonly http: HttpClient) { }

    // Update status for multiple orders
    updateOrderStatus(orderIds: string[], newStatus: OrderStatus): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/bulk/status`, {
            orderIds,
            newStatus
        });
    }

    // Generate PDF invoices for selected orders
    generateInvoices(orders: Order[]): Observable<Blob> {
        return this.http.post(`${this.apiUrl}/bulk/invoices`, {
            orderIds: orders.map(order => order.id)
        }, {
            responseType: 'blob'
        });
    }

    // Generate shipping labels for selected orders
    generateShippingLabels(orders: Order[]): Observable<Blob> {
        return this.http.post(`${this.apiUrl}/bulk/shipping-labels`, {
            orderIds: orders.map(order => order.id)
        }, {
            responseType: 'blob'
        });
    }

    // Generate packing slips for selected orders
    generatePackingSlips(orders: Order[]): Observable<Blob> {
        return this.http.post(`${this.apiUrl}/bulk/packing-slips`, {
            orderIds: orders.map(order => order.id)
        }, {
            responseType: 'blob'
        });
    }

    // Validate orders can be processed in bulk
    validateBulkOperation(orders: Order[], operation: string): Observable<{
        valid: boolean;
        invalidOrders: string[];
        reason?: string;
    }> {
        return this.http.post<{
            valid: boolean;
            invalidOrders: string[];
            reason?: string;
        }>(`${this.apiUrl}/bulk/validate`, {
            orderIds: orders.map(order => order.id),
            operation
        });
    }

    // Get bulk operation status (for long-running operations)
    getBulkOperationStatus(operationId: string): Observable<{
        status: 'pending' | 'processing' | 'completed' | 'failed';
        progress: number;
        error?: string;
    }> {
        return this.http.get<{
            status: 'pending' | 'processing' | 'completed' | 'failed';
            progress: number;
            error?: string;
        }>(`${this.apiUrl}/bulk/status/${operationId}`);
    }
}