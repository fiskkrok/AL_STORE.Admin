// src/app/core/services/category.service.ts
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { BaseCrudService } from './base-crud.service';
import { Category } from '../../shared/models/category.model';
import { Store } from '@ngrx/store';
import { CategoryActions } from '../../store/category/category.actions';
import {
    CreateCategoryRequest,
    UpdateCategoryRequest,
    ReorderCategoryRequest
} from '../../shared/models/Request.model';
import { StockSignalRService } from './stock-signalr.service';

@Injectable({
    providedIn: 'root'
})
export class CategoryService extends BaseCrudService<Category, string> {
    protected override endpoint = 'categories';

    private readonly store = inject(Store);
    private readonly categorySignalR = inject(StockSignalRService);

    /**
     * Get all categories 
     * @returns Observable of categories
     */
    override getAll(): Observable<Category[]> {
        return super.getAll().pipe(
            map(categories => categories.map(this.mapCategoryFromApi))
        );
    }

    /**
     * Get a specific category by ID
     * @param id Category ID
     * @returns Observable of category
     */
    override getById(id: string): Observable<Category> {
        return super.getById(id).pipe(
            map(this.mapCategoryFromApi)
        );
    }

    /**
     * Create a new category
     * @param request Category creation data
     * @returns Observable of created category
     */
    createCategory(request: CreateCategoryRequest): Observable<Category> {
        return this.create(request).pipe(
            map(this.mapCategoryFromApi)
        );
    }

    /**
     * Update an existing category
     * @param id Category ID
     * @param request Category update data
     * @returns Observable of updated category
     */
    updateCategory(id: string, request: UpdateCategoryRequest): Observable<Category> {
        return this.update(id, request).pipe(
            map(this.mapCategoryFromApi)
        );
    }

    /**
     * Reorder categories
     * @param requests Array of reorder requests
     * @returns Observable of operation result
     */
    reorderCategories(requests: ReorderCategoryRequest[]): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/reorder`, requests).pipe(
            tap(() => {
                // After successful reordering, reload categories to get updated positions
                this.store.dispatch(CategoryActions.loadCategories());
            }),
            catchError(error => this.handleError(error, 'Failed to reorder categories'))
        );
    }

    /**
     * Map category from API response to client model
     * @param category Raw category data
     * @returns Mapped Category object
     */
    private mapCategoryFromApi(category: any): Category {
        return {
            ...category,
            subCategories: category.subCategories || [],
            createdAt: category.createdAt,
            lastModifiedAt: category.lastModifiedAt,
            parentCategory: category.parentCategory ? this.mapCategoryFromApi(category.parentCategory) : null
        };
    }
}