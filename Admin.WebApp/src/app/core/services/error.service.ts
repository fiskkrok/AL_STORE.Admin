import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { ApiError, ValidationError } from '../models/error.models';
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

    handleHttpError(error: HttpErrorResponse): void {
        if (error.status === 422) {
            // Handle validation errors
            this.handleValidationError(error.error);
        } else {
            const apiError = this.createApiError(error);
            this.addError(apiError);
        }
    }

    private handleValidationError(errorResponse: any): void {
        const validationErrors = this.parseValidationErrors(errorResponse);
        this.validationErrorsSubject.next(validationErrors);
    }

    private parseValidationErrors(errorResponse: any): ValidationError[] {
        if (Array.isArray(errorResponse.errors)) {
            return errorResponse.errors.map((error: any) => ({
                field: error.field,
                message: error.message,
                code: error.code || 'VALIDATION_ERROR'
            }));
        }
        return [];
    }

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

    private getDismissDelay(severity: ApiError['severity']): number {
        switch (severity) {
            case 'error': return 0;  // Don't auto-dismiss errors
            case 'warning': return 5000;  // 5 seconds
            case 'info': return 3000;  // 3 seconds
            default: return 3000;
        }
    }

    removeError(id: string): void {
        const currentErrors = this.errorSubject.value;
        this.errorSubject.next(currentErrors.filter(error => error.id !== id));
    }

    clearAllErrors(): void {
        this.errorSubject.next([]);
        this.validationErrorsSubject.next([]);
    }

    private createApiError(httpError: HttpErrorResponse): ApiError {
        const errorResponse = httpError.error;

        return {
            id: uuidv4(),
            code: errorResponse.code || this.getErrorCodeFromStatus(httpError.status),
            message: errorResponse.message || this.getDefaultMessage(httpError),
            details: errorResponse.details,
            timestamp: new Date(),
            severity: this.getSeverityFromStatus(httpError.status),
            correlationId: errorResponse.correlationId
        };
    }

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

    private getDefaultMessage(error: HttpErrorResponse): string {
        switch (error.status) {
            case 400: return 'Invalid request. Please check your input.';
            case 401: return 'You need to log in to access this resource.';
            case 403: return 'You don\'t have permission to access this resource.';
            case 404: return 'The requested resource was not found.';
            case 500: return 'An unexpected server error occurred.';
            default: return 'An unexpected error occurred.';
        }
    }

    private getSeverityFromStatus(status: number): ApiError['severity'] {
        if (status >= 500) return 'error';
        if (status >= 400) return 'warning';
        return 'info';
    }
}