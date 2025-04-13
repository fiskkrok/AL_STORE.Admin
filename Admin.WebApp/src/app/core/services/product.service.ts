// src/app/core/services/product.service.ts
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { BaseCrudService } from './base-crud.service';
import {
    Product,
    ProductFilters,
    ProductCreateCommand,
    ProductUpdateCommand
} from '../../shared/models/product.model';
import { PagedResponse } from '../../shared/models/paged-response.model';
import { environment } from '../../../environments/environment';
import { HttpParams } from '@angular/common/http';

@Injectable({
    providedIn: 'root'
})
export class ProductService extends BaseCrudService<Product, string, ProductFilters> {
    protected override endpoint = 'products';
    private readonly blobStorageUrl = environment.azure.blobStorage.containerUrl;
    private readonly productsContainer = environment.azure.blobStorage.productsContainer;

    // Override the base getAll to add product-specific mapping
    override getAll(filters?: ProductFilters): Observable<Product[]> {
        return super.getAll(filters).pipe(
            map(products => products.map(product => this.mapProductFromApi(product)))
        );
    }

    // Override getPaged to add product-specific mapping
    override getPaged(filters?: ProductFilters): Observable<PagedResponse<Product>> {
        return super.getPaged(filters).pipe(
            map(response => ({
                ...response,
                items: response.items.map(item => this.mapProductFromApi(item))
            }))
        );
    }

    // Override getById to add product-specific mapping
    override getById(id: string): Observable<Product> {
        return super.getById(id).pipe(
            map(product => this.mapProductFromApi(product))
        );
    }

    /**
     * Creates a new product
     * @param command Product creation data
     * @returns Observable of created product
     */
    createProduct(command: ProductCreateCommand): Observable<Product> {
        // Generate slug if not provided
        if (!command.slug) {
            command.slug = this.generateSlug(command.name);
        }

        return this.create(command).pipe(
            map(product => this.mapProductFromApi(product))
        );
    }
    /**
    * Deletes a product
    */
    deleteProduct(id: string): Observable<void> {
        return this.http.delete<void>(id).pipe(

            map(() => { }),
            catchError(error => this.handleError(error, 'Failed to delete product'))
        );
    }
    /**
     * Updates an existing product
     * @param command Product update data
     * @returns Observable of updated product
     */
    updateProduct(command: ProductUpdateCommand): Observable<Product> {
        const { id, ...updateData } = command;
        return this.update(id, updateData).pipe(
            map(product => this.mapProductFromApi(product))
        );
    }

    /**
     * Maps a product from API format to client model
     * @param product Raw product data from API
     * @returns Mapped Product object
     */
    private mapProductFromApi(product: any): Product {
        return {
            ...product,
            // Ensure collections are never null and properly format image URLs
            images: (product.images || []).map((img: any) => ({
                ...img,
                url: this.getFullImageUrl(img.url)
            })),
            attributes: product.attributes || [],
            tags: product.tags || [],
            variants: Array.isArray(product.variants)
                ? product.variants.map((v: any) => ({
                    ...v,
                    images: (v.images || []).map((img: any) => ({
                        ...img,
                        url: this.getFullImageUrl(img.url)
                    }))
                }))
                : []
        };
    }

    /**
     * Ensures image URLs are properly formatted with the base URL if needed
     * @param url Raw image URL
     * @returns Full image URL
     */
    private getFullImageUrl(url: string): string {
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
     * @param name Product name
     * @returns URL-friendly slug
     */
    private generateSlug(name: string): string {
        return name
            .toLowerCase()
            .replace(/[^a-z0-9]+/g, '-')
            .replace(/(^-|-$)/g, '');
    }

    /**
     * Gets product statistics
     * @returns Observable of product stats
     */
    getStats(): Observable<any> {
        return this.http.get<any>(`${this.apiUrl}/stats`).pipe(
            catchError(error => this.handleError(error, 'Failed to fetch product statistics'))
        );
    }

    /**
     * Uploads product images
     * @param files Image files to upload
     * @returns Observable of uploaded images data
     */
    uploadImages(files: File[]): Observable<any[]> {
        const formData = new FormData();
        files.forEach(file => formData.append('files', file));

        return this.http.post<any[]>(`${this.apiUrl}/upload-images`, formData).pipe(
            catchError(error => this.handleError(error, 'Failed to upload images'))
        );
    }

    protected override buildParams(params?: ProductFilters): HttpParams {
        // Custom implementation for ProductFilters
        let httpParams = new HttpParams();

        if (params) {
            // Handle primitive properties
            if (params.search) httpParams = httpParams.set('search', params.search);
            if (params.categoryId) httpParams = httpParams.set('categoryId', params.categoryId);
            if (params.subCategoryId) httpParams = httpParams.set('subCategoryId', params.subCategoryId);
            if (params.minPrice !== null && params.minPrice !== undefined) httpParams = httpParams.set('minPrice', params.minPrice.toString());
            if (params.maxPrice !== null && params.maxPrice !== undefined) httpParams = httpParams.set('maxPrice', params.maxPrice.toString());
            if (params.status) httpParams = httpParams.set('status', params.status);
            if (params.visibility) httpParams = httpParams.set('visibility', params.visibility);
            if (params.inStock !== null && params.inStock !== undefined) httpParams = httpParams.set('inStock', params.inStock.toString());
            if (params.page) httpParams = httpParams.set('page', params.page.toString());
            if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
            if (params.sortColumn) httpParams = httpParams.set('sortColumn', params.sortColumn);
            if (params.sortDirection) httpParams = httpParams.set('sortDirection', params.sortDirection);

            // Handle arrays
            if (params.tags?.length) {
                httpParams = httpParams.set('tags', params.tags.join(','));
            }
        }

        return httpParams;
    }
}