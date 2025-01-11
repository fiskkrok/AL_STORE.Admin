// src/app/core/interceptors/auth.interceptor.ts
import { HttpInterceptorFn, HttpStatusCode, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);

    // Skip token injection for non-API and auth endpoints
    if (!req.url.startsWith(environment.apiUrls.admin.baseUrl) ||
        req.url.includes('/auth/')) {
        return next(req);
    }

    return authService.getAccessToken().pipe(
        switchMap(token => {
            if (token) {
                const authReq = req.clone({
                    headers: req.headers.set('Authorization', `Bearer ${token}`)
                });
                return next(authReq);
            }
            return next(req);
        }),
        catchError((error: HttpErrorResponse) => {
            // Handle authentication errors
            if (error.status === HttpStatusCode.Unauthorized) {
                // Try refreshing token silently
                return authService.refreshToken().pipe(
                    switchMap(success => {
                        if (success) {
                            // Retry the original request
                            return authService.getAccessToken().pipe(
                                switchMap(newToken => {
                                    const retryReq = req.clone({
                                        headers: req.headers.set('Authorization', `Bearer ${newToken}`)
                                    });
                                    return next(retryReq);
                                })
                            );
                        }
                        // If refresh failed, logout and redirect to login
                        authService.logout();
                        return throwError(() => error);
                    })
                );
            }

            // Handle forbidden errors
            if (error.status === HttpStatusCode.Forbidden) {
                console.error('Access forbidden:', req.url);
                // You might want to redirect to an error page or show a notification
            }

            return throwError(() => error);
        })
    );
};