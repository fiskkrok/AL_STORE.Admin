// src/app/store/stock/stock.actions.ts
import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { BatchStockAdjustment, StockAdjustment, StockItem } from 'src/app/shared/models/stock.model';

export const StockActions = createActionGroup({
    source: 'Stock',
    events: {
        // Load Stock
        'Load Stock': props<{ productId: string }>(),
        'Load Stock Success': props<{ stock: StockItem }>(),
        'Load Stock Failure': props<{ error: string }>(),

        // Adjust Stock
        'Adjust Stock': props<{ adjustment: StockAdjustment }>(),
        'Adjust Stock Success': props<{ stock: StockItem }>(),
        'Adjust Stock Failure': props<{ error: string }>(),

        // Batch Adjust
        'Batch Adjust Stock': props<{ adjustments: BatchStockAdjustment }>(),
        'Batch Adjust Stock Success': props<{ stocks: StockItem[] }>(),
        'Batch Adjust Stock Failure': props<{ error: string }>(),

        // Real-time Updates
        'Stock Updated': props<{ stock: StockItem }>(),
        'Low Stock Alert': props<{ stock: StockItem }>(),
        'Out Of Stock Alert': props<{ stock: StockItem }>(),

        // Load Alerts
        'Load Low Stock Items': emptyProps(),
        'Load Low Stock Items Success': props<{ items: StockItem[] }>(),
        'Load Low Stock Items Failure': props<{ error: string }>(),

        'Load Out Of Stock Items': emptyProps(),
        'Load Out Of Stock Items Success': props<{ items: StockItem[] }>(),
        'Load Out Of Stock Items Failure': props<{ error: string }>()
    }
});