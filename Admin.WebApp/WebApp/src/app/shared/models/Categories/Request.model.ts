export interface CreateCategoryRequest {
    name: string;
    description: string;
    imageUrl?: string;
    metaTitle?: string;
    metaDescription?: string;
    parentCategoryId?: string;
}
export interface UpdateCategoryRequest extends CreateCategoryRequest {
    sortOrder?: number;
}

export interface ReorderCategoryRequest {
    categoryId: string;
    newSortOrder: number;
}