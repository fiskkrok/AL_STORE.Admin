import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, tap, catchError, switchMap, shareReplay } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import {
    Product,
    ProductFilters,
    ProductCreateCommand,
    ProductUpdateCommand,
    ProductImage,
    ProductVariant,
    mapProductFromApi,
    mapVariantFromApi
} from '../../shared/models/product.model';
import { Category } from 'src/app/shared/models/category.model';
import { DashboardStats } from './statistics.service';

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

    // Core CRUD Operations
    /**
     * Gets a product by its ID
     */
    getProduct(id: string): Observable<Product> {
        return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
            map(product => mapProductFromApi(product, this.getFullImageUrl.bind(this)))
        );
    }

    /**
     * Gets a list of products based on filter criteria
     */
    getProducts(filters?: ProductFilters): Observable<{
        items: Product[];
        totalCount: number;
        page: number;
        pageSize: number;
        totalPages: number;
    }> {
        return this.http.get<any>(this.apiUrl, { params: filters as any }).pipe(
            map(response => ({
                ...response,
                items: response.items.map((item: any) =>
                    mapProductFromApi(item, this.getFullImageUrl.bind(this)))
            }))
        );
    }

    /**
     * Creates a new product
     */
    createProduct(command: ProductCreateCommand): Observable<Product> {
        // Generate slug if not provided
        if (!command.slug) {
            command.slug = this.generateSlug(command.name);
        }

        return this.http.post<Product>(this.apiUrl, command).pipe(
            map(product => mapProductFromApi(product, this.getFullImageUrl.bind(this))),
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

    /**
     * Updates an existing product
     */
    updateProduct(command: ProductUpdateCommand): Observable<Product> {
        return this.http.put<Product>(`${this.apiUrl}/${command.id}`, command).pipe(
            map(product => mapProductFromApi(product, this.getFullImageUrl.bind(this))),
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

    /**
     * Deletes a product
     */
    deleteProduct(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
            tap(() => {
                const currentProducts = this.productsSubject.value;
                this.productsSubject.next(currentProducts.filter(p => p.id !== id));
            }),
            catchError(this.handleError)
        );
    }

    // Image Operations
    /**
     * Uploads product images
     */
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

    /**
     * Deletes product images
     */
    deleteImages(imageIds: string[]): Observable<void> {
        return this.http.post<void>(
            `${this.apiUrl}/delete-images`,
            { imageIds }
        ).pipe(
            catchError(this.handleError)
        );
    }

    // Category Operations
    /**
     * Gets all product categories
     */
    getCategories(): Observable<Category[]> {
        return this.http.get<Category[]>(this.categoriesUrl).pipe(
            catchError(this.handleError)
        );
    }

    // Stats Operations
    /**
     * Gets product statistics for dashboard
     */
    getStats(): Observable<DashboardStats> {
        return this.http.get<DashboardStats>(`${this.apiUrl}/stats`).pipe(
            catchError(this.handleError)
        );
    }

    // Helper methods
    /**
     * Ensures image URLs are properly formatted with the base URL if needed
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

    /**
     * Generates a URL-friendly slug from a string
     */
    private generateSlug(name: string): string {
        return name
            .toLowerCase()
            .replace(/[^a-z0-9]+/g, '-')
            .replace(/(^-|-$)/g, '');
    }

    /**
     * Standard error handler for HTTP requests
     */
    private handleError(error: any): Observable<never> {
        console.error('An error occurred:', error);
        return throwError(() => error);
    }

    // Observable for components to subscribe to
    get products$(): Observable<Product[]> {
        return this.productsSubject.asObservable();
    }
}
