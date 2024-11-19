import { Routes } from '@angular/router';
import { ProductsComponent } from './products.component';
import { AddProductComponent } from './add-product/add-product.component';
import { ProductListComponent } from './product-list/product-list.component';
import { RemoveProductComponent } from './remove-product/remove-product.component';

export const PRODUCT_ROUTES: Routes = [
    {
        path: '',
        component: ProductsComponent,
        children: [
            {
                path: 'add',
                component: AddProductComponent,
                title: 'Add Product'
            },
            {
                path: 'list',
                component: ProductListComponent,
                title: 'Product List'
            },
            {
                path: 'remove',
                component: RemoveProductComponent,
                title: 'Remove Product'
            },
            {
                path: '',
                redirectTo: 'list',
                pathMatch: 'full'
            }
        ]
    }
];