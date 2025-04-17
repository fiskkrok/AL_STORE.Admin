// src/app/core/services/order.service.ts
import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { BaseCrudService } from './base-crud.service';
import { PagedResponse } from '../../shared/models/paged-response.model';
import { Order, OrderStatus, PaymentStatus } from '../../shared/models/order.model';

export interface OrderListParams {
    searchTerm?: string;
    status?: OrderStatus;
    fromDate?: string;
    toDate?: string;
    minTotal?: number;
    maxTotal?: number;
    sortBy?: string;
    sortDirection?: 'asc' | 'desc';
    page?: number;
    pageSize?: number;
}

export interface UpdateOrderStatusRequest {
    newStatus: OrderStatus;
}

export interface AddPaymentRequest {
    transactionId: string;
    method: string;
    amount: number;
    currency: string;
    status: PaymentStatus;
}

export interface UpdateShippingRequest {
    carrier: string;
    trackingNumber: string;
    shippingCost: number;
    currency: string;
    estimatedDeliveryDate: string;
}

@Injectable({
    providedIn: 'root'
})
export class OrderService extends BaseCrudService<Order, string, OrderListParams> {
    protected override endpoint = 'orders';

    /**
     * Get paged orders with filtering
     * @param params Order filter parameters
     * @returns Observable of paged orders
     */
    getOrders(params: OrderListParams): Observable<PagedResponse<Order>> {
        return this.getPaged(params).pipe(
            map(response => ({
                ...response,
                items: response.items.map(this.mapOrderFromApi)
            }))
        );
    }

    /**
     * Get a specific order by ID
     * @param id Order ID
     * @returns Observable of order
     */
    override getById(id: string): Observable<Order> {
        return super.getById(id).pipe(
            map(this.mapOrderFromApi)
        );
    }

    /**
     * Update order status
     * @param orderId Order ID
     * @param request Status update request
     * @returns Observable of operation result
     */
    updateOrderStatus(orderId: string, request: UpdateOrderStatusRequest): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${orderId}/status`, request).pipe(
            catchError(error => this.handleError(error, 'Failed to update order status'))
        );
    }

    /**
     * Add payment to an order
     * @param orderId Order ID
     * @param payment Payment details
     * @returns Observable of operation result
     */
    addPayment(orderId: string, payment: AddPaymentRequest): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/${orderId}/payments`, payment).pipe(
            catchError(error => this.handleError(error, 'Failed to add payment'))
        );
    }

    /**
     * Update order shipping information
     * @param orderId Order ID
     * @param shipping Shipping details
     * @returns Observable of operation result
     */
    updateShipping(orderId: string, shipping: UpdateShippingRequest): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${orderId}/shipping`, shipping).pipe(
            catchError(error => this.handleError(error, 'Failed to update shipping information'))
        );
    }

    /**
     * Map order from API response to client model
     * @param order Raw order data
     * @returns Mapped Order object
     */
    private mapOrderFromApi(order: any): Order {
        return {
            id: order.id,
            orderNumber: order.orderNumber,
            customerId: order.customerId,
            status: order.status as OrderStatus,
            subtotal: order.subtotal,
            shippingCost: order.shippingCost,
            tax: order.tax,
            total: order.total,
            currency: order.currency,
            shippingAddress: order.shippingAddress,
            billingAddress: order.billingAddress,
            notes: order.notes,
            cancelledAt: order.cancelledAt,
            cancellationReason: order.cancellationReason,
            items: (order.items || []).map((item: any) => ({
                id: item.id,
                productId: item.productId,
                productName: item.productName,
                sku: item.sku,
                variantId: item.variantId,
                quantity: item.quantity,
                unitPrice: item.unitPrice,
                currency: item.currency,
                total: item.total
            })),
            payment: order.payment ? {
                transactionId: order.payment.transactionId,
                method: order.payment.method,
                amount: order.payment.amount,
                currency: order.payment.currency,
                status: order.payment.status as PaymentStatus,
                processedAt: order.payment.processedAt
            } : undefined,
            paymentStatus: order.paymentStatus as PaymentStatus,
            paymentMethod: order.paymentMethod,
            shippingInfo: order.shippingInfo ? {
                carrier: order.shippingInfo.carrier,
                trackingNumber: order.shippingInfo.trackingNumber,
                estimatedDeliveryDate: order.shippingInfo.estimatedDeliveryDate,
                actualDeliveryDate: order.shippingInfo.actualDeliveryDate
            } : undefined,
            createdAt: order.createdAt,
            createdBy: order.createdBy,
            lastModifiedAt: order.lastModifiedAt,
            lastModifiedBy: order.lastModifiedBy
        };
    }
}