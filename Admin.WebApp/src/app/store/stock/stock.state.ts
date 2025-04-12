// src/app/store/stock/stock.state.ts
import { StockItem } from '../../shared/models/stock.model';

export interface StockState {
    items: Record<string, StockItem>;
    loading: boolean;
    error: string | null;
    lowStockAlerts: StockItem[];
    outOfStockAlerts: StockItem[];
}

export const initialStockState: StockState = {
    items: {},
    loading: false,
    error: null,
    lowStockAlerts: [],
    outOfStockAlerts: []
};