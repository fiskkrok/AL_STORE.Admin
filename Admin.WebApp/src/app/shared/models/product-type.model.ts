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