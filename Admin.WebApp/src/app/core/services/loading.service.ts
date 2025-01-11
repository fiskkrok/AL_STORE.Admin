// src/app/core/services/loading.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface LoadingState {
    loading: boolean;
    message?: string | null;
}

@Injectable({
    providedIn: 'root'
})
export class LoadingService {
    private loadingSubject = new BehaviorSubject<LoadingState>({
        loading: false,
        message: null
    });

    loading$ = this.loadingSubject.asObservable();
    private loadingCount = 0;

    show(message?: string) {
        this.loadingCount++;
        if (this.loadingCount === 1) {
            this.loadingSubject.next({
                loading: true,
                message
            });
        }
    }

    hide() {
        this.loadingCount--;
        if (this.loadingCount <= 0) {
            this.loadingCount = 0;
            this.loadingSubject.next({
                loading: false,
                message: null
            });
        }
    }

    // Force reset loading state
    reset() {
        this.loadingCount = 0;
        this.loadingSubject.next({
            loading: false,
            message: null
        });
    }
}