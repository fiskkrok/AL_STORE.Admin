import { Component, OnInit, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Product } from '../../../shared/models/product.model';
import { ProductService, ProductFilters } from '../../../core/services/product.service';
import { ErrorService } from '../../../core/services/error.service';
import { DialogService } from '../../../core/services/dialog.service';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { MatTableModule, MatTable } from '@angular/material/table';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { EditProductDialogComponent } from 'src/app/shared/components/dialog/edit-product-dialog/edit-product-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
    selector: 'app-product-list',
    standalone: true,
    imports: [
        CommonModule,
        RouterLink,
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
        MatCardModule
    ],
    templateUrl: './product-list.component.html',
    styleUrls: ['./product-list.component.scss']
})
export class ProductListComponent implements OnInit {
    onKeyPress($event: KeyboardEvent) {
        throw new Error('Method not implemented.');
    }
    // Column name to Product property mapping for sorting
    private readonly sortPropertyMap: { [key: string]: keyof Product } = {
        'name': 'name',
        'category': 'category',
        'price': 'price',
        'stock': 'stock'
    };
    @ViewChild(MatSort) sort!: MatSort;
    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatTable) table!: MatTable<Product>;

    // Signals
    products = signal<Product[]>([]);
    totalCount = signal<number>(0);
    page = signal<number>(1);
    pageSize = signal<number>(5);
    loading = signal<boolean>(false);
    categories = signal<{ id: string, name: string }[]>([]);

    // Form Controls
    searchControl = new FormControl('');
    categoryFilter = new FormControl('');
    minPriceFilter = new FormControl<number | null>(null);
    maxPriceFilter = new FormControl<number | null>(null);
    inStockFilter = new FormControl(false);

    // Table configuration
    displayedColumns: string[] = ['image', 'name', 'category', 'price', 'stock', 'actions'];

    constructor(
        private readonly productService: ProductService,
        private readonly errorService: ErrorService,
        private readonly dialogService: DialogService,
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
        this.searchControl.valueChanges.pipe(
            debounceTime(300),
            distinctUntilChanged()
        ).subscribe(() => {
            this.page.set(1);
            this.loadProducts();
        });

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

        const filters: ProductFilters = {
            page: this.page(),
            pageSize: this.pageSize(),
            search: this.searchControl.value || undefined,
            category: this.categoryFilter.value || undefined,
            minPrice: this.minPriceFilter.value || undefined,
            maxPrice: this.maxPriceFilter.value || undefined,
            inStock: this.inStockFilter.value || undefined,
            sortColumn: this.sort?.active as keyof Product | undefined,
            sortDirection: this.sort?.direction || undefined
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

    onPageChange(event: PageEvent) {
        this.pageSize.set(event.pageSize);
        this.page.set(event.pageIndex + 1);
        this.loadProducts();
    }

    editProduct(product: Product) {
        this.productService.getProduct(product.id).subscribe({
            next: (completeProduct) => {
                const dialogRef = this.matDialog.open(EditProductDialogComponent, {
                    width: '600px',
                    data: completeProduct
                });

                dialogRef.afterClosed().subscribe(result => {
                    if (result) {
                        this.snackBar.open('Product updated successfully', 'Close', { duration: 3000 });
                        this.loadProducts();
                    } else if (result === false) {
                        this.snackBar.open('Product update failed', 'Close', { duration: 3000 });
                    }
                });
            },
            error: (error) => {
                this.snackBar.open('Error loading product details', 'Close', { duration: 3000 });
            }
        });
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

    async openImagePreview(imageUrl: string) {
        await this.dialogService.show({
            title: 'Image Preview',
            message: imageUrl,
            type: 'preview'
        });
    }
}