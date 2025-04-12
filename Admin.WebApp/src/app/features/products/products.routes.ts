// src/app/features/products/products.routes.ts
import { Routes } from '@angular/router';
import { ProductsComponent } from './products.component';
import { ProductListComponent } from './product-list/product-list.component';
import { ProductFormComponent } from './pages/product-form.component';

export const PRODUCT_ROUTES: Routes = [
    {
        path: '',
        component: ProductsComponent,
        children: [
            {
                path: 'list',
                component: ProductListComponent,
                title: 'Product List'
            },
            {
                path: 'add',
                component: ProductFormComponent,
                title: 'Add Product'
            },
            {
                path: 'edit/:id',
                component: ProductFormComponent,
                title: 'Edit Product'
            },
            {
                path: 'import',
                component: ProductFormComponent,
                title: 'Import Products',
                data: { tab: 'bulk' }
            },
            {
                path: 'scan',
                component: ProductFormComponent,
                title: 'Scan Products',
                data: { tab: 'scanner' }
            },
            {
                path: '',
                redirectTo: 'list',
                pathMatch: 'full'
            }
        ]
    }
];