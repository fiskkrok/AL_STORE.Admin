import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, tap, catchError, switchMap, shareReplay } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import {
    Product,
    ProductStatus,
    ProductVisibility,
    ProductImage,
    ProductVariant,
} from '../../shared/models/product.model';
import { PagedResponse } from '../../shared/models/paged-response.model';
import { Category } from 'src/app/shared/models/category.model';
import { DashboardStats } from './statistics.service';

// Enhanced filter options
export interface ProductFilters {
    search?: string;
    categoryId?: string;
    subCategoryId?: string;
    minPrice?: number | null;  // Allow null
    maxPrice?: number | null;  // Allow null
    status?: ProductStatus;
    visibility?: ProductVisibility;
    inStock?: boolean | null;  // Allow null
    tags?: string[];
    page?: number;
    pageSize?: number;
    sortColumn?: keyof Product;
    sortDirection?: 'asc' | 'desc';
}

export interface ProductCreateCommand {
    name: string;
    slug?: string;
    description: string;
    shortDescription?: string;
    sku: string;
    price: number;  // Change to number
    currency: string;  // Add currency field
    compareAtPrice?: number;
    categoryId: string;
    subCategoryId?: string;
    stock: number;
    lowStockThreshold?: number;
    barcode?: string;
    images: ProductImage[];
    status: ProductStatus;
    visibility: ProductVisibility;
    attributes?: Array<{ name: string; value: string; type: string }>;
    tags?: string[];
    seo?: {
        title?: string;
        description?: string;
        keywords?: string[];
    };
    dimensions?: {
        weight: number;
        width: number;
        height: number;
        length: number;
        unit: 'cm' | 'inch';
    };
}

export interface ProductUpdateCommand extends Partial<ProductCreateCommand> {
    id: string;
    newImages?: File[];
    imageIdsToRemove?: string[];
    imageUpdates?: Array<{ id: string; isPrimary?: boolean; sortOrder?: number; alt?: string }>;
}

@Injectable({
    providedIn: 'root'
})
export class ProductService {

    private readonly productsSubject = new BehaviorSubject<Product[]>([]);
    private readonly apiUrl = environment.apiUrls.admin.products;
    private readonly categoriesUrl = environment.apiUrls.admin.categories;
    private readonly blobStorageUrl = environment.azure.blobStorage.containerUrl;
    private readonly productsContainer = environment.azure.blobStorage.productsContainer;

    constructor(
        private readonly http: HttpClient,
        private readonly authService: AuthService
    ) { }

