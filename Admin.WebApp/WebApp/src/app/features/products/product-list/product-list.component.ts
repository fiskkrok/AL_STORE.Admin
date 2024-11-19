import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { Product } from '../../../shared/models/product.model';
import { ProductService } from '../../../core/services/product.service';
import { CeilPipe } from '../../../shared/pipes/ceil-pipe';
import { ErrorService } from '../../../core/services/error.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
    selector: 'app-product-list',
    standalone: true,
    imports: [CommonModule, CeilPipe, RouterLink],
    templateUrl: 'product-list.component.html',
    styleUrls: ['product-list.component.scss']
})
export class ProductListComponent implements OnInit {
    products = signal<Product[]>([]);
    totalCount = signal<number>(0);
    page = signal<number>(1);
    pageSize = signal<number>(5);
    loading = signal<boolean>(false);

    // Computed values for pagination
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
        private readonly router: Router
    ) { }

    ngOnInit() {
        this.loadProducts();
    }

    loadProducts() {
        this.loading.set(true);
        this.productService.getProducts({
            page: this.page(),
            pageSize: this.pageSize()
        }).subscribe({
            next: (response) => {
                this.products.set(response.items);
                this.totalCount.set(response.totalCount);
                this.loading.set(false);
            },
            error: (error) => {
                this.errorService.addError({
                    message: 'Failed to load products. Please try again later.',
                    type: 'error'
                });
                this.loading.set(false);
            }
        });
    }

    editProduct(product: Product) {
        this.router.navigate(['/products/edit', product.id]);
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
                    this.loadProducts(); // Reload the list
                },
                error: (error) => {
                    this.errorService.addError({
                        message: 'Failed to delete product. Please try again later.',
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

    // Add sorting functionality
    sortProducts(column: keyof Product) {
        // Implementation coming in next phase
    }

    // Add filtering functionality
    filterProducts(filters: any) {
        // Implementation coming in next phase
    }

    // Add search functionality
    searchProducts(term: string) {
        // Implementation coming in next phase
    }
}