// src/app/features/products/product-list/product-list.component.ts
import { Component, OnInit, OnDestroy, signal, viewChild, inject } from '@angular/core';
import { CommonModule, NgIf } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, Subject, combineLatest } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil, startWith } from 'rxjs/operators';
import { Product } from '../../../shared/models/product.model';
import { ProductActions } from '../../../store/product/product.actions';
import {
    selectAllProducts,
    selectProductsLoading,
    selectProductsError,
    selectProductPagination
} from '../../../store/product/product.selectors';
import { DialogService } from '../../../core/services/dialog.service';
import { MatDialog } from '@angular/material/dialog';
import { MatTableModule } from '@angular/material/table';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { ProductService } from 'src/app/core/services/product.service';
import { ErrorService } from 'src/app/core/services/error.service';
import { MatIconModule } from '@angular/material/icon';
import { StockActions } from 'src/app/store/stock/stock.actions';
import { StockManagementComponent } from '../components/stock-management/stock-management.component';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MatTooltip } from '@angular/material/tooltip';
import { ImagePreviewDialogComponent } from './image-preview-dialog.component';

@Component({
    selector: 'app-product-list',
    templateUrl: './product-list.component.html',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatTableModule,
        MatSortModule,
        MatTooltip,
        NgIf,
        MatPaginatorModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatCheckboxModule,
        MatSelectModule,
        MatProgressSpinner,
        MatIconModule,
        StockManagementComponent,
        RouterModule
    ]
})
export class ProductListComponent implements OnInit, OnDestroy {
    readonly sort = viewChild.required(MatSort);
    readonly paginator = viewChild.required(MatPaginator);

    displayedColumns = ['image', 'name', 'category', 'price', 'stock', 'actions'];
    private readonly destroy$ = new Subject<void>();

    products$: Observable<Product[]>;
    loading$: Observable<boolean>;
    error$: Observable<string | null>;
    pagination$: Observable<any>;

    // Form Controls
    searchControl = new FormControl('');
    categoryFilter = new FormControl('');
    minPriceFilter = new FormControl<number | null>(null);
    maxPriceFilter = new FormControl<number | null>(null);
    inStockFilter = new FormControl(false);

    categories = signal<{ id: string, name: string }[]>([]);

