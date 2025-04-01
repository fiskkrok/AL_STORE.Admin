// Product related interfaces
export interface Category {
    id: string;
    name: string;
    description: string;
    slug: string;
    sortOrder: number;
    metaTitle?: string;
    metaDescription?: string;
    imageUrl?: string;
    parentCategoryId?: string;
    parentCategory?: Category | null;
    subCategories: Category[];
    productCount: number;
    createdAt: string; // ISO date string
    createdBy?: string;
    lastModifiedAt?: string; // ISO date string
    lastModifiedBy?: string;
}

interface SubCategory extends Omit<Category, 'parentId'> {
    parentCategoryId: string;
}
