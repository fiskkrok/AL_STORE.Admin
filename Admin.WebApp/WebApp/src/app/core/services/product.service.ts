// src/app/core/services/product.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap, catchError, throwError, switchMap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Product } from "../../shared/models/product.model";
import { PagedResponse } from '../../shared/models/paged-response.model';
import { AuthService } from './auth.service';

export interface ProductFilters {
    category?: string;
    search?: string;
    minPrice?: number;
    maxPrice?: number;
    inStock?: boolean;
    page?: number;
    pageSize?: number;
    sortColumn?: keyof Product;
    sortDirection?: 'asc' | 'desc';
}

@Injectable({
    providedIn: 'root'
})
export class ProductService {
    private readonly productsSubject = new BehaviorSubject<Product[]>([]);
    private readonly apiUrl = environment.apiUrls.admin.products;
    private readonly apiUrlcategories = environment.apiUrls.admin.categories;
    private readonly blobStorageUrl = environment.azure.blobStorage.containerUrl;
    private readonly productsContainer = environment.azure.blobStorage.productsContainer;

    constructor(
        private readonly http: HttpClient,
        private readonly authService: AuthService
    ) { }

    private getAuthHeaders(): Observable<HttpHeaders> {
        return this.authService.getAccessToken().pipe(
            map(token => new HttpHeaders({
                'Authorization': `Bearer ${token}`
            }))
        );
    }

    getProducts(filters?: ProductFilters): Observable<PagedResponse<Product>> {
        let params = new HttpParams();

        if (filters) {
            Object.keys(filters).forEach(key => {
                const value = filters[key as keyof ProductFilters];
                if (value !== undefined && value !== null) {
                    params = params.set(key, value.toString());
                }
            });
        }

        return this.http.get<PagedResponse<Product>>(
            this.apiUrl,
            { params }
        ).pipe(
            tap(response => this.productsSubject.next(response.items)),
            catchError(this.handleError)
        );
    }

    getProduct(id: string): Observable<Product> {
        return this.http.get<Product>(
            `${this.apiUrl}/${id}`
        ).pipe(
            catchError(this.handleError)
        );
    }

    addProduct(product: Omit<Product, 'id'>): Observable<Product> {
        return this.http.post<Product>(
            this.apiUrl,
            product
        ).pipe(
            tap(newProduct => {
                const currentProducts = this.productsSubject.value;
                this.productsSubject.next([...currentProducts, newProduct]);
            }),
            catchError(this.handleError)
        );
    }

    updateProduct(id: string, updateData: {
        name: string;
        description: string;
        price: number;
        currency: string;
        categoryId: string;
        subCategoryId?: string;
        newImages?: File[];
        imageIdsToRemove?: string[];
    }): Observable<void> {
        const formData = new FormData();

        // Add basic product data
        formData.append('id', id);
        formData.append('name', updateData.name);
        formData.append('description', updateData.description);
        formData.append('price', updateData.price.toString());
        formData.append('currency', updateData.currency);
        formData.append('categoryId', updateData.categoryId);

        if (updateData.subCategoryId) {
            formData.append('subCategoryId', updateData.subCategoryId);
        }

        // Add new images if any
        if (updateData.newImages?.length) {
            updateData.newImages.forEach(file => {
                formData.append('newImages', file);
            });
        }

        // Add image IDs to remove if any
        if (updateData.imageIdsToRemove?.length) {
            updateData.imageIdsToRemove.forEach(imageId => {
                formData.append('imageIdsToRemove', imageId);
            });
        }

        return this.getAuthHeaders().pipe(
            switchMap(headers => this.http.put<any>(
                `${this.apiUrl}/${id}`,
                formData,
                { headers }
            )),
            catchError(this.handleError)
        );
    }

    deleteProduct(id: string): Observable<void> {
        return this.http.delete<void>(
            `${this.apiUrl}/${id}`
        ).pipe(
            tap(() => {
                const currentProducts = this.productsSubject.value;
                this.productsSubject.next(currentProducts.filter(p => p.id !== id));
            }),
            catchError(this.handleError)
        );
    }

    uploadImages(files: File[]): Observable<string[]> {
        const formData = new FormData();
        files.forEach(file => formData.append('files', file));

        return this.http.post<string[]>(
            `${this.apiUrl}/upload-images`,
            formData
        ).pipe(
            map(fileNames => fileNames.map(fileName =>
                `${this.blobStorageUrl}/${this.productsContainer}/${fileName}`
            )),
            catchError(this.handleError)
        );
    }

    getCategories(): Observable<{ id: string, name: string, description: string }[]> {
        return this.http.get<{ id: string, name: string, description: string }[]>(
            `${this.apiUrlcategories}`
        ).pipe(
            catchError(this.handleError)
        );
    }

    private handleError(error: any): Observable<never> {
        console.error('An error occurred:', error);

        if (error.status === 401) {
            // Unauthorized - redirect to login or refresh token
            this.authService.login();
            return throwError(() => new Error('Unauthorized'));
        }

        return throwError(() => error);
    }

    get products$(): Observable<Product[]> {
        return this.productsSubject.asObservable();
    }
}


