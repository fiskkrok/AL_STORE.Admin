import { productReducer } from './product.reducer';
import { ProductActions } from './product.actions';
import { initialProductState, ProductState } from './product.state';
import { Product, ProductStatus, ProductVisibility } from '../../shared/models/product.model';

describe('Product Reducer', () => {
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

    describe('unknown action', () => {
        it('should return the default state', () => {
            const action = { type: 'Unknown' };
            const state = productReducer(initialProductState, action);

            expect(state).toBe(initialProductState);
        });
    });

    describe('loadProducts actions', () => {
        it('should set loading to true', () => {
            const action = ProductActions.loadProducts({ filters: {} });
            const state = productReducer(initialProductState, action);

            expect(state.loading).toBe(true);
            expect(state.error).toBeNull();
        });

        it('should update state with products on success', () => {
            const products = [mockProduct];
            const action = ProductActions.loadProductsSuccess({
                products,
                totalItems: 1
            });
            const state = productReducer(initialProductState, action);

            expect(state.loading).toBe(false);
            expect(state.products).toEqual(products);
            expect(state.pagination.totalItems).toBe(1);
            expect(state.error).toBeNull();
        });

        it('should set error on failure', () => {
            const error = 'Error loading products';
            const action = ProductActions.loadProductsFailure({ error });
            const state = productReducer(initialProductState, action);

            expect(state.loading).toBe(false);
            expect(state.error).toBe(error);
        });
    });

    describe('addProduct actions', () => {
        it('should set loading to true', () => {
            const action = ProductActions.addProduct({
                product: mockProduct
            });
            const state = productReducer(initialProductState, action);

            expect(state.loading).toBe(true);
            expect(state.error).toBeNull();
        });

        it('should add product to state on success', () => {
            const action = ProductActions.addProductSuccess({
                product: mockProduct
            });
            const state = productReducer(initialProductState, action);

            expect(state.loading).toBe(false);
            expect(state.products).toContain(mockProduct);
            expect(state.error).toBeNull();
        });

        it('should set error on failure', () => {
            const error = 'Error adding product';
            const action = ProductActions.addProductFailure({ error });
            const state = productReducer(initialProductState, action);

            expect(state.loading).toBe(false);
            expect(state.error).toBe(error);
        });
    });

    describe('updateProduct actions', () => {
        const initialStateWithProduct: ProductState = {
            ...initialProductState,
            products: [mockProduct]
        };

        it('should set loading to true', () => {
            const action = ProductActions.updateProduct({
                id: '1',
                product: { ...mockProduct, name: 'Updated Name' }
            });
            const state = productReducer(initialStateWithProduct, action);

            expect(state.loading).toBe(true);
            expect(state.error).toBeNull();
        });

        it('should update product in state on success', () => {
            const updatedProduct = { ...mockProduct, name: 'Updated Name' };
            const action = ProductActions.updateProductSuccess({
                product: updatedProduct
            });
            const state = productReducer(initialStateWithProduct, action);

            expect(state.loading).toBe(false);
            expect(state.products.find(p => p.id === '1')?.name).toBe('Updated Name');
            expect(state.error).toBeNull();
        });

        it('should set error on failure', () => {
            const error = 'Error updating product';
            const action = ProductActions.updateProductFailure({ error });
            const state = productReducer(initialStateWithProduct, action);

            expect(state.loading).toBe(false);
            expect(state.error).toBe(error);
        });
    });

    describe('deleteProduct actions', () => {
        const initialStateWithProduct: ProductState = {
            ...initialProductState,
            products: [mockProduct]
        };

        it('should set loading to true', () => {
            const action = ProductActions.deleteProduct({ id: '1' });
            const state = productReducer(initialStateWithProduct, action);

            expect(state.loading).toBe(true);
            expect(state.error).toBeNull();
        });

        it('should remove product from state on success', () => {
            const action = ProductActions.deleteProductSuccess({ id: '1' });
            const state = productReducer(initialStateWithProduct, action);

            expect(state.loading).toBe(false);
            expect(state.products).not.toContain(mockProduct);
            expect(state.error).toBeNull();
        });

        it('should set error on failure', () => {
            const error = 'Error deleting product';
            const action = ProductActions.deleteProductFailure({ error });
            const state = productReducer(initialStateWithProduct, action);

            expect(state.loading).toBe(false);
            expect(state.error).toBe(error);
        });
    });

    describe('filter actions', () => {
        it('should update filters', () => {
            const filters = { search: 'test', category: '1' };
            const action = ProductActions.setFilters({ filters });
            const state = productReducer(initialProductState, action);

            expect(state.filters.search).toBe('test');
            expect(state.filters.category).toBe('1');
        });

        it('should reset filters', () => {
            const stateWithFilters: ProductState = {
                ...initialProductState,
                filters: {
                    ...initialProductState.filters,
                    search: 'test',
                    category: '1'
                }
            };

            const action = ProductActions.resetFilters();
            const state = productReducer(stateWithFilters, action);

            expect(state.filters).toEqual(initialProductState.filters);
        });
    });
});