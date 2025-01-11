import { Product } from "src/app/shared/models/product.model";

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
        inStock: boolean | null;
        page: number;
        pageSize: number;
        sortColumn?: keyof Product;
        sortDirection?: 'asc' | 'desc';
    };
    pagination: {
        currentPage: number;
        pageSize: number;
        totalItems: number;
    };
    optimisticUpdate: {
        originalProduct: Product | null;
        pending: boolean;
    };
    cache: {
        timestamp: number | null;
        duration: number; // cache duration in milliseconds
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
        inStock: null,
        page: 1,
        pageSize: 10,
        sortColumn: undefined,
        sortDirection: undefined
    },
    pagination: {
        currentPage: 1,
        pageSize: 10,
        totalItems: 0
    },
    optimisticUpdate: {
        originalProduct: null,
        pending: false
    },
    cache: {
        timestamp: null,
        duration: 5 * 60 * 1000 // 5 minutes
    }
};