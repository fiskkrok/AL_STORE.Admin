// src/app/store/product/product.state.ts
import { Product } from '../../shared/models/product.model';

export interface ProductState {
    products: Product[];
    selectedProduct: Product | null;
    loading: boolean;
    error: string | null;
    filters: {
        search: string;
        category: string;
        minPrice: number | null;
        maxPrice: number | null;
        inStock: boolean;
    };
    pagination: {
        currentPage: number;
        pageSize: number;
        totalItems: number;
    };
}

export const initialProductState: ProductState = {
    products: [],
    selectedProduct: null,
    loading: false,
    error: null,
    filters: {
        search: '',
        category: '',
        minPrice: null,
        maxPrice: null,
        inStock: false
    },
    pagination: {
        currentPage: 1,
        pageSize: 10,
        totalItems: 0
    }
};