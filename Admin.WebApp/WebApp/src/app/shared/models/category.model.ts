// Product related interfaces
export interface Category {
    id: string;
    name: string;
    description: string;
    parentId?: string;
    slug: string;
    isActive: boolean;
}