    private readonly store = inject(Store);
    private readonly router = inject(Router);
    private readonly dialogService = inject(DialogService);
    private readonly matDialog = inject(MatDialog);
    private readonly productService = inject(ProductService);
    private readonly errorService = inject(ErrorService);
    constructor() {
        this.products$ = this.store.select(selectAllProducts);
        this.loading$ = this.store.select(selectProductsLoading);
        this.error$ = this.store.select(selectProductsError);
        this.pagination$ = this.store.select(selectProductPagination);
    }
    ngOnInit() {
        this.initializeFilters();
        this.loadCategories();
        this.products$.pipe(takeUntil(this.destroy$)).subscribe(products => {
            products.forEach(product => {
                this.store.dispatch(StockActions.loadStock({ productId: product.id }));
            });
        });
        this.loadProducts();
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private initializeFilters() {
        combineLatest([
            this.searchControl.valueChanges.pipe(startWith('')),
            this.categoryFilter.valueChanges.pipe(startWith('')),
            this.minPriceFilter.valueChanges.pipe(startWith(null)),
            this.maxPriceFilter.valueChanges.pipe(startWith(null)),
            this.inStockFilter.valueChanges.pipe(startWith(false))
        ]).pipe(
            debounceTime(300),
            distinctUntilChanged((prev, curr) => JSON.stringify(prev) === JSON.stringify(curr)),
            takeUntil(this.destroy$)
        ).subscribe(([search, category, minPrice, maxPrice, inStock]) => {
            const paginator = this.paginator();
            const sort = this.sort();
            const filters = {
                search: search ?? '',
                categoryId: category ?? undefined,
                minPrice: minPrice ?? null,
                maxPrice: maxPrice ?? null,
                inStock: inStock || undefined,
                page: paginator?.pageIndex ? paginator.pageIndex + 1 : 1,
                pageSize: this.paginator()?.pageSize || 10,
                sortColumn: sort?.active as keyof Product,
                sortDirection: sort?.direction || undefined
            };

            this.store.dispatch(ProductActions.setFilters({ filters }));
            this.loadProducts(filters);
        });
    }

    private loadCategories() {
        this.productService.getCategories().subscribe({
            next: (categories) => this.categories.set(categories),
            error: (error) => {
                this.errorService.addError({
                    code: '',
                    message: 'Failed to load categories',
                    severity: 'error'
                });
            }
        });
    }

    sortChange(sort: Sort) {
        const filters = {
            sortColumn: sort.active as keyof Product,
            sortDirection: sort.direction || undefined
        };

        this.store.dispatch(ProductActions.setFilters({ filters }));
        this.loadProducts({
            ...this.getCurrentFilters(),
            ...filters
        });
    }

    onPageChange(event: PageEvent) {
        const filters = {
            page: event.pageIndex + 1,
            pageSize: event.pageSize
        };

        this.store.dispatch(ProductActions.setFilters({ filters }));
        this.loadProducts({
            ...this.getCurrentFilters(),
            ...filters
        });
    }

    private getCurrentFilters() {
        const paginator = this.paginator();
        const sort = this.sort();
        return {
            search: this.searchControl.value ?? '',
            categoryId: this.categoryFilter.value ?? undefined,
            minPrice: this.minPriceFilter.value ?? null,
            maxPrice: this.maxPriceFilter.value ?? null,
            inStock: this.inStockFilter.value || undefined,
            page: paginator?.pageIndex ? paginator.pageIndex + 1 : 1,
            pageSize: this.paginator()?.pageSize || 10,
            sortColumn: sort?.active as keyof Product,
            sortDirection: sort?.direction || undefined
        };
    }

    loadProducts(filters = this.getCurrentFilters()) {
        this.store.dispatch(ProductActions.loadProducts({ filters }));
    }

    resetFilters() {
        this.searchControl.reset();
        this.categoryFilter.reset();
        this.minPriceFilter.reset();
        this.maxPriceFilter.reset();
        this.inStockFilter.reset();

        const paginator = this.paginator();
        if (paginator) {
            paginator.pageIndex = 0;
            paginator.pageSize = 10;
        }

        const sort = this.sort();
        if (sort) {
            sort.active = '';
            sort.direction = '';
        }

        this.store.dispatch(ProductActions.resetFilters());
        this.loadProducts({
            page: 1,
            pageSize: 10,
            search: '',
            categoryId: undefined,
            minPrice: null,
            maxPrice: null,
            inStock: undefined,
            sortColumn: 'name' as keyof Product,  // Provide valid default
            sortDirection: undefined
        });
    }

    /**
     * Checks if any filter is currently active
     */
    hasActiveFilters(): boolean {
        return !!(
            this.searchControl.value ||
            this.categoryFilter.value ||
            this.minPriceFilter.value ||
            this.maxPriceFilter.value ||
            this.inStockFilter.value
        );
    }

    /**
     * Returns a formatted string representing the current price filter range
     */
    getPriceFilterLabel(): string {
        const min = this.minPriceFilter.value;
        const max = this.maxPriceFilter.value;

        if (min && max) {
            return `$${min} - $${max}`;
        } else if (min) {
            return `$${min}+`;
        } else if (max) {
            return `Up to $${max}`;
        }

        return '';
    }

    /**
     * Clears only the price-related filters
     */
    clearPriceFilters(): void {
        this.minPriceFilter.reset();
        this.maxPriceFilter.reset();
        this.loadProducts();
    }

    async openImagePreview(imageUrl: string) {
        this.matDialog.open(ImagePreviewDialogComponent, {
            width: '800px',
            data: {
                url: imageUrl,
                alt: 'Product Image'
            },
            panelClass: 'image-preview-dialog'
        });
    }

    async deleteProduct(product: Product) {
        const confirmed = await this.dialogService.confirm(
            `Are you sure you want to delete ${product.name}?`,
            'Delete Product'
        );

        if (confirmed) {
            this.store.dispatch(ProductActions.deleteProduct({ id: product.id }));
        }
    } editProduct(product: Product, event?: Event) {
        // Stop event propagation if event is provided
        if (event) {
            event.stopPropagation();
        }

        // Instead of opening a dialog, navigate to the dynamic product form in edit mode
        this.router.navigate(['/products/edit', product.id]);

        // Store the selected product in the state for potential use
        this.store.dispatch(ProductActions.selectProduct({ product }));
    }
}