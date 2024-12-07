import { Category } from "./category.model";

export interface SubCategory extends Omit<Category, 'parentId'> {
    parentCategoryId: string;
}
