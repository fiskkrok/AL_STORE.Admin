import { createActionGroup, props, emptyProps } from '@ngrx/store';
import { Product } from '../../shared/models/product.model';
import { ProductFilters } from '../../core/services/product.service';

export const ProductActions = createActionGroup({
    source: 'Product',
    events: {
        // Load Products
        'Load Products': props<{ filters: ProductFilters }>(),
        'Load Products Success': props<{ products: Product[]; totalItems: number }>(),
        'Load Products Failure': props<{ error: string }>(),

        // Add Product
        'Add Product': props<{ product: Omit<Product, 'id'> }>(),
        'Add Product Success': props<{ product: Product }>(),
        'Add Product Failure': props<{ error: string }>(),

        // Update Product
        'Update Product': props<{ id: string; product: Partial<Product> }>(),
        'Update Product Success': props<{ product: Product }>(),
        'Update Product Failure': props<{ error: string }>(),

        // Optimistic Update Actions
        'Optimistic Update Product': props<{ id: string; changes: Partial<Product> }>(),
        'Revert Optimistic Update': emptyProps(),

        // Delete Product
        'Delete Product': props<{ id: string }>(),
        'Delete Product Success': props<{ id: string }>(),
        'Delete Product Failure': props<{ error: string }>(),

        // Selection
        'Select Product': props<{ product: Product }>(),
        'Clear Selected Product': emptyProps(),

        // Filters
        'Set Filters': props<{ filters: Partial<ProductFilters> }>(),
        'Reset Filters': emptyProps(),

        // Cache
        'Set Cache Timestamp': props<{ timestamp: number }>(),
        'Clear Cache': emptyProps()
    }
});