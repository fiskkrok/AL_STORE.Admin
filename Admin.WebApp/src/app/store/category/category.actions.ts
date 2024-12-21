// src/app/store/category/category.actions.ts
import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { CreateCategoryRequest, UpdateCategoryRequest, ReorderCategoryRequest } from 'src/app/shared/models/Categories/Request.model';
import { Category } from 'src/app/shared/models/Categories/category.model';

export const CategoryActions = createActionGroup({
    source: 'Category',
    events: {
        'Load Categories': emptyProps(),
        'Load Categories Success': props<{ categories: Category[] }>(),
        'Load Categories Failure': props<{ error: string }>(),

        'Create Category': props<{ request: CreateCategoryRequest }>(),
        'Create Category Success': props<{ category: Category }>(),
        'Create Category Failure': props<{ error: string }>(),

        'Update Category': props<{ id: string, request: UpdateCategoryRequest }>(),
        'Update Category Success': props<{ category: Category }>(),
        'Update Category Failure': props<{ error: string }>(),

        'Delete Category': props<{ id: string }>(),
        'Delete Category Success': props<{ id: string }>(),
        'Delete Category Failure': props<{ error: string }>(),

        'Reorder Categories': props<{ requests: ReorderCategoryRequest[] }>(),
        'Reorder Categories Success': emptyProps(),
        'Reorder Categories Failure': props<{ error: string }>(),

        'Select Category': props<{ category: Category | null }>()
    }
});