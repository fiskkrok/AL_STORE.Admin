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
                        errorService.addError({
                            code: 'NETWORK_ERROR',
                            message: 'Unable to connect to the server. Please check your connection.',
                            severity: 'error'
                        });
                        router.navigate(['/error/network']);
                        break;

                    case HttpStatusCode.NotFound:
                        errorService.addError({
                            code: 'NOT_FOUND',
                            message: `The requested resource at ${req.url} was not found.`,
                            severity: 'warning'
                        });
                        break;

                    case HttpStatusCode.InternalServerError:
                        errorService.addError({
                            code: 'SERVER_ERROR',
                            message: 'An unexpected server error occurred. Please try again later.',
                            severity: 'error'
                        });
                        router.navigate(['/error/500']);
                        break;

                    case HttpStatusCode.Unauthorized:
                        // Let auth interceptor handle this
                        break;

                    case HttpStatusCode.Forbidden:
                        errorService.addError({
                            code: 'FORBIDDEN',
                            message: 'You do not have permission to access this resource.',
                            severity: 'warning'
                        });
                        router.navigate(['/unauthorized']);
                        break;

                    case HttpStatusCode.BadRequest:
                        // Handle validation errors specially
                        if (error.error?.validation) {
                            errorService.handleValidationError(error.error);
                        } else {
                            errorService.addError({
                                code: error.error?.code || 'BAD_REQUEST',
                                message: error.error?.message || 'The request was invalid.',
                                severity: 'warning'
                            });
                        }
                        break;

                    default:
                        // For other errors, add a generic error
                        errorService.addError({
                            code: error.error?.code || `HTTP_${error.status}`,
                            message: error.error?.message || 'An unexpected error occurred',
                            severity: error.status >= 500 ? 'error' : 'warning',
                            details: error.error?.details
                        });
                }
            } else {
                // Handle non-HTTP errors
                console.error('Non-HTTP Error:', error);
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