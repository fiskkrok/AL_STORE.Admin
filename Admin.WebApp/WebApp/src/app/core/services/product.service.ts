// src/app/core/services/product.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders, HttpResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap, catchError, throwError, switchMap, filter, shareReplay } from 'rxjs';
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
            // tap(token => console.log('Token in getAuthHeaders:', token)), // log log
            map(token => {
                if (!token) {
                    throw new Error('No authentication token available');
                }
                // Note: Don't set Content-Type for FormData requests
                return new HttpHeaders().set('Authorization', `Bearer ${token}`);
            })
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
        return this.authService.getAccessToken().pipe(
            switchMap(token => {
                if (!token) {
                    throw new Error('No authentication token available');
                }
                const command = {
                    name: product.name,
                    description: product.description,
                    price: product.price,
                    currency: product.currency,
                    stock: product.stock,
                    categoryId: product.category.id,
                    subCategoryId: product.subCategory?.id || null,
                    images: product.images.map(img => ({
                        fileName: img.fileName,
                        url: img.url,
                        size: img.size
                    }))
                };
                const headers = new HttpHeaders()
                    .set('Authorization', `Bearer ${token}`)
                    .set('Content-Type', 'application/json');

                // console.log('Final headers:', headers.keys()); // Debug log
                // console.log('Product being sent:', JSON.stringify(product, null, 2)); // Debug log

                return this.http.post<Product>(
                    `${this.apiUrl}`,
                    command,
                    { headers }
                ).pipe(
                    tap(newProduct => {
                        console.log('Product added successfully:', newProduct);
                        const currentProducts = this.productsSubject.value;
                        this.productsSubject.next([...currentProducts, newProduct]);
                    }),
                    catchError(error => {
                        console.error('Error adding product:', error);
                        if (error.error) {
                            console.error('Error details:', error.error);
                        }
                        return throwError(() => error);
                    })
                );
            })
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
        // If there are new images, upload them first
        if (updateData.newImages?.length) {
            return this.uploadImages(updateData.newImages).pipe(
                switchMap(uploadResults => {
                    // Create FormData with uploaded image results
                    const formData = new FormData();
                    formData.append('id', id);
                    formData.append('name', updateData.name);
                    formData.append('description', updateData.description);
                    formData.append('price', updateData.price.toFixed(2));
                    formData.append('currency', updateData.currency);
                    formData.append('categoryId', updateData.categoryId);

                    if (updateData.subCategoryId) {
                        formData.append('subCategoryId', updateData.subCategoryId);
                    }

                    // Add uploaded image URLs
                    uploadResults.forEach(result => {
                        formData.append('newImages', result.url);
                    });

                    if (updateData.imageIdsToRemove?.length) {
                        updateData.imageIdsToRemove.forEach(imageId => {
                            formData.append('imageIdsToRemove', imageId);
                        });
                    }

                    return this.authService.getAccessToken().pipe(
                        switchMap(token => {
                            if (!token) {
                                throw new Error('No authentication token available');
                            }

                            const headers = new HttpHeaders({
                                'Authorization': `Bearer ${token}`
                            });

                            return this.http.put<void>(
                                `${this.apiUrl}/${id}`,
                                formData,
                                {
                                    headers,
                                    reportProgress: true,
                                    observe: 'events'
                                }
                            );
                        })
                    );
                }),
                filter(event => event instanceof HttpResponse),
                map(() => void 0),
                catchError(error => {
                    console.error('Error in updateProduct:', error);
                    return throwError(() => error);
                })
            );
        }

        return this.authService.getAccessToken().pipe(
            switchMap(token => {
                if (!token) {
                    throw new Error('No authentication token available');
                }

                const headers = new HttpHeaders()
                    .set('Authorization', `Bearer ${token}`)
                    .set('Content-Type', 'application/json')
                    .set('Accept', 'application/json');

                const jsonData = {
                    id,
                    name: updateData.name,
                    description: updateData.description,
                    price: updateData.price,
                    currency: updateData.currency,
                    categoryId: updateData.categoryId,
                    subCategoryId: updateData.subCategoryId,
                    imageIdsToRemove: updateData.imageIdsToRemove || [],
                    newImages: []
                };

                return this.http.put<void>(
                    `https://localhost:7048/api/products/${id}`,
                    jsonData,
                    { headers }
                );
            }),
            catchError(error => {
                console.error('Error in updateProduct:', error);
                return throwError(() => error);
            })
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

    uploadImages(files: File[]): Observable<FileUploadResult[]> {
        const formData = new FormData();

        files.forEach(file => {
            formData.append('files', file);
        });

        return this.authService.getAccessToken().pipe(
            switchMap(token => {
                if (!token) {
                    throw new Error('No authentication token available');
                }

                const headers = new HttpHeaders({
                    'Authorization': `Bearer ${token}`
                });

                console.log('Files being uploaded:', files.map(f => ({
                    name: f.name,
                    type: f.type,
                    size: f.size
                })));

                // Use shareReplay to prevent multiple HTTP requests
                return this.http.post<FileUploadResult[]>(
                    `${this.apiUrl}/upload-images`,
                    formData,
                    { headers }
                ).pipe(
                    tap(response => {
                        console.log('Upload response:', response);
                    }),
                    catchError(error => {
                        console.error('Error uploading images:', error);
                        return throwError(() => error);
                    }),
                    shareReplay(1)  // Add this to prevent duplicate requests
                );
            })
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

interface FileUploadResult {
    url: string;
    fileName: string;
    size: number;
}
