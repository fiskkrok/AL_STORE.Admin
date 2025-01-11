import * as fromSelectors from './product.selectors';
import { ProductState, initialProductState } from './product.state';
import { Product, ProductStatus, ProductVisibility } from '../../shared/models/product.model';

describe('Product Selectors', () => {
    const mockProduct: Product = {
        id: '1',
        name: 'Test Product',
        description: 'Test Description',
        price: 99.99,
        currency: 'USD',
        stock: 10,
        category: {
            id: '1',
            name: 'Test Category',
            description: 'Test Category Description',
            slug: '',
            isActive: false
        },
        subCategory: undefined,
        images: [],
        slug: '',
        sku: '',
        status: ProductStatus.Draft,
        visibility: ProductVisibility.Visible,
        tags: [],
        isArchived: false
    };

    const mockState: { products: ProductState } = {
        products: {
            ...initialProductState,
            products: [mockProduct],
            selectedProduct: mockProduct,
            loading: false,
            error: null,
            filters: {
                ...initialProductState.filters,
                search: 'test'
            },
            pagination: {
                currentPage: 1,
                pageSize: 10,
                totalItems: 1
            }
        }
    };

    describe('selectAllProducts', () => {
        it('should return all products', () => {
            const result = fromSelectors.selectAllProducts(mockState);
            expect(result).toEqual([mockProduct]);
        });
    });

    describe('selectSelectedProduct', () => {
        it('should return the selected product', () => {
            const result = fromSelectors.selectSelectedProduct(mockState);
            expect(result).toEqual(mockProduct);
        });
    });

    describe('selectProductsLoading', () => {
        it('should return the loading state', () => {
            const result = fromSelectors.selectProductsLoading(mockState);
            expect(result).toBeFalsy();
        });
    });

    describe('selectProductsError', () => {
        it('should return the error state', () => {
            const result = fromSelectors.selectProductsError(mockState);
            expect(result).toBeNull();
        });
    });

    describe('selectProductFilters', () => {
        it('should return the current filters', () => {
            const result = fromSelectors.selectProductFilters(mockState);
            expect(result.search).toBe('test');
        });
    });

    describe('selectProductPagination', () => {
        it('should return the pagination state', () => {
            const result = fromSelectors.selectProductPagination(mockState);
            expect(result).toEqual({
                currentPage: 1,
                pageSize: 10,
                totalItems: 1
            });
        });
    });
});