// src/app/core/services/error.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { ApiError, ValidationError } from '../../shared/models/error.models';
import { HttpErrorResponse } from '@angular/common/http';
import { v4 as uuidv4 } from 'uuid';

@Injectable({ providedIn: 'root' })
export class ErrorService {
    private readonly errorSubject = new BehaviorSubject<ApiError[]>([]);
    readonly errors$ = this.errorSubject.asObservable();

    // Keep track of validation errors separately
    private readonly validationErrorsSubject = new BehaviorSubject<ValidationError[]>([]);
    readonly validationErrors$ = this.validationErrorsSubject.asObservable();

    // Error Categories - helps with error handling strategies
    private readonly ERROR_CATEGORIES = {
        VALIDATION: ['VALIDATION_ERROR', 'INVALID_INPUT'],
        NETWORK: ['NETWORK_ERROR', 'TIMEOUT'],
        AUTH: ['UNAUTHORIZED', 'FORBIDDEN'],
        SERVER: ['SERVER_ERROR', 'DATABASE_ERROR'],
        BUSINESS: ['BUSINESS_RULE_VIOLATION', 'INSUFFICIENT_STOCK']
    };

    /**
     * Handle HTTP errors consistently
     * @param error HTTP error response
     */
    handleHttpError(error: HttpErrorResponse): void {
        if (error.status === 422 || (error.error && error.error.errors)) {
            // Handle validation errors
            this.handleValidationError(error.error);
        } else {
            const apiError = this.createApiError(error);
            this.addError(apiError);
        }
    }

    /**
     * Parse and handle validation errors
     * @param errorResponse Error response with validation errors
     */
    handleValidationError(errorResponse: any): void {
        const validationErrors = this.parseValidationErrors(errorResponse);
        this.validationErrorsSubject.next(validationErrors);

        // Also add a general validation error to the main errors
        if (validationErrors.length > 0) {
            this.addError({
                code: 'VALIDATION_ERROR',
                message: 'Please correct the validation errors',
                severity: 'warning',
                details: `${validationErrors.length} validation errors found`
            });
        }
    }

    /**
     * Parse validation errors from server response
     * @param errorResponse Server error response
     * @returns Array of ValidationError objects
     */
    private parseValidationErrors(errorResponse: any): ValidationError[] {
        // Handle different validation error formats
        if (Array.isArray(errorResponse.errors)) {
            return errorResponse.errors.map((error: any) => ({
                field: error.field || error.property,
                message: error.message || error.errorMessage,
                code: error.code || 'VALIDATION_ERROR'
            }));
        } else if (typeof errorResponse.errors === 'object') {
            // Handle .NET ModelState errors format
            return Object.keys(errorResponse.errors).map(key => ({
                field: key,
                message: Array.isArray(errorResponse.errors[key])
                    ? errorResponse.errors[key][0]
                    : errorResponse.errors[key],
                code: 'VALIDATION_ERROR'
            }));
        }
        return [];
    }

    /**
     * Add an error to the errors list
     * @param error Error details
     */
    addError(error: Omit<ApiError, 'id' | 'timestamp'>): void {
        const apiError: ApiError = {
            id: uuidv4(),
            timestamp: new Date(),
            ...error
        };

        const currentErrors = this.errorSubject.value;
        this.errorSubject.next([...currentErrors, apiError]);

        // Auto-dismiss after delay based on severity
        const dismissDelay = this.getDismissDelay(error.severity);
        if (dismissDelay) {
            setTimeout(() => this.removeError(apiError.id), dismissDelay);
        }
    }

    /**
     * Get auto-dismiss delay based on error severity
     * @param severity Error severity level
     * @returns Delay in milliseconds
     */
    private getDismissDelay(severity: ApiError['severity']): number {
        switch (severity) {
            case 'error': return 0;  // Don't auto-dismiss errors
            case 'warning': return 5000;  // 5 seconds
            case 'info': return 3000;  // 3 seconds
            default: return 3000;
        }
    }

    /**
     * Remove an error by ID
     * @param id Error ID
     */
    removeError(id: string): void {
        const currentErrors = this.errorSubject.value;
        this.errorSubject.next(currentErrors.filter(error => error.id !== id));
    }

    /**
     * Clear all errors
     */
    clearAllErrors(): void {
        this.errorSubject.next([]);
        this.validationErrorsSubject.next([]);
    }

    /**
     * Create a standardized API error from HTTP error
     * @param httpError HTTP error response
     * @returns Standardized API error
     */
    private createApiError(httpError: HttpErrorResponse): ApiError {
        const errorResponse = httpError.error;

        return {
            id: uuidv4(),
            code: errorResponse?.code || this.getErrorCodeFromStatus(httpError.status),
            message: errorResponse?.message || this.getDefaultMessage(httpError),
            details: errorResponse?.details,
            timestamp: new Date(),
            severity: this.getSeverityFromStatus(httpError.status),
            correlationId: errorResponse?.correlationId
        };
    }

    /**
     * Get error code based on HTTP status
     * @param status HTTP status code
     * @returns Error code string
     */
    private getErrorCodeFromStatus(status: number): string {
        switch (status) {
            case 400: return 'BAD_REQUEST';
            case 401: return 'UNAUTHORIZED';
            case 403: return 'FORBIDDEN';
            case 404: return 'NOT_FOUND';
            case 500: return 'SERVER_ERROR';
            default: return 'UNKNOWN_ERROR';
        }
    }

    /**
     * Get default error message based on HTTP status
     * @param error HTTP error response
     * @returns User-friendly error message
     */
    private getDefaultMessage(error: HttpErrorResponse): string {
        switch (error.status) {
            case 400: return 'The request was invalid. Please check your input.';
            case 401: return 'You need to log in to access this resource.';
            case 403: return 'You don\'t have permission to access this resource.';
            case 404: return 'The requested resource was not found.';
            case 500: return 'An unexpected server error occurred. Please try again later.';
            default: return 'An unexpected error occurred. Please try again.';
        }
    }

    /**
     * Determine error severity based on HTTP status
     * @param status HTTP status code
     * @returns Error severity level
     */
    private getSeverityFromStatus(status: number): ApiError['severity'] {
        if (status >= 500) return 'error';
        if (status >= 400) return 'warning';
        return 'info';
    }

    /**
     * Get field-specific validation errors
     * @param fieldName Form field name
     * @returns Validation errors for the field
     */
    getFieldErrors(fieldName: string): ValidationError[] {
        return this.validationErrorsSubject.value.filter(error => error.field === fieldName);
    }

    /**
     * Check if a field has validation errors
     * @param fieldName Form field name
     * @returns Boolean indicating if field has errors
     */
    hasFieldErrors(fieldName: string): boolean {
        return this.getFieldErrors(fieldName).length > 0;
    }
}