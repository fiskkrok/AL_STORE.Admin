// src/app/features/products/product-list/product-list.component.ts
import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Product } from '../../../shared/models/product.model';
import { ProductService } from '../../../core/services/product.service';
import { CeilPipe } from '../../../shared/pipes/ceil-pipe';
import { ErrorService } from '../../../core/services/error.service';
import { DialogService } from '../../../core/services/dialog.service';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { EditProductDialogComponent } from '../../../shared/components/edit-product-dialog/edit-product-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
interface SortConfig {
    column: keyof Product;
    direction: 'asc' | 'desc';
}

@Component({
    selector: 'app-product-list',
    standalone: true,
    imports: [CommonModule, RouterLink, ReactiveFormsModule, CeilPipe],
    templateUrl: './product-list.component.html',
    styleUrls: ['./product-list.component.scss']
})
export class ProductListComponent implements OnInit {
    // Signals
    products = signal<Product[]>([]);
    totalCount = signal<number>(0);
    page = signal<number>(1);
    pageSize = signal<number>(5);
    loading = signal<boolean>(false);
    categories = signal<{ id: string, name: string }[]>([]);
    sortConfig = signal<SortConfig>({ column: 'name', direction: 'asc' });

    // Form Controls
    searchControl = new FormControl('');
    categoryFilter = new FormControl('');
    minPriceFilter = new FormControl<number | null>(null);
    maxPriceFilter = new FormControl<number | null>(null);
    inStockFilter = new FormControl(false);

    // Computed values
    hasNextPage = computed(() => {
        return this.page() * this.pageSize() < this.totalCount();
    });

    hasPreviousPage = computed(() => {
        return this.page() > 1;
    });

    totalPages = computed(() => {
        return Math.ceil(this.totalCount() / this.pageSize());
    });

    constructor(
        private readonly productService: ProductService,
        private readonly errorService: ErrorService,
        private readonly dialogService: DialogService,
        private readonly router: Router,
        private readonly matDialog: MatDialog,
        private readonly snackBar: MatSnackBar

    ) {
        this.initializeFilters();
    }

    ngOnInit() {
        this.loadCategories();
        this.loadProducts();
    }

    private initializeFilters() {
        // Search with debounce
        this.searchControl.valueChanges.pipe(
            debounceTime(300),
            distinctUntilChanged()
        ).subscribe(() => {
            this.page.set(1); // Reset to first page when searching
            this.loadProducts();
        });

        // Other filters
        this.categoryFilter.valueChanges.subscribe(() => {
            this.page.set(1);
            this.loadProducts();
        });

        this.minPriceFilter.valueChanges.pipe(debounceTime(300))
            .subscribe(() => {
                this.page.set(1);
                this.loadProducts();
            });

        this.maxPriceFilter.valueChanges.pipe(debounceTime(300))
            .subscribe(() => {
                this.page.set(1);
                this.loadProducts();
            });

        this.inStockFilter.valueChanges.subscribe(() => {
            this.page.set(1);
            this.loadProducts();
        });
    }

    private loadCategories() {
        this.productService.getCategories().subscribe({
            next: (categories) => this.categories.set(categories),
            error: (error) => {
                this.errorService.addError({
                    message: 'Failed to load categories',
                    type: 'error'
                });
            }
        });
    }

    loadProducts() {
        this.loading.set(true);

        const filters = {
            page: this.page(),
            pageSize: this.pageSize(),
            search: this.searchControl.value || undefined,
            category: this.categoryFilter.value || undefined,
            minPrice: this.minPriceFilter.value || undefined,
            maxPrice: this.maxPriceFilter.value || undefined,
            inStock: this.inStockFilter.value || undefined,
            sortColumn: this.sortConfig().column,
            sortDirection: this.sortConfig().direction
        };

        this.productService.getProducts(filters).subscribe({
            next: (response) => {
                this.products.set(response.items);
                this.totalCount.set(response.totalCount);
                this.loading.set(false);
            },
            error: (error) => {
                this.errorService.addError({
                    message: 'Failed to load products',
                    type: 'error'
                });
                this.loading.set(false);
            }
        });
    }

    sort(column: keyof Product) {
        const currentSort = this.sortConfig();
        const direction = currentSort.column === column && currentSort.direction === 'asc'
            ? 'desc'
            : 'asc';
        this.sortConfig.set({ column, direction });
        this.loadProducts();
    }

    openEditProductDialog(product: Product) {
        const dialogRef = this.matDialog.open(EditProductDialogComponent, {
            width: '600px',
            data: product
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                this.snackBar.open('Product updated successfully', 'Close', { duration: 3000 });
                this.loadProducts();
            } else if (result === false) {
                this.snackBar.open('Product update failed', 'Close', { duration: 3000 });
            }
        });
    }

    async editProduct(product: Product) {
        this.openEditProductDialog(product);
    }

    async deleteProduct(product: Product) {
        const confirmed = await this.dialogService.confirm(
            `Are you sure you want to delete ${product.name}?`,
            'Delete Product'
        );

        if (confirmed) {
            this.loading.set(true);
            this.productService.deleteProduct(product.id).subscribe({
                next: () => {
                    this.errorService.addError({
                        message: 'Product deleted successfully',
                        type: 'info'
                    });
                    this.loadProducts();
                },
                error: (error) => {
                    this.errorService.addError({
                        message: 'Failed to delete product',
                        type: 'error'
                    });
                    this.loading.set(false);
                }
            });
        }
    }

    previousPage() {
        if (this.hasPreviousPage()) {
            this.page.update(p => p - 1);
            this.loadProducts();
        }
    }

    nextPage() {
        if (this.hasNextPage()) {
            this.page.update(p => p + 1);
            this.loadProducts();
        }
    }

    async openImagePreview(imageUrl: string) {
        await this.dialogService.show({
            title: 'Image Preview',
            message: imageUrl,
            type: 'preview'
        });
    }
}
