// src/app/core/services/error.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface ErrorState {
    message: string;
    type: 'error' | 'warning' | 'info';
    timestamp: Date;
}

@Injectable({
    providedIn: 'root'
})
export class ErrorService {
    private readonly errorSubject = new BehaviorSubject<ErrorState[]>([]);
    errors$ = this.errorSubject.asObservable();

    addError(error: Omit<ErrorState, 'timestamp'>) {
        const currentErrors = this.errorSubject.value;
        this.errorSubject.next([
            ...currentErrors,
            { ...error, timestamp: new Date() }
        ]);
    }

    clearErrors() {
        this.errorSubject.next([]);
    }

    removeError(index: number) {
        const currentErrors = this.errorSubject.value;
        currentErrors.splice(index, 1);
        this.errorSubject.next([...currentErrors]);
    }

    handleFormError(error: any) {
        if (error instanceof TypeError) {
            this.addError({
                message: 'Form validation error occurred. Please check your input.',
                type: 'error'
            });
        } else {
            this.addError({
                message: error.message || 'An unexpected error occurred',
                type: 'error'
            });
        }
    }
}
