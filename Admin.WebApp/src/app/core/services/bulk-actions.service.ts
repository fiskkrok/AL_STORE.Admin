// src/app/core/services/bulk-actions.service.ts
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { BaseCrudService } from './base-crud.service';
import { Order, OrderStatus } from '../../shared/models/order.model';

// Interface representing a generic bulk operation
interface BulkOperation<T> {
    ids: string[];
    operation: string;
    params?: T;
}

// Interface for bulk operation status
interface BulkOperationStatus {
    status: 'pending' | 'processing' | 'completed' | 'failed';
    progress: number;
    error?: string;
}

// Interface for bulk validation result
interface BulkValidationResult {
    valid: boolean;
    invalidItems: string[];
    reason?: string;
}

@Injectable({
    providedIn: 'root'
})
export class BulkActionsService extends BaseCrudService<any, string> {
    // Override the endpoint to point to bulk operations
    protected override endpoint = 'orders/bulk';

    /**
     * Update status for multiple orders
     * @param orderIds Array of order IDs
     * @param newStatus New order status
     * @returns Observable of operation result
     */
    updateOrderStatus(orderIds: string[], newStatus: OrderStatus): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/status`, {
            orderIds,
            newStatus
        }).pipe(
            catchError(error => this.handleError(error, 'Failed to update order statuses'))
        );
    }

    /**
     * Generate PDF invoices for selected orders
     * @param orders Array of orders
     * @returns Observable of PDF blob
     */
    generateInvoices(orders: Order[]): Observable<Blob> {
        return this.http.post(`${this.apiUrl}/invoices`, {
            orderIds: orders.map(order => order.id)
        }, {
            responseType: 'blob'
        }).pipe(
            catchError(error => this.handleError(error, 'Failed to generate invoices'))
        );
    }

    /**
     * Generate shipping labels for selected orders
     * @param orders Array of orders
     * @returns Observable of PDF blob
     */
    generateShippingLabels(orders: Order[]): Observable<Blob> {
        return this.http.post(`${this.apiUrl}/shipping-labels`, {
            orderIds: orders.map(order => order.id)
        }, {
            responseType: 'blob'
        }).pipe(
            catchError(error => this.handleError(error, 'Failed to generate shipping labels'))
        );
    }

    /**
     * Generate packing slips for selected orders
     * @param orders Array of orders
     * @returns Observable of PDF blob
     */
    generatePackingSlips(orders: Order[]): Observable<Blob> {
        return this.http.post(`${this.apiUrl}/packing-slips`, {
            orderIds: orders.map(order => order.id)
        }, {
            responseType: 'blob'
        }).pipe(
            catchError(error => this.handleError(error, 'Failed to generate packing slips'))
        );
    }

    /**
     * Validate that orders can be processed in bulk
     * @param orders Array of orders
     * @param operation Operation to validate
     * @returns Observable of validation result
     */
    validateBulkOperation(orders: Order[], operation: string): Observable<BulkValidationResult> {
        return this.http.post<BulkValidationResult>(`${this.apiUrl}/validate`, {
            orderIds: orders.map(order => order.id),
            operation
        }).pipe(
            catchError(error => this.handleError(error, 'Failed to validate bulk operation'))
        );
    }

    /**
     * Get status of a long-running bulk operation
     * @param operationId Operation ID
     * @returns Observable of operation status
     */
    getBulkOperationStatus(operationId: string): Observable<BulkOperationStatus> {
        return this.http.get<BulkOperationStatus>(`${this.apiUrl}/status/${operationId}`).pipe(
            catchError(error => this.handleError(error, 'Failed to get operation status'))
        );
    }

    /**
     * Execute a generic bulk operation
     * @param entityType Type of entity (e.g., 'orders', 'products')
     * @param operation Operation name
     * @param ids Array of entity IDs
     * @param params Optional operation parameters
     * @returns Observable of operation result
     */
    executeBulkOperation<T, R>(
        entityType: string,
        operation: string,
        ids: string[],
        params?: T
    ): Observable<R> {
        const endpoint = `${this.apiUrlBase}/${entityType}/bulk/${operation}`;

        return this.http.post<R>(endpoint, {
            ids,
            ...params
        }).pipe(
            catchError(error => this.handleError(error, `Failed to execute bulk ${operation}`))
        );
    }
}