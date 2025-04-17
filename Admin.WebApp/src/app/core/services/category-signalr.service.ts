// src/app/core/services/category-signalr.service.ts
import { Injectable, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { CategoryActions } from '../../store/category/category.actions';
import { BaseSignalRService } from './signalr-service.base';
import { environment } from '../../../environments/environment';
import { Category } from '../../shared/models/category.model';
import { Subject } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class CategorySignalRService extends BaseSignalRService {
    protected override hubUrl = environment.signalR.category;
    private readonly store = inject(Store);

    // Event subjects
    private readonly categoryCreatedSubject = new Subject<Category>();
    readonly categoryCreated$ = this.categoryCreatedSubject.asObservable();

    private readonly categoryUpdatedSubject = new Subject<Category>();
    readonly categoryUpdated$ = this.categoryUpdatedSubject.asObservable();

    private readonly categoryDeletedSubject = new Subject<string>();
    readonly categoryDeleted$ = this.categoryDeletedSubject.asObservable();

    /**
     * Register SignalR event handlers for category hub
     */
    protected override registerEventHandlers(): void {
        if (!this.hubConnection) return;

        // Handle category created
        this.hubConnection.on('CategoryCreated', (category: Category) => {
            console.log('Category created via SignalR:', category.id);
            this.categoryCreatedSubject.next(category);
            this.store.dispatch(CategoryActions.createCategorySuccess({ category }));
        });

        // Handle category updated
        this.hubConnection.on('CategoryUpdated', (category: Category) => {
            console.log('Category updated via SignalR:', category.id);
            this.categoryUpdatedSubject.next(category);
            this.store.dispatch(CategoryActions.updateCategorySuccess({ category }));
        });

        // Handle category deleted
        this.hubConnection.on('CategoryDeleted', (categoryId: string) => {
            console.log('Category deleted via SignalR:', categoryId);
            this.categoryDeletedSubject.next(categoryId);
            this.store.dispatch(CategoryActions.deleteCategorySuccess({ id: categoryId }));
        });
    }

    /**
     * Called after successful connection
     */
    protected override onConnected(): void {
        console.log('Connected to category hub, requesting fresh category data');
        // Refresh category data upon successful connection
        this.store.dispatch(CategoryActions.loadCategories());
    }
}