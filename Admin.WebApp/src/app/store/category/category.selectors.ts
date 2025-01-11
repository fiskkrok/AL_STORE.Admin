// src/app/store/category/category.selectors.ts
import { createFeatureSelector, createSelector } from '@ngrx/store';
import { CategoryState } from './category.state';
import { Category } from 'src/app/shared/models/category.model';

export const selectCategoryState = createFeatureSelector<CategoryState>('categories');

export const selectAllCategories = createSelector(
    selectCategoryState,
    (state) => state.categories
);

export const selectSelectedCategory = createSelector(
    selectCategoryState,
    (state) => state.selectedCategory
);

export const selectCategoriesLoading = createSelector(
    selectCategoryState,
    (state) => state.loading
);

export const selectCategoriesError = createSelector(
    selectCategoryState,
    (state) => state.error
);

// Helper selector for category hierarchy
export const selectCategoryHierarchy = createSelector(
    selectAllCategories,
    (categories) => {
        const rootCategories = categories.filter(c => !c.parentCategoryId);
        return buildHierarchy(rootCategories, categories);
    }
);

function buildHierarchy(roots: Category[], allCategories: Category[]): Category[] {
    return roots.map(root => ({
        ...root,
        subCategories: allCategories
            .filter(c => c.parentCategoryId === root.id)
            .map(child => ({
                ...child,
                subCategories: buildHierarchy(
                    [child],
                    allCategories
                )[0].subCategories
            }))
    }));
}