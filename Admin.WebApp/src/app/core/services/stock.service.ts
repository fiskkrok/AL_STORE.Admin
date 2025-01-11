// src/app/core/services/stock.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import * as signalR from '@microsoft/signalr';
import { Store } from '@ngrx/store';
import { StockActions } from '../../store/stock/stock.actions';
import { BatchStockAdjustment, StockAdjustment, StockItem } from 'src/app/shared/models/stock.model';

@Injectable({
    providedIn: 'root'
})
export class StockService {
    private readonly apiUrl = environment.apiUrls.admin.products;
    private hubConnection: signalR.HubConnection | undefined;

    constructor(
        private readonly http: HttpClient,
        private readonly store: Store
    ) {
        this.initializeSignalR();
    }

    private initializeSignalR() {
        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(environment.signalR.product)
            .withAutomaticReconnect()
            .build();

        this.hubConnection.start().catch(err => console.error('Error starting SignalR:', err));

        this.hubConnection.on('StockUpdated', (stock: StockItem) => {
            this.store.dispatch(StockActions.stockUpdated({ stock }));
        });

        this.hubConnection.on('LowStockAlert', (stock: StockItem) => {
            this.store.dispatch(StockActions.lowStockAlert({ stock }));
        });
    }

    getStockLevel(productId: string): Observable<StockItem> {
        return this.http.get<StockItem>(`${this.apiUrl}/${productId}`);
    }

    adjustStock(adjustment: StockAdjustment): Observable<void> {
        return this.http.post<void>(
            `${this.apiUrl}/${adjustment.productId}/adjust`,
            adjustment
        );
    }

    batchAdjustStock(adjustments: BatchStockAdjustment): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/batch-adjust`, adjustments);
    }

    getLowStockItems(): Observable<StockItem[]> {
        return this.http.get<StockItem[]>(`${this.apiUrl}/low-stock`);
    }

    getOutOfStockItems(): Observable<StockItem[]> {
        return this.http.get<StockItem[]>(`${this.apiUrl}/out-of-stock`);
    }

    // Clean up SignalR connection
    ngOnDestroy() {
        if (this.hubConnection) {
            this.hubConnection.stop();
        }
    }
}