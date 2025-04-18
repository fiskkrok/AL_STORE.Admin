import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter, withViewTransitions } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { FormlyModule } from '@ngx-formly/core';
import { ReactiveFormsModule } from '@angular/forms';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { loadingInterceptor } from './core/interceptors/loading.interceptor';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { FormlyImageUploadTypeComponent } from './shared/formly/image-upload.type';
import { FormlyColorPickerTypeComponent } from './shared/formly/color-picker.type';
import { FileValueAccessorDirective } from './shared/formly/file-value-accessor';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { environment } from '../environments/environment';
import { productReducer } from './store/product/product.reducer';
import { ProductEffects } from './store/product/product.effects';
import { provideAnimations } from '@angular/platform-browser/animations';
import { CategoryEffects } from './store/category/category.effects';
import { categoryReducer } from './store/category/category.reducer';
import { stockReducer } from './store/stock/stock.reducer';
import { StockEffects } from './store/stock/stock.effects';
import { orderReducer } from './store/order/order.reducer';
import { OrderEffects } from './store/order/order.effects';
import { provideNativeDateAdapter } from '@angular/material/core';
import { FormlyMaterialModule } from '@ngx-formly/material';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes, withViewTransitions()),
    provideHttpClient(
      withInterceptors([
        authInterceptor,
        errorInterceptor,
        loadingInterceptor,
      ])
    ),
    // Add NgRx Store configuration
    provideStore({
      products: productReducer,
      categories: categoryReducer,
      stock: stockReducer,
      order: orderReducer,
    }),
    provideEffects([ProductEffects, CategoryEffects, StockEffects, OrderEffects]),
    provideStoreDevtools({
      maxAge: 25,
      logOnly: environment.production,
      autoPause: true,
    }),
    provideAnimations(), // Add animations provider
    provideNativeDateAdapter(), // Add date adapter provider
    importProvidersFrom(
      ReactiveFormsModule,
      FileValueAccessorDirective,
      FormlyModule.forRoot({
        types: [
          {
            name: 'file',
            component: FormlyImageUploadTypeComponent,
            wrappers: ['form-field'],
            defaultOptions: {
              templateOptions: {
                multiple: false,
                maxSize: 5000000,
                accept: 'image/*'
              }
            }
          },
          {
            name: 'color-picker',
            component: FormlyColorPickerTypeComponent,
            // wrappers: ['form-field'],
            defaultOptions: {
              validators: {
                pattern: {
                  expression: (c: any) => !c.value || /^#[0-9A-Fa-f]{6}$/.test(c.value),
                  message: 'Color must be a valid hex color (e.g. #FF0000)'
                }
              }
            }
          },
        ],
        validationMessages: [
          { name: 'required', message: 'This field is required' },
          { name: 'minLength', message: 'Minimum length not met' },
          { name: 'maxLength', message: 'Maximum length exceeded' },
          { name: 'min', message: 'Value is too small' },
          { name: 'max', message: 'Value is too large' },
          { name: 'email', message: 'Invalid email address' }
        ],
      }),
      FormlyMaterialModule,
    ),
  ]
};