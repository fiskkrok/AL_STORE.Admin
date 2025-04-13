import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { inject } from '@angular/core';
import { ErrorService } from './error.service';
import { PagedResponse } from '../../shared/models/paged-response.model';

export interface QueryParams {
    [key: string]: string | number | boolean | null | undefined;
}
export abstract class BaseCrudService<T, ID = string, F = QueryParams> {
    protected readonly http = inject(HttpClient);
    protected readonly errorService = inject(ErrorService);
    protected apiUrlBase = environment.apiUrls.admin.baseUrl;
    protected abstract endpoint: string; // To be defined by subclasses (e.g., 'categories', 'products')

    protected get apiUrl(): string {
        return `${this.apiUrlBase}/${this.endpoint}`;
    }

    /**
     * Get all resources with optional filtering
     * @param params Optional query parameters
     * @returns Observable of resources
     */
    getAll(params?: F): Observable<T[]> {
        return this.http.get<T[]>(this.apiUrl, { params: this.buildParams(params) })
            .pipe(catchError(error => this.handleError(error, 'Failed to fetch data')));
    }

    /**
     * Get paged resources with optional filtering
     * @param params Optional query parameters including pagination
     * @returns Observable of paged response
     */
    getPaged(params?: F): Observable<PagedResponse<T>> {
        return this.http.get<PagedResponse<T>>(this.apiUrl, { params: this.buildParams(params) })
            .pipe(catchError(error => this.handleError(error, 'Failed to fetch paged data')));
    }

    // ...other methods remain the same...

    /**
     * Build HTTP parameters from object
     * @param params Object containing parameter key-value pairs
     * @returns HttpParams
     */
    protected buildParams(params?: F): HttpParams {
        let httpParams = new HttpParams();

        if (params && typeof params === 'object') {
            Object.entries(params as object).forEach(([key, value]) => {
                if (value !== null && value !== undefined) {
                    httpParams = httpParams.set(key, String(value));
                }
            });
        }

        return httpParams;
    }

    /**
     * Get resource by ID
     * @param id Resource ID
     * @returns Observable of resource
     */
    getById(id: ID): Observable<T> {
        return this.http.get<T>(`${this.apiUrl}/${id}`)
            .pipe(catchError(error => this.handleError(error, `Failed to fetch item with ID ${id}`)));
    }

    /**
     * Create a new resource
     * @param item Resource data
     * @returns Observable of created resource
     */
    create(item: Partial<T>): Observable<T> {
        return this.http.post<T>(this.apiUrl, item)
            .pipe(catchError(error => this.handleError(error, 'Failed to create item')));
    }

    /**
     * Update an existing resource
     * @param id Resource ID
     * @param item Updated resource data
     * @returns Observable of updated resource
     */
    update(id: ID, item: Partial<T>): Observable<T> {
        return this.http.put<T>(`${this.apiUrl}/${id}`, item)
            .pipe(catchError(error => this.handleError(error, `Failed to update item with ID ${id}`)));
    }

    /**
     * Delete a resource
     * @param id Resource ID
     * @returns Observable of void or deletion result
     */
    delete(id: ID): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`)
            .pipe(catchError(error => this.handleError(error, `Failed to delete item with ID ${id}`)));
    }

    /**
     * Bulk operation on multiple resources
     * @param ids Array of resource IDs
     * @param action Action to perform
     * @returns Observable of operation result
     */
    bulkOperation(ids: ID[], action: string): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/bulk/${action}`, { ids })
            .pipe(catchError(error => this.handleError(error, `Failed to perform bulk operation '${action}'`)));
    }

    /**
     * Standardized error handler for HTTP requests
     * @param error HTTP error
     * @param friendlyMessage User-friendly error message
     * @returns Observable error
     */
    protected handleError(error: HttpErrorResponse, friendlyMessage: string): Observable<never> {
        // Log the technical error details
        console.error('API Error:', error);

        // Add the error to the error service for display/logging
        this.errorService.addError({
            code: error.error?.code || `HTTP_${error.status}`,
            message: error.error?.message || friendlyMessage,
            details: error.error?.details,
            severity: error.status >= 500 ? 'error' : 'warning'
        });

        // Return a standardized error
        return throwError(() => new Error(friendlyMessage));
    }
}