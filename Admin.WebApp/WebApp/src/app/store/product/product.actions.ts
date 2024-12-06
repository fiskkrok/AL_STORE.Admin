import { createActionGroup, props } from '@ngrx/store';
import { Product } from '../../shared/models/product.model';
import { ProductFilters } from '../../core/services/product.service';

export const ProductActions = createActionGroup({
    source: 'Product',
    events: {
        'Load Products': props<{ filters: ProductFilters }>(),
        'Load Products Success': props<{ products: Product[]; totalItems: number }>(),
        'Load Products Failure': props<{ error: string }>(),

        'Add Product': props<{ product: Omit<Product, 'id'> }>(),
        'Add Product Success': props<{ product: Product }>(),
        'Add Product Failure': props<{ error: string }>(),

        'Update Product': props<{ id: string; product: Partial<Product> }>(),
        'Update Product Success': props<{ product: Product }>(),
        'Update Product Failure': props<{ error: string }>(),

        'Delete Product': props<{ id: string }>(),
        'Delete Product Success': props<{ id: string }>(),
        'Delete Product Failure': props<{ error: string }>(),

        'Select Product': props<{ product: Product }>(),
        'Clear Selected Product': props<void>(),

        'Set Filters': props<{ filters: Partial<ProductFilters> }>(),
        'Reset Filters': props<void>(),
    }
});
