import { createReducer, on } from '@ngrx/store';
import { ProductActions } from './product.actions';
import { initialProductState } from './product.state';



export const productReducer = createReducer(
    initialProductState,

    // Load Products
    on(ProductActions.loadProducts, (state) => ({
        ...state,
        loading: true,
        error: null
    })),

    on(ProductActions.loadProductsSuccess, (state, { products, totalItems }) => ({
        ...state,
        products,
        loading: false,
        error: null,
        pagination: {
            ...state.pagination,
            totalItems
        },
        cache: {
            ...state.cache,
            timestamp: Date.now()
        }
    })),

    on(ProductActions.loadProductsFailure, (state, { error }) => ({
        ...state,
        loading: false,
        error
    })),

    // Add Product
    on(ProductActions.addProduct, (state) => ({
        ...state,
        loading: true,
        error: null
    })),

    on(ProductActions.addProductSuccess, (state, { product }) => ({
        ...state,
        products: [...state.products, product],
        loading: false,
        error: null
    })),

    on(ProductActions.addProductFailure, (state, { error }) => ({
        ...state,
        loading: false,
        error
    })),

    // Optimistic Update
    on(ProductActions.optimisticUpdateProduct, (state, { id, changes }) => {
        const productToUpdate = state.products.find(p => p.id === id);
        if (!productToUpdate) return state;

        return {
            ...state,
            optimisticUpdate: {
                originalProduct: productToUpdate,
                pending: true
            },
            products: state.products.map(p =>
                p.id === id ? { ...p, ...changes } : p
            )
        };
    }),

    on(ProductActions.revertOptimisticUpdate, (state) => {
        if (!state.optimisticUpdate.originalProduct) return state;

        return {
            ...state,
            products: state.products.map(p =>
                p.id === state.optimisticUpdate.originalProduct?.id
                    ? state.optimisticUpdate.originalProduct
                    : p
            ),
            optimisticUpdate: {
                originalProduct: null,
                pending: false
            }
        };
    }),

    // Update Product
    on(ProductActions.updateProductSuccess, (state, { product }) => ({
        ...state,
        products: state.products.map(p => p.id === product.id ? product : p),
        loading: false,
        error: null,
        optimisticUpdate: {
            originalProduct: null,
            pending: false
        }
    })),

    on(ProductActions.updateProductFailure, (state, { error }) => ({
        ...state,
        loading: false,
        error
    })),

    // Delete Product
    on(ProductActions.deleteProduct, (state) => ({
        ...state,
        loading: true,
        error: null
    })),

    on(ProductActions.deleteProductSuccess, (state, { id }) => ({
        ...state,
        products: state.products.filter(p => p.id !== id),
        loading: false,
        error: null
    })),

    on(ProductActions.deleteProductFailure, (state, { error }) => ({
        ...state,
        loading: false,
        error
    })),

    // Selection
    on(ProductActions.selectProduct, (state, { product }) => ({
        ...state,
        selectedProductId: product.id
    })),

    on(ProductActions.clearSelectedProduct, (state) => ({
        ...state,
        selectedProductId: null
    })),

    // Filters
    on(ProductActions.setFilters, (state, { filters }) => ({
        ...state,
        filters: {
            ...state.filters,
            ...filters
        }
    })),

    on(ProductActions.resetFilters, (state) => ({
        ...state,
        filters: initialProductState.filters
    })),

    // Cache
    on(ProductActions.setCacheTimestamp, (state, { timestamp }) => ({
        ...state,
        cache: {
            ...state.cache,
            timestamp
        }
    })),

    on(ProductActions.clearCache, (state) => ({
        ...state,
        cache: initialProductState.cache
    }))
);