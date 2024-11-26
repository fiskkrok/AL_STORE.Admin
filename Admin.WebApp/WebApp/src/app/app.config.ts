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