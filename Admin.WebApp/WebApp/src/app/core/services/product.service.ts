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
    Category,
    ProductVariant,
    Money
} from '../../shared/models/product.model';
import { PagedResponse } from '../../shared/models/paged-response.model';

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
    price: Money;
    compareAtPrice?: Money;
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

    // Main CRUD operations
    getProducts(filters?: ProductFilters): Observable<PagedResponse<Product>> {
        let params = new HttpParams();

        if (filters) {
            Object.entries(filters).forEach(([key, value]) => {
                if (value !== undefined && value !== null) {
                    if (Array.isArray(value)) {
                        value.forEach(v => params = params.append(key, v));
                    } else {
                        params = params.set(key, value.toString());
                    }
                }
            });
        }

        return this.http.get<PagedResponse<Product>>(this.apiUrl, { params }).pipe(
            tap(response => this.productsSubject.next(response.items)),
            catchError(this.handleError)
        );
    }

    getProduct(id: string): Observable<Product> {
        return this.http.get<Product>(`${this.apiUrl}/${id}`).pipe(
            catchError(this.handleError)
        );
    }

    createProduct(command: ProductCreateCommand): Observable<Product> {
        return this.authService.getAccessToken().pipe(
            switchMap(token => {
                if (!token) throw new Error('No authentication token available');

                const headers = new HttpHeaders()
                    .set('Authorization', `Bearer ${token}`)
                    .set('Content-Type', 'application/json');

                // Generate slug if not provided
                if (!command.slug) {
                    command.slug = this.generateSlug(command.name);
                }

                return this.http.post<Product>(this.apiUrl, command, { headers });
            }),
            tap(product => {
                const currentProducts = this.productsSubject.value;
                this.productsSubject.next([...currentProducts, product]);
            }),
            catchError(this.handleError)
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

        // Handle image updates
        if (command.newImages?.length) {
            command.newImages.forEach(file => formData.append('newImages', file));
        }

        if (command.imageIdsToRemove?.length) {
            command.imageIdsToRemove.forEach(id => formData.append('imageIdsToRemove', id));
        }

        if (command.imageUpdates?.length) {
            formData.append('imageUpdates', JSON.stringify(command.imageUpdates));
        }

        return this.authService.getAccessToken().pipe(
            switchMap(token => {
                if (!token) throw new Error('No authentication token available');

                const headers = new HttpHeaders()
                    .set('Authorization', `Bearer ${token}`);

                return this.http.put<Product>(`${this.apiUrl}/${command.id}`, formData, { headers });
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