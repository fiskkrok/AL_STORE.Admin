import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable, of, throwError } from 'rxjs';
import { Action } from '@';
import { ProductEffects } from './product.effects';
import { ProductActions } from './product.actions';
import { ProductService } from '../../core/services/product.service';
import { Product, ProductStatus, ProductVisibility } from '../../shared/models/product.model';
import { MockStore, provideMockStore } from '@/testing';
import { initialProductState } from './product.state';

describe('ProductEffects', () => {
    let actions$: Observable<Action>;
    let effects: ProductEffects;
    let productService: jasmine.SpyObj<ProductService>;
    let store: MockStore;

    const mockProduct: Product = {
        id: '1',
        name: 'Test Product',
        description: 'Test Description',
        price: { amount: 100, currency: 'USD' },
        stock: 10,
        category: {
            id: '1',
            name: 'Test Category',
            slug: 'test-category',
            description: 'Test Category Description',
            isActive: true,
        },
        subCategory: {
            parentCategoryId: '1',
            id: '2',
            name: 'Test Subcategory',
            slug: 'test-subcategory',
            description: 'Test Subcategory Description',
            isActive: true,
        },
        images: [],
        createdAt: new Date().toISOString(),
        createdBy: null,
        lastModifiedAt: null,
        lastModifiedBy: null,
        slug: 'test-product',
        sku: 'TEST-001',
        status: ProductStatus.Active,
        visibility: ProductVisibility.Visible,
        tags: ['test'],
        isArchived: false,
    };

    beforeEach(() => {
        const mockProductService = jasmine.createSpyObj('ProductService', [
            'getProducts',
            'addProduct',
            'updateProduct',
            'deleteProduct'
        ]);

        TestBed.configureTestingModule({
            providers: [
                ProductEffects,
                provideMockActions(() => actions$),
                provideMockStore({ initialState: { products: initialProductState } }),
                { provide: ProductService, useValue: mockProductService }
            ]
        });

        effects = TestBed.inject(ProductEffects);
        productService = TestBed.inject(ProductService) as jasmine.SpyObj<ProductService>;
        store = TestBed.inject(MockStore);
    });

    describe('loadProducts$', () => {
        it('should return a loadProductsSuccess action', (done) => {
            const filters = {};
            const mockResponse = {
                items: [mockProduct],
                totalCount: 1,
                page: 1,
                pageSize: 10,
                totalPages: 1,
                hasNextPage: false,
                hasPreviousPage: false
            };

            productService.getProducts.and.returnValue(of(mockResponse));
            actions$ = of(ProductActions.loadProducts({ filters }));

            effects.loadProducts$.subscribe(action => {
                expect(action).toEqual(
                    ProductActions.loadProductsSuccess({
                        products: mockResponse.items,
                        totalItems: mockResponse.totalCount
                    })
                );
                done();
            });
        });

        it('should return a loadProductsFailure action on error', (done) => {
            const filters = {};
            const error = new Error('Error loading products');

            productService.getProducts.and.returnValue(throwError(() => error));
            actions$ = of(ProductActions.loadProducts({ filters }));

            effects.loadProducts$.subscribe(action => {
                expect(action).toEqual(
                    ProductActions.loadProductsFailure({ error: error.message })
                );
                done();
            });
        });
    });

    describe('addProduct$', () => {
        it('should return an addProductSuccess action', (done) => {
            const { id, ...productWithoutId } = mockProduct;

            productService.createProduct.and.returnValue(of(mockProduct));
            actions$ = of(ProductActions.addProduct({ product: productWithoutId }));

            effects.addProduct$.subscribe(action => {
                expect(action).toEqual(
                    ProductActions.addProductSuccess({ product: mockProduct })
                );
                done();
            });
        });

        it('should return an addProductFailure action on error', (done) => {
            const { id, ...productWithoutId } = mockProduct;
            const error = new Error('Error adding product');

            productService.createProduct.and.returnValue(throwError(() => error));
            actions$ = of(ProductActions.addProduct({ product: productWithoutId }));

            effects.addProduct$.subscribe(action => {
                expect(action).toEqual(
                    ProductActions.addProductFailure({ error: error.message })
                );
                done();
            });
        });
    });

    describe('updateProduct$', () => {
        it('should return an updateProductSuccess action', (done) => {
            const updatedProduct = { ...mockProduct, name: 'Updated Name' };

            productService.updateProduct.and.returnValue(of(updatedProduct));
            actions$ = of(ProductActions.updateProduct({
                id: updatedProduct.id,
                product: updatedProduct
            }));

            effects.updateProduct$.subscribe(action => {
                expect(action).toEqual(
                    ProductActions.updateProductSuccess({ product: updatedProduct })
                );
                done();
            });
        });

        it('should return an updateProductFailure action on error', (done) => {
            const updatedProduct = { ...mockProduct, name: 'Updated Name' };
            const error = new Error('Error updating product');

            productService.updateProduct.and.returnValue(throwError(() => error));
            actions$ = of(ProductActions.updateProduct({
                id: updatedProduct.id,
                product: updatedProduct
            }));

            effects.updateProduct$.subscribe(action => {
                expect(action).toEqual(
                    ProductActions.updateProductFailure({ error: error.message })
                );
                done();
            });
        });
    });

    describe('deleteProduct$', () => {
        it('should return a deleteProductSuccess action', (done) => {
            productService.deleteProduct.and.returnValue(of(void 0));
            actions$ = of(ProductActions.deleteProduct({ id: mockProduct.id }));

            effects.deleteProduct$.subscribe(action => {
                expect(action).toEqual(
                    ProductActions.deleteProductSuccess({ id: mockProduct.id })
                );
                done();
            });

        });

        it('should return a deleteProductFailure action on error', (done) => {
            const error = new Error('Error deleting product');

            productService.deleteProduct.and.returnValue(throwError(() => error));
            actions$ = of(ProductActions.deleteProduct({ id: mockProduct.id }));

            effects.deleteProduct$.subscribe(action => {
                expect(action).toEqual(
                    ProductActions.deleteProductFailure({ error: error.message })
                );
                done();
            });
        });
    });
}
);

