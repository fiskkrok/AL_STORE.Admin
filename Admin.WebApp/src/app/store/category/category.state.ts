import { Category } from "src/app/shared/models/categories/category.model";


export interface CategoryState {
    categories: Category[];
    selectedCategory: Category | null;
    loading: boolean;
    error: string | null;
}

export const initialCategoryState: CategoryState = {
    categories: [],
    selectedCategory: null,
    loading: false,
    error: null
};