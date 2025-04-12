// src/app/core/services/category.service.ts
import { Injectable, inject, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map, shareReplay } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Category } from 'src/app/shared/models/category.model';
import {
    CreateCategoryRequest,
    UpdateCategoryRequest,
    ReorderCategoryRequest
} from 'src/app/shared/models/Request.model';
import { Store } from '@ngrx/store';
import * as signalR from '@microsoft/signalr';
import { CategoryActions } from 'src/app/store/category/category.actions';
import { AuthService } from './auth.service';

@Injectable({
    providedIn: 'root'
})
export class CategoryService implements OnDestroy {
    private readonly apiUrl = environment.apiUrls.admin.categories;
    private hubConnection: signalR.HubConnection | undefined;
    private readonly authService = inject(AuthService);

    constructor(
        private readonly http: HttpClient,
        private readonly store: Store
    ) {
        this.initializeSignalR();
    }
    private mapCategoryFromApi(category: any): Category {
        return {
            ...category,
            // Ensure collections are never null
            subCategories: category.subCategories || [],
            // Convert dates if needed
            createdAt: category.createdAt,
            lastModifiedAt: category.lastModifiedAt,
            // Convert parent category recursively if it exists
            parentCategory: category.parentCategory ? this.mapCategoryFromApi(category.parentCategory) : null
        };
    }
    private initializeSignalR() {
        this.authService.getAccessToken().subscribe(token => {
            if (!token) {
                console.warn('No auth token available for Category SignalR connection');
                return;
            }

            this.hubConnection = new signalR.HubConnectionBuilder()
                .withUrl(environment.signalR.category, {
                    accessTokenFactory: () => token
                })
                .withAutomaticReconnect()
                .build();

            this.hubConnection.start().catch(err => console.error('Error starting SignalR:', err));

            this.hubConnection.on('CategoryCreated', (category: Category) => {
                this.store.dispatch(CategoryActions.createCategorySuccess({ category }));
            });

            this.hubConnection.on('CategoryUpdated', (category: Category) => {
                this.store.dispatch(CategoryActions.updateCategorySuccess({ category }));
            });

            this.hubConnection.on('CategoryDeleted', (categoryId: string) => {
                this.store.dispatch(CategoryActions.deleteCategorySuccess({ id: categoryId }));
            });
        });

        // Recreate connection when auth state changes
        this.authService.authState$.subscribe(state => {
            if (state.isAuthenticated && state.accessToken && (!this.hubConnection || this.hubConnection.state === signalR.HubConnectionState.Disconnected)) {
                this.createConnection(state.accessToken);
            }
        });
    }

    private createConnection(token: string) {
        // Close existing connection if open
        if (this.hubConnection) {
            this.hubConnection.stop().catch(err => console.error('Error stopping connection:', err));
        }

        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(environment.signalR.category, {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        this.setupHubListeners();
        this.hubConnection.start().catch(err => console.error('Error starting SignalR:', err));
    }

    private setupHubListeners() {
        if (!this.hubConnection) return;

        this.hubConnection.on('CategoryCreated', (category: Category) => {
            this.store.dispatch(CategoryActions.createCategorySuccess({ category }));
        });

        this.hubConnection.on('CategoryUpdated', (category: Category) => {
            this.store.dispatch(CategoryActions.updateCategorySuccess({ category }));
        });

        this.hubConnection.on('CategoryDeleted', (categoryId: string) => {
            this.store.dispatch(CategoryActions.deleteCategorySuccess({ id: categoryId }));
        });
    }

    getCategories(): Observable<Category[]> {
        return this.http.get<any[]>(this.apiUrl).pipe(
            map(categories => categories.map(this.mapCategoryFromApi))
        );
    }

    getCategory(id: string): Observable<Category> {
        return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
            map(this.mapCategoryFromApi)
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

    // Clean up SignalR connection
    ngOnDestroy() {
        if (this.hubConnection) {
            this.hubConnection.stop();
        }
    }
}