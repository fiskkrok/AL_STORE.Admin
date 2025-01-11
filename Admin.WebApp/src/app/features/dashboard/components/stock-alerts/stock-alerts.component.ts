// src/app/features/dashboard/components/stock-alerts/stock-alerts.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { StockItem } from 'src/app/shared/models/stock.model';
import { StockActions } from '../../../../store/stock/stock.actions';
import { selectLowStockAlerts, selectOutOfStockAlerts } from '../../../../store/stock/stock.selectors';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'app-stock-alerts',
    standalone: true,
    imports: [
        CommonModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        RouterModule
    ],
    template: `
        <div class="stock-alerts-container">
            <h2>Stock Alerts</h2>

            <div class="alerts-grid">
                <!-- Out of Stock Alerts -->
                <mat-card class="alert-card out-of-stock">
                    <mat-card-header>
                        <mat-card-title>
                            <mat-icon>warning</mat-icon>
                            Out of Stock Items
                        </mat-card-title>
                    </mat-card-header>
                    <mat-card-content>
                        @if (outOfStockAlerts$ | async; as alerts) {
                            @if (alerts.length === 0) {
                                <p class="no-alerts">No out of stock items</p>
                            } @else {
                                <ul class="alerts-list">
                                    @for (alert of alerts; track alert.productId) {
                                        <li>
                                            <span class="product-name">{{ alert.productName }}</span>
                                            <button mat-button color="primary" 
                                                    [routerLink]="['/products', alert.productId]">
                                                View Product
                                            </button>
                                        </li>
                                    }
                                </ul>
                            }
                        }
                    </mat-card-content>
                </mat-card>

                <!-- Low Stock Alerts -->
                <mat-card class="alert-card low-stock">
                    <mat-card-header>
                        <mat-card-title>
                            <mat-icon>info</mat-icon>
                            Low Stock Items
                        </mat-card-title>
                    </mat-card-header>
                    <mat-card-content>
                        @if (lowStockAlerts$ | async; as alerts) {
                            @if (alerts.length === 0) {
                                <p class="no-alerts">No low stock items</p>
                            } @else {
                                <ul class="alerts-list">
                                    @for (alert of alerts; track alert.productId) {
                                        <li>
                                            <div class="alert-info">
                                                <span class="product-name">{{ alert.productName }}</span>
                                                <span class="stock-count">
                                                    {{ alert.availableStock }} remaining
                                                </span>
                                            </div>
                                            <button mat-button color="primary" 
                                                    [routerLink]="['/products', alert.productId]">
                                                View Product
                                            </button>
                                        </li>
                                    }
                                </ul>
                            }
                        }
                    </mat-card-content>
                </mat-card>
            </div>
        </div>
    `,
    styles: [`
        .stock-alerts-container {
            padding: 1rem;

            h2 {
                margin-bottom: 1rem;
                color: var(--text-primary);
            }
        }

        .alerts-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 1rem;
        }

        .alert-card {
            background-color: var(--bg-secondary);

            &.out-of-stock {
                border-left: 4px solid var(--error);
            }

            &.low-stock {
                border-left: 4px solid var(--warning);
            }

            mat-card-title {
                display: flex;
                align-items: center;
                gap: 0.5rem;
                color: var(--text-primary);

                mat-icon {
                    width: 24px;
                    height: 24px;
                }
            }
        }

        .alerts-list {
            list-style: none;
            padding: 0;
            margin: 0;

            li {
                display: flex;
                justify-content: space-between;
                align-items: center;
                padding: 0.5rem 0;
                border-bottom: 1px solid var(--border);

                &:last-child {
                    border-bottom: none;
                }
            }
        }

        .alert-info {
            display: flex;
            flex-direction: column;
            gap: 0.25rem;

            .product-name {
                color: var(--text-primary);
                font-weight: 500;
            }

            .stock-count {
                color: var(--text-secondary);
                font-size: 0.875rem;
            }
        }

        .no-alerts {
            color: var(--text-secondary);
            font-style: italic;
            margin: 1rem 0;
        }
    `]
})
export class StockAlertsComponent implements OnInit {
    lowStockAlerts$: Observable<StockItem[]>;
    outOfStockAlerts$: Observable<StockItem[]>;

    constructor(private readonly store: Store) {
        this.lowStockAlerts$ = this.store.select(selectLowStockAlerts);
        this.outOfStockAlerts$ = this.store.select(selectOutOfStockAlerts);
    }

    ngOnInit() {
        // Load alerts
        this.store.dispatch(StockActions.loadLowStockItems());
        this.store.dispatch(StockActions.loadOutOfStockItems());
    }
}