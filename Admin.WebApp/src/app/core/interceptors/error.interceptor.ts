// src/app/core/interceptors/error.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { ErrorService } from '../services/error.service';
import { Router } from '@angular/router';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const errorService = inject(ErrorService);
    const router = inject(Router);

    return next(req).pipe(
        catchError(error => {
            // Handle specific error cases
            if (error.status === 401) {
                router.navigate(['/login']);
            }

            errorService.handleHttpError(error);
            return throwError(() => error);
        })
    );
};