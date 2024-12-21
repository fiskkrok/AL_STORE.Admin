// app.routes.ts
import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home.component';
import { LoginComponent } from './features/auth/login/login.component';
import { CallbackComponent } from './features/auth/login/callback.component';
import { authGuard } from './core/guards/auth.guard';
import { CategoryTreeComponent } from './features/categories/components/category-tree/category-tree.component';

export const routes: Routes = [
  {
    path: 'callback',
    component: CallbackComponent
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        component: HomeComponent
      },
      {
        path: 'products',
        loadChildren: () => import('./features/products/products.routes')
          .then(m => m.PRODUCT_ROUTES)
      },
      {
        path: 'statistics',
        loadChildren: () => import('./features/statistics/statistics.routes')
          .then(m => m.STATISTICS_ROUTES)
      },
      {
        path: 'categories',
        component: CategoryTreeComponent,
        canActivate: [authGuard]
      }
    ]
  }
];