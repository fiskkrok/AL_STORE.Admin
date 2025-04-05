// src/app/features/products/components/stock-management/stock-management.component.ts
import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Store } from '@ngrx/store';
import { Product } from 'src/app/shared/models/product.model';
import { StockItem } from 'src/app/shared/models/stock.model';
import { Observable } from 'rxjs';
import { selectStockForProduct } from 'src/app/store/stock/stock.selectors';
import { StockAdjustmentDialogComponent } from './stock-adjustment-dialog.component';

@Component({
    selector: 'app-stock-management',
    standalone: true,
    imports: [
        CommonModule,
        MatButtonModule,
        MatDialogModule,
        MatIconModule,
        MatTooltipModule
    ],
    template: `
        <div class="stock-info">
            @if (stock$ | async; as stock) {
                <div class="flex items-center space-x-3">
                    <div class="flex items-center">
                        <!-- Stock status indicator -->
                        <div [ngClass]="{
                            'bg-emerald-100 dark:bg-emerald-900 text-emerald-600 dark:text-emerald-400': !stock.isLowStock && !stock.isOutOfStock,
                            'bg-amber-100 dark:bg-amber-900 text-amber-600 dark:text-amber-400': stock.isLowStock && !stock.isOutOfStock,
                            'bg-rose-100 dark:bg-rose-900 text-rose-600 dark:text-rose-400': stock.isOutOfStock
                        }" class="px-2 py-1 rounded-full text-sm">
                            <span class="font-medium">{{ stock.availableStock }}</span> available
                            @if (stock.reservedStock > 0) {
                                <span class="text-xs ml-1">({{ stock.reservedStock }} reserved)</span>
                            }
                        </div>
                    </div>

                    <!-- Low stock warning -->
                    @if (stock.isLowStock) {
                        <div class="flex items-center text-amber-500 dark:text-amber-400">
                            <mat-icon class="text-sm h-5 w-5 mr-1">warning</mat-icon>
                            <span class="text-xs">Low Stock</span>
                        </div>
                    }

                    <!-- Stock adjustment button -->
                    <button 
                        mat-icon-button 
                        class="text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-full p-1 transition-colors"
                        matTooltip="Adjust Stock"
                        (click)="openStockAdjustment($event)">
                        <mat-icon class="text-sm h-5 w-5 icon-success">edit</mat-icon>
                    </button>
                </div>
            } @else {
                <div class="text-sm text-slate-500 dark:text-slate-400">
                    Loading stock...
                </div>
            }
        </div>
    `,
    styles: []
})
export class StockManagementComponent {
    @Input() product!: Product;
    stock$: Observable<StockItem | undefined> | undefined;

    constructor(
        private readonly store: Store,
        private readonly dialog: MatDialog
    ) { }

    ngOnInit() {
        this.stock$ = this.store.select(selectStockForProduct(this.product?.id));
    }

    openStockAdjustment(event: Event) {
        // Stop event propagation to prevent row click handler from firing
        event.stopPropagation();

        if (this.stock$) {
            this.stock$.subscribe(stock => {
                if (stock) {
                    const dialogRef = this.dialog.open(StockAdjustmentDialogComponent, {
                        width: '400px',
                        data: {
                            productId: this.product.id,
                            productName: this.product.name,
                            currentStock: stock.availableStock
                        }
                    });
                }
            }).unsubscribe();
        }
    }
}