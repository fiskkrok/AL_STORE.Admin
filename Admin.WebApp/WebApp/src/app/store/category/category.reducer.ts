// src/app/store/category/category.reducer.ts
import { createReducer, on } from '@ngrx/store';
import { CategoryActions } from './category.actions';
import { CategoryState, initialCategoryState } from './category.state';

export const categoryReducer = createReducer(
    initialCategoryState,

    // Load Categories
    on(CategoryActions.loadCategories, (state) => ({
        ...state,
        loading: true,
        error: null
    })),

    on(CategoryActions.loadCategoriesSuccess, (state, { categories }) => ({
        ...state,
        categories,
        loading: false,
        error: null
    })),

    on(CategoryActions.loadCategoriesFailure, (state, { error }) => ({
        ...state,
        loading: false,
        error
    })),

    // Create Category
    on(CategoryActions.createCategory, (state) => ({
        ...state,
        loading: true,
        error: null
    })),

    on(CategoryActions.createCategorySuccess, (state, { category }) => ({
        ...state,
        categories: [...state.categories, category],
        loading: false,
        error: null
    })),

    on(CategoryActions.createCategoryFailure, (state, { error }) => ({
        ...state,
        loading: false,
        error
    })),

    // Update Category
    on(CategoryActions.updateCategory, (state) => ({
        ...state,
        loading: true,
        error: null
    })),

    on(CategoryActions.updateCategorySuccess, (state, { category }) => ({
        ...state,
        categories: state.categories.map(c =>
            c.id === category.id ? category : c
        ),
        loading: false,
        error: null
    })),

    on(CategoryActions.updateCategoryFailure, (state, { error }) => ({
        ...state,
        loading: false,
        error
    })),

    // Delete Category
    on(CategoryActions.deleteCategory, (state) => ({
        ...state,
        loading: true,
        error: null
    })),

    on(CategoryActions.deleteCategorySuccess, (state, { id }) => ({
        ...state,
        categories: state.categories.filter(c => c.id !== id),
        loading: false,
        error: null
    })),

    on(CategoryActions.deleteCategoryFailure, (state, { error }) => ({
        ...state,
        loading: false,
        error
    })),

    // Select Category
    on(CategoryActions.selectCategory, (state, { category }) => ({
        ...state,
        selectedCategory: category
    }))
);