// src/app/core/services/stock.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import * as signalR from '@microsoft/signalr';
import { Store } from '@ngrx/store';
import { StockActions } from '../../store/stock/stock.actions';
import { BatchStockAdjustment, StockAdjustment, StockItem } from 'src/app/shared/models/stock.model';
import { AuthService } from './auth.service';

@Injectable({
    providedIn: 'root'
})
export class StockService {
    private readonly apiUrl = environment.apiUrls.admin.stock;

    constructor(private http: HttpClient) { }

    getStockItem(productId: string): Observable<StockItem> {
        return this.http.get<any>(`${this.apiUrl}/stock/${productId}`).pipe(
            map(this.mapStockItemFromApi)
        );
    }

    getLowStockItems(): Observable<StockItem[]> {
        return this.http.get<any[]>(`${this.apiUrl}/stock/low-stock`).pipe(
            map(items => items.map(this.mapStockItemFromApi))
        );
    }

    getOutOfStockItems(): Observable<StockItem[]> {
        return this.http.get<any[]>(`${this.apiUrl}/stock/out-of-stock`).pipe(
            map(items => items.map(this.mapStockItemFromApi))
        );
    }

    adjustStock(adjustment: StockAdjustment): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/stock/${adjustment.productId}/adjust`, adjustment);
    }

    batchAdjustStock(adjustments: BatchStockAdjustment): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/stock/batch-adjust`, adjustments);
    }

    private mapStockItemFromApi(item: any): StockItem {
        return {
            id: item.id,
            productId: item.productId,
            productName: item.productName || 'Unknown Product',
            currentStock: item.currentStock,
            reservedStock: item.reservedStock,
            availableStock: item.availableStock,
            lowStockThreshold: item.lowStockThreshold,
            trackInventory: item.trackInventory,
            isLowStock: item.isLowStock,
            isOutOfStock: item.isOutOfStock,
            reservations: item.reservations?.map((r: any) => ({
                id: r.id,
                orderId: r.orderId,
                quantity: r.quantity,
                status: r.status,
                expiresAt: r.expiresAt,
                confirmedAt: r.confirmedAt,
                cancelledAt: r.cancelledAt
            })) || []
        };
    }
}