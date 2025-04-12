import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { inject } from '@angular/core';

export abstract class BaseCrudService<T, ID> {
    protected http = inject(HttpClient);
    protected apiUrlBase = environment.apiUrls.admin.baseUrl;
    protected abstract endpoint: string; // To be defined by subclasses (e.g., 'categories', 'products')

    protected get apiUrl(): string {
        return `${this.apiUrlBase}/${this.endpoint}`;
    }

    getAll(): Observable<T[]> {
        return this.http.get<T[]>(this.apiUrl).pipe(catchError(this.handleError));
    }

    getById(id: ID): Observable<T> {
        return this.http
            .get<T>(`${this.apiUrl}/${id}`)
            .pipe(catchError(this.handleError));
    }

    create(item: Partial<T>): Observable<T> {
        // Use Partial<T> for creation if not all fields are required initially
        return this.http
            .post<T>(this.apiUrl, item)
            .pipe(catchError(this.handleError));
    }

    update(id: ID, item: T): Observable<T> {
        return this.http
            .put<T>(`${this.apiUrl}/${id}`, item)
            .pipe(catchError(this.handleError));
    }

    delete(id: ID): Observable<void> {
        return this.http
            .delete<void>(`${this.apiUrl}/${id}`)
            .pipe(catchError(this.handleError));
    }

    protected handleError(error: HttpErrorResponse) {
        // Basic error handling, can be expanded
        console.error(
            `Backend returned code ${error.status}, body was: `, error.error
        );
        // Return an observable with a user-facing error message.
        return throwError(
            () => new Error('Something bad happened; please try again later.')
        );
    }
}
