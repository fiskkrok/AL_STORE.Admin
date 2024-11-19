import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap, catchError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Product } from "../../shared/models/product.model";
import { PagedResponse } from '../../shared/models/paged-response.model';

export interface ProductFilters {
    category?: string;
    search?: string;
    minPrice?: number;
    maxPrice?: number;
    inStock?: boolean;
    page?: number;
    pageSize?: number;
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

    constructor(private readonly http: HttpClient) { }

    // Get products with filtering and pagination
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

        return this.http.get<PagedResponse<Product>>(this.apiUrl, { params }).pipe(
            tap(response => this.productsSubject.next(response.items)),
            catchError(this.handleError)
        );
    }

    // Get single product by ID
    getProduct(id: string): Observable<Product> {
        return this.http.get<Product>(`${this.apiUrl}/${id}`).pipe(
            catchError(this.handleError)
        );
    }

    // Add new product
    addProduct(product: Omit<Product, 'id'>): Observable<Product> {
        return this.http.post<Product>(this.apiUrl, product).pipe(
            tap(newProduct => {
                const currentProducts = this.productsSubject.value;
                this.productsSubject.next([...currentProducts, newProduct]);
            }),
            catchError(this.handleError)
        );
    }

    // Update existing product
    updateProduct(id: string, product: Partial<Product>): Observable<Product> {
        return this.http.put<Product>(`${this.apiUrl}/${id}`, product).pipe(
            tap(updatedProduct => {
                const currentProducts = this.productsSubject.value;
                const index = currentProducts.findIndex(p => p.id === id);
                if (index !== -1) {
                    currentProducts[index] = updatedProduct;
                    this.productsSubject.next([...currentProducts]);
                }
            }),
            catchError(this.handleError)
        );
    }

    // Delete product
    deleteProduct(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
            tap(() => {
                const currentProducts = this.productsSubject.value;
                this.productsSubject.next(currentProducts.filter(p => p.id !== id));
            }),
            catchError(this.handleError)
        );
    }

    // Upload product images to Azure Blob Storage
    uploadImages(files: File[]): Observable<string[]> {
        const formData = new FormData();
        files.forEach(file => formData.append('files', file));

        return this.http.post<string[]>(`${this.apiUrl}/upload-images`, formData).pipe(
            map(fileNames => fileNames.map(fileName => `${this.blobStorageUrl}/${this.productsContainer}/${fileName}`)),
            catchError(this.handleError)
        );
    }

    getCategories(): Observable<{ id: string, name: string, description: string }[]> {
        return this.http.get<{ id: string, name: string, description: string }[]>(`${this.apiUrlcategories}`).pipe(
            catchError(this.handleError)
        );
    }

    // Error handling
    private handleError(error: any): Observable<never> {
        console.error('An error occurred:', error);
        throw error;
    }

    // Get products as observable
    get products$(): Observable<Product[]> {
        return this.productsSubject.asObservable();
    }
}
