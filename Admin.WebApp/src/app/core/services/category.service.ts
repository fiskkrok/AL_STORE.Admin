// src/app/core/services/category.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, shareReplay } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Category } from 'src/app/shared/models/Categories/category.model';
import {
    CreateCategoryRequest,
    UpdateCategoryRequest,
    ReorderCategoryRequest
} from 'src/app/shared/models/Categories/Request.model';

@Injectable({
    providedIn: 'root'
})
export class CategoryService {
    private readonly apiUrl = environment.apiUrls.admin.categories;

    constructor(private readonly http: HttpClient) { }

    getCategory(id: string): Observable<Category> {
        return this.http.get<Category>(`${this.apiUrl}/${id}`).pipe(
            catchError(this.handleError)
        );
    }

    getCategories(): Observable<Category[]> {
        return this.http.get<Category[]>(this.apiUrl).pipe(
            shareReplay(1),
            catchError(this.handleError)
        );
    }

    createCategory(request: CreateCategoryRequest): Observable<Category> {
        return this.http.post<Category>(this.apiUrl, request).pipe(
            catchError(this.handleError)
        );
    }

    updateCategory(id: string, request: UpdateCategoryRequest): Observable<Category> {
        return this.http.put<Category>(`${this.apiUrl}/${id}`, request).pipe(
            catchError(this.handleError)
        );
    }

    deleteCategory(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
            catchError(this.handleError)
        );
    }

    reorderCategories(requests: ReorderCategoryRequest[]): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/reorder`, requests).pipe(
            catchError(this.handleError)
        );
    }

    private handleError(error: any): Observable<never> {
        let errorMessage = 'An unknown error occurred';

        if (error.error instanceof ErrorEvent) {
            // Client-side error
            errorMessage = error.error.message;
        } else {
            // Server-side error
            if (error.status === 400 && error.error?.code === 'Category.HasProducts') {
                errorMessage = 'Cannot delete category with associated products';
            } else if (error.status === 400 && error.error?.code === 'Category.CircularReference') {
                errorMessage = 'Cannot create circular reference in category hierarchy';
            } else {
                errorMessage = error.error?.message || 'Server error';
            }
        }

        console.error('Category service error:', error);
        return throwError(() => new Error(errorMessage));
    }
}