    getProduct(id: string): Observable<Product> {
        return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
            map(product => this.mapProductFromApi(product))
        );
    }

    getProducts(filters?: any): Observable<any> {
        return this.http.get<any>(this.apiUrl, { params: filters }).pipe(
            map(response => ({
                ...response,
                items: response.items.map((item: any) => this.mapProductFromApi(item))
            }))
        );
    }

    /**
     * Ensures image URLs are properly formatted with the base URL if needed
     * @param url The image URL or filename to check and format
     * @returns The fully qualified image URL
     */
    getFullImageUrl(url: string): string {
        if (!url) return '';

        // Check if the URL already contains http:// or https:// protocol
        if (url.startsWith('http://') || url.startsWith('https://')) {
            return url;
        }

        // If it's just a filename, prepend the blob storage URL and container
        return `${this.blobStorageUrl}/${this.productsContainer}/${url}`;
    }

    private mapProductFromApi(product: any): Product {
        return {
            ...product,
            // Map string values to enum values
            status: product.status as ProductStatus,
            visibility: product.visibility as ProductVisibility,
            // Ensure collections are never null and properly format image URLs
            images: (product.images || []).map((img: ProductImage) => ({
                ...img,
                url: this.getFullImageUrl(img.url)
            })),
            attributes: product.attributes || [],
            tags: product.tags || [],
            // Format dates if needed
            createdAt: product.createdAt,
            lastModifiedAt: product.lastModifiedAt,
            variants: Array.isArray(product.variants)
                ? product.variants.map((v: any) => this.mapVariantFromApi(v))
                : []
        };
    }

    private mapVariantFromApi(variant: any): ProductVariant {
        return {
            id: variant.id,
            sku: variant.sku,
            price: variant.price,
            currency: variant.currency || 'USD',
            compareAtPrice: variant.compareAtPrice,
            costPrice: variant.costPrice,
            barcode: variant.barcode,
            stock: variant.stock,
            trackInventory: variant.trackInventory ?? true,
            allowBackorders: variant.allowBackorders ?? false,
            lowStockThreshold: variant.lowStockThreshold,
            sortOrder: variant.sortOrder || 0,
            isLowStock: variant.isLowStock ?? false,
            isOutOfStock: variant.isOutOfStock ?? false,
            attributes: Array.isArray(variant.attributes) ? variant.attributes : [],
            images: Array.isArray(variant.images)
                ? variant.images.map((img: ProductImage) => ({
                    ...img,
                    url: this.getFullImageUrl(img.url)
                }))
                : [],
            productId: variant.productId
        };
    }
    getStats(): Observable<DashboardStats> {
        return this.http.get<DashboardStats>(`${this.apiUrl}/stats`);
    }
    createProduct(command: ProductCreateCommand): Observable<Product> {
        return this.authService.getAccessToken().pipe(
            switchMap(token => {
                if (!token) throw new Error('No authentication token available');

                // Generate slug if not provided
                if (!command.slug) {
                    command.slug = this.generateSlug(command.name);
                }

                return this.http.post<Product>(this.apiUrl, command);
            }),
            tap(product => {
                const currentProducts = this.productsSubject.value;
                this.productsSubject.next([...currentProducts, product]);
            }),
            catchError(error => {
                console.error('Error creating product:', error);
                return throwError(() => new Error('Failed to create product: ' + error.message));
            })
        );
    }

    updateProduct(command: ProductUpdateCommand): Observable<Product> {
        const formData = new FormData();

        // Append basic product data
        Object.entries(command).forEach(([key, value]) => {
            if (value !== undefined && !['newImages', 'imageIdsToRemove', 'imageUpdates'].includes(key)) {
                formData.append(key, typeof value === 'object' ? JSON.stringify(value) : value.toString());
            }
        });

        return this.authService.getAccessToken().pipe(
            switchMap(token => {
                if (!token) throw new Error('No authentication token available');

                // const headers = new HttpHeaders()
                //     .set('Authorization', `Bearer ${token}`)
                //     .set('Content-Type', 'application/json');

                return this.http.put<Product>(
                    `${this.apiUrl}/${command.id}`,
                    command
                );
            }),
            tap(updatedProduct => {
                const currentProducts = this.productsSubject.value;
                const index = currentProducts.findIndex(p => p.id === updatedProduct.id);
                if (index !== -1) {
                    currentProducts[index] = updatedProduct;
                    this.productsSubject.next([...currentProducts]);
                }
            }),
            catchError(this.handleError)
        );
    }

    deleteProduct(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
            tap(() => {
                const currentProducts = this.productsSubject.value;
                this.productsSubject.next(currentProducts.filter(p => p.id !== id));
            }),
            catchError(this.handleError)
        );
    }

    // Image handling
    uploadImages(files: File[]): Observable<ProductImage[]> {
        const formData = new FormData();
        files.forEach(file => formData.append('files', file));

        return this.authService.getAccessToken().pipe(
            switchMap(token => {
                if (!token) throw new Error('No authentication token available');

                const headers = new HttpHeaders()
                    .set('Authorization', `Bearer ${token}`);

                return this.http.post<ProductImage[]>(
                    `${this.apiUrl}/upload-images`,
                    formData,
                    { headers }
                ).pipe(shareReplay(1));
            }),
            catchError(this.handleError)
        );
    }
    deleteImages(imageIds: string[]): Observable<void> {
        return this.http.post<void>(
            `${this.apiUrl}/delete-images`,
            { imageIds }
        );
    }
    // Category management
    getCategories(): Observable<Category[]> {
        return this.http.get<Category[]>(this.categoriesUrl).pipe(
            catchError(this.handleError)
        );
    }

    // Variant management
    addVariant(productId: string, variant: Omit<ProductVariant, 'id'>): Observable<ProductVariant> {
        return this.http.post<ProductVariant>(
            `${this.apiUrl}/${productId}/variants`,
            variant
        ).pipe(
            catchError(this.handleError)
        );
    }

    updateVariant(productId: string, variantId: string, updates: Partial<ProductVariant>): Observable<ProductVariant> {
        return this.http.put<ProductVariant>(
            `${this.apiUrl}/${productId}/variants/${variantId}`,
            updates
        ).pipe(
            catchError(this.handleError)
        );
    }

    // Utility methods
    private generateSlug(name: string): string {
        return name
            .toLowerCase()
            .replace(/[^a-z0-9]+/g, '-')
            .replace(/(^-|-$)/g, '');
    }

    private handleError(error: any): Observable<never> {
        console.error('An error occurred:', error);
        return throwError(() => error);
    }

    // Observable for components to subscribe to
    get products$(): Observable<Product[]> {
        return this.productsSubject.asObservable();
    }
}
