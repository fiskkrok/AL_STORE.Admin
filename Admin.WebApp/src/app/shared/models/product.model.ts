// Admin.WebApp/src/app/shared/models/product.model.ts
import { Category } from "./category.model";

// Enhanced filter options for product queries
export interface ProductFilters {
    search?: string;
    categoryId?: string;
    subCategoryId?: string;
    minPrice?: number | null;
    maxPrice?: number | null;
    status?: ProductStatus;
    visibility?: ProductVisibility;
    inStock?: boolean | null;
    tags?: string[];
    page?: number;
    pageSize?: number;
    sortColumn?: keyof Product;
    sortDirection?: 'asc' | 'desc';
}

// Command pattern interface for creating products
export interface ProductCreateCommand {
    name: string;
    slug?: string;
    description: string;
    shortDescription?: string;
    sku: string;
    price: number;
    currency: string;
    compareAtPrice?: number;
    categoryId: string;
    subCategoryId?: string;
    stock: number;
    lowStockThreshold?: number;
    barcode?: string;
    images: ProductImage[];
    status: ProductStatus;
    visibility: ProductVisibility;
    attributes?: Array<{ name: string; value: string; type: string }>;
    tags?: string[];
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
}

// Command pattern interface for updating products
export interface ProductUpdateCommand extends Partial<ProductCreateCommand> {
    id: string;
    newImages?: File[];
    imageIdsToRemove?: string[];
    imageUpdates?: Array<{ id: string; isPrimary?: boolean; sortOrder?: number; alt?: string }>;
}

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

// Utility functions for mapping API responses to model objects

/**
 * Maps a product from API response format to the Product interface
 * @param product The raw product data from API
 * @param getFullImageUrl Function to transform image URLs
 * @returns A properly formatted Product object
 */
export function mapProductFromApi(product: any, getFullImageUrl: (url: string) => string): Product {
    return {
        ...product,
        // Map string values to enum values
        status: product.status as ProductStatus,
        visibility: product.visibility as ProductVisibility,
        // Ensure collections are never null and properly format image URLs
        images: (product.images || []).map((img: ProductImage) => ({
            ...img,
            url: getFullImageUrl(img.url)
        })),
        attributes: product.attributes || [],
        tags: product.tags || [],
        // Format dates if needed
        createdAt: product.createdAt,
        lastModifiedAt: product.lastModifiedAt,
        variants: Array.isArray(product.variants)
            ? product.variants.map((v: any) => mapVariantFromApi(v, getFullImageUrl))
            : []
    };
}

/**
 * Maps a product variant from API response format to the ProductVariant interface
 * @param variant The raw variant data from API
 * @param getFullImageUrl Function to transform image URLs
 * @returns A properly formatted ProductVariant object
 */
export function mapVariantFromApi(variant: any, getFullImageUrl: (url: string) => string): ProductVariant {
    return {
        id: variant.id,
        sku: variant.sku,
        price: variant.price,
        currency: variant.currency || 'USD',
        compareAtPrice: variant.compareAtPrice,
        costPrice: variant.costPrice,
        barcode: variant.barcode,
        stock: variant.stock,
        trackInventory: variant.trackInventory ?? true,
        allowBackorders: variant.allowBackorders ?? false,
        lowStockThreshold: variant.lowStockThreshold,
        sortOrder: variant.sortOrder || 0,
        isLowStock: variant.isLowStock ?? false,
        isOutOfStock: variant.isOutOfStock ?? false,
        attributes: Array.isArray(variant.attributes) ? variant.attributes : [],
        images: Array.isArray(variant.images)
            ? variant.images.map((img: ProductImage) => ({
                ...img,
                url: getFullImageUrl(img.url)
            }))
            : [],
        productId: variant.productId
    };
}