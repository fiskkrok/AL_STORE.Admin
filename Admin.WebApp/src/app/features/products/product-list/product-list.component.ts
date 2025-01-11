// src/app/features/products/product-list/product-list.component.ts
import { Component, OnInit, OnDestroy, ViewChild, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
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
import { EditProductDialogComponent } from '../../../shared/components/dialog/edit-product-dialog/edit-product-dialog.component';
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

@Component({
    selector: 'app-product-list',
    templateUrl: './product-list.component.html',
    styleUrls: ['./product-list.component.scss'],
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatTableModule,
        MatSortModule,
        MatPaginatorModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatCheckboxModule,
        MatSelectModule,
        MatIconModule,
        StockManagementComponent
    ]
})
export class ProductListComponent implements OnInit, OnDestroy {
    @ViewChild(MatSort) sort!: MatSort;
    @ViewChild(MatPaginator) paginator!: MatPaginator;

    displayedColumns = ['image', 'name', 'category', 'price', 'stock', 'actions'];
    private destroy$ = new Subject<void>();

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

    constructor(
        private readonly store: Store,
        private readonly dialogService: DialogService,
        private readonly matDialog: MatDialog,
        private readonly productService: ProductService,
        private readonly errorService: ErrorService
    ) {
        this.products$ = this.store.select(selectAllProducts);
        this.loading$ = this.store.select(selectProductsLoading);
        this.error$ = this.store.select(selectProductsError);
        this.pagination$ = this.store.select(selectProductPagination);
    }

    ngOnInit() {
        this.initializeFilters();
        this.loadCategories();
        this.loadProducts();
        this.products$.pipe(takeUntil(this.destroy$)).subscribe(products => {
            products.forEach(product => {
                this.store.dispatch(StockActions.loadStock({ productId: product.id }));
            });
        });
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
            const filters = {
                search: search || '',
                categoryId: category || undefined,
                minPrice: minPrice || undefined,
                maxPrice: maxPrice || undefined,
                inStock: inStock || undefined,
                page: this.paginator?.pageIndex ? this.paginator.pageIndex + 1 : 1,
                pageSize: this.paginator?.pageSize || 10,
                sortColumn: this.sort?.active as keyof Product,
                sortDirection: this.sort?.direction || undefined
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
        return {
            search: this.searchControl.value || '',
            categoryId: this.categoryFilter.value || undefined,
            minPrice: this.minPriceFilter.value || undefined,
            maxPrice: this.maxPriceFilter.value || undefined,
            inStock: this.inStockFilter.value || undefined,
            page: this.paginator?.pageIndex ? this.paginator.pageIndex + 1 : 1,
            pageSize: this.paginator?.pageSize || 10,
            sortColumn: this.sort?.active as keyof Product,
            sortDirection: this.sort?.direction || undefined
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

        if (this.paginator) {
            this.paginator.pageIndex = 0;
            this.paginator.pageSize = 10;
        }

        if (this.sort) {
            this.sort.active = '';
            this.sort.direction = '';
        }

        this.store.dispatch(ProductActions.resetFilters());
        this.loadProducts({
            page: 1,
            pageSize: 10,
            search: '',
            categoryId: undefined,
            minPrice: undefined,
            maxPrice: undefined,
            inStock: undefined,
            sortColumn: 'name' as keyof Product,  // Provide valid default
            sortDirection: undefined
        });
    }

    async openImagePreview(imageUrl: string) {
        await this.dialogService.show({
            title: 'Image Preview',
            message: imageUrl,
            type: 'preview'
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
    }

    editProduct(product: Product) {
        this.store.dispatch(ProductActions.selectProduct({ product }));

        const dialogRef = this.matDialog.open(EditProductDialogComponent, {
            width: '600px',
            data: product
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                this.store.dispatch(ProductActions.updateProduct({
                    id: product.id,
                    product: result
                }));
            }
            this.store.dispatch(ProductActions.clearSelectedProduct());
        });
    }
}