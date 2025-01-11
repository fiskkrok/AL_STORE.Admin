// src/app/store/order/order.state.ts
import { Order } from 'src/app/shared/models/order.model';
import { OrderListParams } from '../../core/services/order.service';

export interface OrderState {
    orders: Order[];
    selectedOrder: Order | null;
    loading: boolean;
    error: string | null;
    filters: OrderListParams;
    pagination: {
        currentPage: number;
        pageSize: number;
        totalItems: number;
    };
}

export const initialOrderState: OrderState = {
    orders: [],
    selectedOrder: null,
    loading: false,
    error: null,
    filters: {
        page: 1,
        pageSize: 10,
        sortBy: 'createdAt',
        sortDirection: 'desc'
    },
    pagination: {
        currentPage: 1,
        pageSize: 10,
        totalItems: 0
    }
};