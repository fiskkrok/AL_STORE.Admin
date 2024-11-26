// auth.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { switchMap } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);

    return authService.getAccessToken().pipe(
        switchMap(token => {
            // Add logging to debug
            console.log('Token:', token);
            console.log('Request URL:', req.url);

            if (token) {
                const authReq = req.clone({
                    headers: req.headers.set('Authorization', `Bearer ${token}`)
                });
                return next(authReq);
            }
            return next(req);
        })
    );
};