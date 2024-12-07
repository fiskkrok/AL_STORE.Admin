export interface ProductImage {
    id: string;
    url: string;
    fileName: string;
    size: number;
    isPrimary: boolean;
    sortOrder: number;
    alt?: string;
}
