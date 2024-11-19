import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ErrorService } from '../services/error.service';
import { Router } from '@angular/router';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const errorService = inject(ErrorService);
    const router = inject(Router);

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            let errorMessage = 'An unexpected error occurred';

            if (error.error instanceof ErrorEvent) {
                // Client-side error
                errorMessage = error.error.message;
            } else {
                // Server-side error
                switch (error.status) {
                    case 400:
                        errorMessage = error.error?.message || 'Invalid request';
                        break;
                    case 401:
                        errorMessage = 'Unauthorized access';
                        router.navigate(['/login']);
                        break;
                    case 403:
                        errorMessage = 'Access forbidden';
                        break;
                    case 404:
                        errorMessage = 'Resource not found';
                        break;
                    case 422:
                        errorMessage = 'Validation error';
                        break;
                    case 500:
                        errorMessage = 'Server error';
                        break;
                    default:
                        errorMessage = `Error: ${error.status} - ${error.message}`;
                }
            }

            errorService.addError({
                message: errorMessage,
                type: 'error'
            });

            return throwError(() => error);
        })
    );
};