import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideRouter } from '@angular/router';
import { routes } from './app/app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { SignalRService } from './app/core/services/signalr.service';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { authInterceptor } from './app/core/interceptors/auth.interceptor';
import { errorInterceptor } from './app/core/interceptors/error.interceptor';
import { loadingInterceptor } from './app/core/interceptors/loading.interceptor';

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([
        authInterceptor,  // Make sure this is here
        errorInterceptor,
        loadingInterceptor,
      ])
    ),
    SignalRService,
    provideAnimationsAsync(), provideAnimationsAsync()
  ]
}).catch(err => console.error(err));