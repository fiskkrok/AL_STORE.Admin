// Admin.WebApp/src/app/shared/models/product.model.ts
import { Category } from "./category.model";

export interface Product {
    id: string;
    name: string;
    slug: string;
    description: string;
    shortDescription?: string;
    sku: string;
    barcode?: string;
    price: number;
    currency: string;
    compareAtPrice?: number;
    category: Category;
    subCategory?: Category;
    stock: number;
    lowStockThreshold?: number;
    status: ProductStatus; // Enum that maps to backend string values
    visibility: ProductVisibility; // Enum that maps to backend string values
    images: ProductImage[];
    variants: ProductVariant[];
    attributes: ProductAttribute[];
    seo?: ProductSeo;
    dimensions?: ProductDimensions;
    tags: string[];
    createdAt: string; // ISO date string
    createdBy?: string;
    lastModifiedAt?: string; // ISO date string
    lastModifiedBy?: string;
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
    price: number;
    currency: string;
    compareAtPrice?: number;
    costPrice?: number;
    barcode?: string;
    stock: number;
    trackInventory: boolean;
    allowBackorders: boolean;
    lowStockThreshold?: number;
    sortOrder: number;
    isLowStock: boolean;
    isOutOfStock: boolean;
    attributes: ProductAttribute[];
    images: ProductImage[];
    productId: string;
}

export interface ProductAttribute {
    name: string;
    value: string;
    type: string;
}

export interface ProductSeo {
    title?: string;
    description?: string;
    keywords: string[];
}

export interface ProductDimensions {
    weight: number;
    width: number;
    height: number;
    length: number;
    unit: 'cm' | 'inch';
}