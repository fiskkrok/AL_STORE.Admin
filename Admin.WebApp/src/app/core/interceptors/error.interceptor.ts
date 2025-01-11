// src/app/core/interceptors/error.interceptor.ts
import { HttpInterceptorFn, HttpStatusCode, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { ErrorService } from '../services/error.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const errorService = inject(ErrorService);
    const router = inject(Router);

    return next(req).pipe(
        catchError((error: unknown) => {
            if (error instanceof HttpErrorResponse) {
                // Handle different types of errors
                switch (error.status) {
                    case 0:
                        // Network error or CORS
                        router.navigate(['/error/network']);
                        break;

                    case HttpStatusCode.NotFound:
                        router.navigate(['/error/404']);
                        break;

                    case HttpStatusCode.InternalServerError:
                        router.navigate(['/error/500']);
                        break;

                    case HttpStatusCode.Unauthorized:
                        // Let auth interceptor handle this
                        break;

                    case HttpStatusCode.Forbidden:
                        router.navigate(['/unauthorized']);
                        break;

                    default:
                        // For other errors, show the error message
                        router.navigate(['/error/generic'], {
                            queryParams: {
                                message: error.error?.message || 'An unexpected error occurred'
                            }
                        });
                }

                // Add error to error service
                errorService.addError({
                    code: error.error?.code || `HTTP_${error.status}`,
                    message: error.error?.message || error.message,
                    severity: error.status >= 500 ? 'error' : 'warning'
                });
            } else {
                // Handle non-HTTP errors
                console.error('Non-HTTP Error:', error);
                router.navigate(['/error/generic']);

                errorService.addError({
                    code: 'UNKNOWN_ERROR',
                    message: 'An unexpected error occurred',
                    severity: 'error'
                });
            }

            return throwError(() => error);
        })
    );
};