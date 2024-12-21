// src/app/store/category/category.effects.ts
import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { map, mergeMap, catchError, withLatestFrom, tap } from 'rxjs/operators';
import { CategoryService } from '../../core/services/category.service';
import { CategoryActions } from './category.actions';
import { selectCategoryState } from './category.selectors';
import { LoadingService } from '../../core/services/loading.service';
import { ErrorService } from '../../core/services/error.service';

@Injectable()
export class CategoryEffects {
    private actions$ = inject(Actions);
    private store = inject(Store);
    private categoryService = inject(CategoryService);
    private loadingService = inject(LoadingService);
    private errorService = inject(ErrorService);

    loadCategories$ = createEffect(() => {
        return this.actions$.pipe(
            ofType(CategoryActions.loadCategories),
            tap(() => this.loadingService.show()),
            mergeMap(() =>
                this.categoryService.getCategories().pipe(
                    map(categories => {
                        this.loadingService.hide();
                        return CategoryActions.loadCategoriesSuccess({ categories });
                    }),
                    catchError(error => {
                        this.loadingService.hide();
                        this.errorService.addError({
                            message: 'Failed to load categories',
                            code: error.code || 'LOAD_ERROR',
                            severity: 'error'
                        });
                        return of(CategoryActions.loadCategoriesFailure({
                            error: error.message || 'Failed to load categories'
                        }));
                    })
                )
            )
        );
    });

    createCategory$ = createEffect(() => {
        return this.actions$.pipe(
            ofType(CategoryActions.createCategory),
            tap(() => this.loadingService.show()),
            mergeMap(action =>
                this.categoryService.createCategory(action.request).pipe(
                    map(category => {
                        this.loadingService.hide();
                        return CategoryActions.createCategorySuccess({ category });
                    }),
                    catchError(error => {
                        this.loadingService.hide();
                        this.errorService.addError({
                            message: 'Failed to create category',
                            code: error.code || 'CREATE_ERROR',
                            severity: 'error'
                        });
                        return of(CategoryActions.createCategoryFailure({
                            error: error.message || 'Failed to create category'
                        }));
                    })
                )
            )
        );
    });

    updateCategory$ = createEffect(() => {
        return this.actions$.pipe(
            ofType(CategoryActions.updateCategory),
            tap(() => this.loadingService.show()),
            mergeMap(action =>
                this.categoryService.updateCategory(action.id, action.request).pipe(
                    map(category => {
                        this.loadingService.hide();
                        return CategoryActions.updateCategorySuccess({ category });
                    }),
                    catchError(error => {
                        this.loadingService.hide();
                        this.errorService.addError({
                            message: 'Failed to update category',
                            code: error.code || 'UPDATE_ERROR',
                            severity: 'error'
                        });
                        return of(CategoryActions.updateCategoryFailure({
                            error: error.message || 'Failed to update category'
                        }));
                    })
                )
            )
        );
    });

    deleteCategory$ = createEffect(() => {
        return this.actions$.pipe(
            ofType(CategoryActions.deleteCategory),
            tap(() => this.loadingService.show()),
            mergeMap(action =>
                this.categoryService.deleteCategory(action.id).pipe(
                    map(() => {
                        this.loadingService.hide();
                        return CategoryActions.deleteCategorySuccess({ id: action.id });
                    }),
                    catchError(error => {
                        this.loadingService.hide();
                        let errorMessage = 'Failed to delete category';

                        if (error.code === 'Category.HasProducts') {
                            errorMessage = 'Cannot delete category with associated products';
                        }

                        this.errorService.addError({
                            message: errorMessage,
                            code: error.code || 'DELETE_ERROR',
                            severity: 'error'
                        });
                        return of(CategoryActions.deleteCategoryFailure({
                            error: error.message || errorMessage
                        }));
                    })
                )
            )
        );
    });

    reorderCategories$ = createEffect(() => {
        return this.actions$.pipe(
            ofType(CategoryActions.reorderCategories),
            tap(() => this.loadingService.show()),
            mergeMap(action =>
                this.categoryService.reorderCategories(action.requests).pipe(
                    map(() => {
                        this.loadingService.hide();
                        return CategoryActions.reorderCategoriesSuccess();
                    }),
                    catchError(error => {
                        this.loadingService.hide();
                        this.errorService.addError({
                            message: 'Failed to reorder categories',
                            code: error.code || 'REORDER_ERROR',
                            severity: 'error'
                        });
                        return of(CategoryActions.reorderCategoriesFailure({
                            error: error.message || 'Failed to reorder categories'
                        }));
                    })
                )
            )
        );
    });

    // Reload categories after successful operations
    reloadAfterOperation$ = createEffect(() => {
        return this.actions$.pipe(
            ofType(
                CategoryActions.createCategorySuccess,
                CategoryActions.updateCategorySuccess,
                CategoryActions.deleteCategorySuccess,
                CategoryActions.reorderCategoriesSuccess
            ),
            map(() => CategoryActions.loadCategories())
        );
    });
}