// src/app/shared/models/product-type.model.ts
import { FormlyFieldConfig } from '@ngx-formly/core';

export interface ProductType {
    id: string;
    name: string;
    description: string;
    icon: string;
    attributes: ProductTypeAttribute[];
    formConfig: FormlyFieldConfig[];
}

export interface ProductTypeAttribute {
    id: string;
    name: string;
    description?: string;
    type: 'text' | 'number' | 'boolean' | 'select' | 'multiselect' | 'color' | 'size' | 'dimension';
    isRequired: boolean;
    defaultValue?: any;
    options?: { label: string, value: any }[];
    validation?: {
        min?: number;
        max?: number;
        pattern?: string;
        message?: string;
    };
    displayOrder: number;
    isFilterable: boolean;
    isComparable: boolean;
}

// Core product categories with specific attribute sets
export enum ProductCategory {
    Clothing = 'clothing',
    Electronics = 'electronics',
    Furniture = 'furniture',
    Books = 'books',
    Groceries = 'groceries',
    Beauty = 'beauty',
    Toys = 'toys',
    Other = 'other'
}

// Specific product types within categories
export interface ClothingAttributes {
    size?: string[];
    color?: string[];
    material?: string;
    gender?: 'men' | 'women' | 'unisex' | 'boys' | 'girls';
    season?: string[];
    care?: string;
}

export interface ElectronicsAttributes {
    brand?: string;
    model?: string;
    specifications?: Record<string, string>;
    warranty?: string;
    powerRequirements?: string;
    connectivity?: string[];
}

export interface BookAttributes {
    author?: string;
    publisher?: string;
    isbn?: string;
    language?: string;
    format?: 'hardcover' | 'paperback' | 'ebook' | 'audiobook';
    genre?: string[];
    pages?: number;
}

// Utility types for product variants
export interface SizeVariant {
    size: string;
    stock: number;
    price?: number; // Price adjustment if different from base
    sku?: string;
}

export interface ColorVariant {
    color: string;
    colorCode: string; // Hex code
    imageUrl?: string;
    stock: number;
    price?: number; // Price adjustment if different from base
    sku?: string;
}

export interface SizeColorVariant extends SizeVariant, Omit<ColorVariant, 'stock' | 'price' | 'sku'> {
    // Combined variant with specific stock/price for size+color combination
    stock: number;
    price?: number;
    sku?: string;

    // Additional properties can be added as needed
    // e.g., discount, promotional price, etc.
    discount?: number;
    promotionalPrice?: number;
    isFeatured?: boolean;
    isAvailable?: boolean;

}