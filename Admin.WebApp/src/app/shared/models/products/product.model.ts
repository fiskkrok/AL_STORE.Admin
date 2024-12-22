import { Category } from "../categories/category.model";
import { SubCategory } from "../categories/sub-category.model";
import { Money } from "../valueobjects/value-object.model";


export interface ProductImage {
    id: string;
    url: string;
    fileName: string;
    size: number;
    isPrimary: boolean;
    sortOrder: number;
    alt?: string;
}

export interface ProductVariant {
    id: string;
    sku: string;
    price: Money;
    stock: number;
    attributes: ProductAttribute[];
}

export interface ProductAttribute {
    name: string;
    value: string;
    type: 'color' | 'size' | 'material' | 'style' | string;
}

export interface Product { // extends AuditableEntity
    id: string;
    name: string;
    slug: string;
    description: string;
    shortDescription?: string;
    sku: string;
    barcode?: string;
    price: number;  // Change to number
    currency: string;  // Add currency field
    compareAtPrice?: number;
    category: Category;
    subCategory?: SubCategory;
    stock: number;
    lowStockThreshold?: number;

    status: ProductStatus;
    visibility: ProductVisibility;

    images: ProductImage[];
    variants?: ProductVariant[];
    attributes?: ProductAttribute[];

    seo?: {
        title?: string;
        description?: string;
        keywords?: string[];
    };

    dimensions?: {
        weight: number;
        width: number;
        height: number;
        length: number;
        unit: 'cm' | 'inch';
    };

    tags: string[];
    isArchived: boolean;
}

export enum ProductStatus {
    Draft = 'draft',
    Active = 'active',
    OutOfStock = 'out_of_stock',
    Discontinued = 'discontinued'
}

export enum ProductVisibility {
    Visible = 'visible',
    Hidden = 'hidden',
    Featured = 'featured'
}




enum PaymentStatus {
    Pending = 'pending',
    Authorized = 'authorized',
    Paid = 'paid',
    Failed = 'failed',
    Refunded = 'refunded'
}



