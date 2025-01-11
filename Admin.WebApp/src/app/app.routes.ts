// app.routes.ts
import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home.component';
import { LoginComponent } from './features/auth/login/login.component';
import { CallbackComponent } from './features/auth/login/callback.component';
import { authenticatedWithPermissionGuard, authGuard } from './core/guards/auth.guard';
import { CategoryTreeComponent } from './features/categories/components/category-tree/category-tree.component';
import { UnauthorizedComponent } from './features/auth/unauthorized/unauthorized.component';
import { GenericErrorComponent } from './features/error-pages/generic-error/generic-error.component';
import { NetworkErrorComponent } from './features/error-pages/network-error/network-error.component';
import { NotFoundComponent } from './features/error-pages/not-found/not-found.component';
import { ServerErrorComponent } from './features/error-pages/server-error/server-error.component';

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
    path: 'unauthorized',
    component: UnauthorizedComponent,
    title: 'Access Denied'
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
        path: 'dashboards',
        loadChildren: () => import('./features/dashboard/dashboards.routes')
          .then(m => m.DASHBOARD_ROUTES)
      },
      {
        path: 'categories',
        component: CategoryTreeComponent,
        canActivate: [authGuard]
      },
      {
        path: 'orders',
        loadChildren: () => import('./features/orders/orders.routes')
          .then(m => m.ORDER_ROUTES)
      },
      // Error routes
      {
        path: 'error',
        children: [
          {
            path: '404',
            component: NotFoundComponent,
            title: 'Page Not Found'
          },
          {
            path: '500',
            component: ServerErrorComponent,
            title: 'Server Error'
          },
          {
            path: 'network',
            component: NetworkErrorComponent,
            title: 'Network Error'
          },
          {
            path: 'generic',
            component: GenericErrorComponent,
            title: 'Error'
          }
        ]
      },

      // 404 catch-all route
      {
        path: '**',
        component: NotFoundComponent,
        title: 'Page Not Found'
      }
    ]
  }
];