import { ProductActions } from './product.actions';
import { ProductState, initialProductState } from './product.state';

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
    // Update Product
    on(ProductActions.updateProduct, (state) => ({
        ...state,
        loading: true,
        error: null
    })),

    on(ProductActions.updateProductSuccess, (state, { product }) => ({
        ...state,
        products: state.products.map(p => p.id === product.id ? product : p),
        loading: false,
        error: null
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

    // Product Selection
    on(ProductActions.selectProduct, (state, { product }) => ({
        ...state,
        selectedProduct: product
    })),

    on(ProductActions.clearSelectedProduct, (state) => ({
        ...state,
        selectedProduct: null
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
    }))
);


