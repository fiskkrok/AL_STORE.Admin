// src/app/shared/models/stock.model.ts
export interface StockItem {
    id: string;
    productId: string;
    productName: string;
    currentStock: number;
    reservedStock: number;
    availableStock: number;
    lowStockThreshold: number;
    trackInventory: boolean;
    isLowStock: boolean;
    isOutOfStock: boolean;
    reservations: StockReservation[];
}

export interface StockReservation {
    id: string;
    orderId: string;
    quantity: number;
    status: 'Pending' | 'Confirmed' | 'Cancelled';
    expiresAt: string; // ISO date string
    confirmedAt?: string; // ISO date string
    cancelledAt?: string; // ISO date string
}

export interface StockAdjustment {
    productId: string;
    adjustment: number;
    reason?: string;
}

export interface BatchStockAdjustment {
    adjustments: StockAdjustment[];
    reason: string;
}