// src/app/core/services/error.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface ErrorState {
    message: string;
    type: 'error' | 'warning' | 'info';
    timestamp: Date;
}

@Injectable({
    providedIn: 'root'
})
export class ErrorService {
    private errorSubject = new BehaviorSubject<ErrorState[]>([]);
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
}
