// src/app/features/statistics/statistics.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { MatCardModule } from '@angular/material/card';
import { ProductService } from '../../core/services/product.service';

export interface DashboardStats {
    totalProducts: number;
    lowStockCount: number;
    totalCategories: number;
    activeProductCount: number;
}

@Component({
    selector: 'app-statistics',
    templateUrl: './statistics.component.html',
    styleUrls: ['./statistics.component.scss'],
    standalone: true,
    imports: [
        CommonModule,
        MatCardModule
    ]
})
export class StatisticsComponent implements OnInit {
    stats$: Observable<DashboardStats>;

    constructor(
        private readonly productService: ProductService,
        private readonly store: Store
    ) {
        // Initialize with loading state
        this.stats$ = this.productService.getStats().pipe(
            map(stats => ({
                totalProducts: stats.totalProducts,
                lowStockCount: stats.lowStockCount,
                totalCategories: stats.totalCategories,
                activeProductCount: stats.activeProductCount
            }))
        );
    }

    ngOnInit() {

    }
}