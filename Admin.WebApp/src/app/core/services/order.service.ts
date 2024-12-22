import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResponse } from '../../shared/models/paged-response.model';
import { Order, OrderStatus, PaymentStatus } from '../../shared/models/orders/order.model';

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
export class OrderService {
    private readonly apiUrl = environment.apiUrls.admin.orders;

    constructor(private readonly http: HttpClient) { }

    getOrders(params: OrderListParams): Observable<PagedResponse<Order>> {
        let httpParams = new HttpParams();

        Object.entries(params).forEach(([key, value]) => {
            if (value !== undefined && value !== null) {
                httpParams = httpParams.set(key, value.toString());
            }
        });

        return this.http.get<PagedResponse<Order>>(this.apiUrl, { params: httpParams });
    }

    updateOrderStatus(orderId: string, request: UpdateOrderStatusRequest): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${orderId}/status`, request);
    }

    addPayment(orderId: string, payment: AddPaymentRequest): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/${orderId}/payments`, payment);
    }

    updateShipping(orderId: string, shipping: UpdateShippingRequest): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${orderId}/shipping`, shipping);
    }
}