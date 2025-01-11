// src/app/features/products/components/stock-management/stock-management.component.ts
import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { Store } from '@ngrx/store';
import { Product } from 'src/app/shared/models/product.model';
import { StockItem } from 'src/app/shared/models/stock.model';
import { Observable } from 'rxjs';
import { selectStockForProduct } from 'src/app/store/stock/stock.selectors';

@Component({
    selector: 'app-stock-management',
    standalone: true,
    imports: [CommonModule, MatButtonModule, MatDialogModule, MatIconModule],
    template: `
        <div class="stock-info">
            @if (stock$ | async; as stock) {
                <div class="stock-status" [ngClass]="{
                    'text-red-500': stock.isOutOfStock,
                    'text-yellow-500': stock.isLowStock && !stock.isOutOfStock,
                    'text-green-500': !stock.isLowStock && !stock.isOutOfStock
                }">
                    <span class="font-bold">{{ stock.availableStock }}</span> available
                    @if (stock.reservedStock > 0) {
                        ({{ stock.reservedStock }} reserved)
                    }
                </div>

                @if (stock.isLowStock) {
                    <div class="text-yellow-500">
                        <mat-icon>warning</mat-icon>
                        Low Stock
                    </div>
                }

                <button mat-icon-button (click)="openStockAdjustment()">
                    <mat-icon>edit</mat-icon>
                </button>
            }
        </div>
    `,
    styles: [`
        .stock-info {
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        .stock-status {
            display: flex;
            align-items: center;
            gap: 0.25rem;
        }
    `]
})
export class StockManagementComponent {
    @Input() product!: Product;
    stock$: Observable<StockItem | undefined> | undefined;

    constructor(private readonly store: Store) { }

    ngOnInit() {
        this.stock$ = this.store.select(selectStockForProduct(this.product?.id));
    }

    openStockAdjustment() {
        // TODO: Implement stock adjustment dialog
    }
}

