import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter, withViewTransitions } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { FormlyModule } from '@ngx-formly/core';
import { FormlyBootstrapModule } from '@ngx-formly/bootstrap';
import { ReactiveFormsModule } from '@angular/forms';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { loadingInterceptor } from './core/interceptors/loading.interceptor';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { FormlyImageUploadTypeComponent } from './shared/formly/image-upload.type';
import { FileValueAccessor } from './shared/formly/file-value-accessor';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { environment } from '../environments/environment';
import { productReducer } from './store/product/product.reducer';
import { ProductEffects } from './store/product/product.effects';
import { provideAnimations } from '@angular/platform-browser/animations';

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
      products: productReducer
    }),
    provideEffects([ProductEffects]),
    provideStoreDevtools({
      maxAge: 25,
      logOnly: environment.production,
      autoPause: true,
    }),
    provideAnimations(), // Add animations provider
    importProvidersFrom(
      ReactiveFormsModule,
      FileValueAccessor,
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
      FormlyBootstrapModule,
    ),
  ]
};