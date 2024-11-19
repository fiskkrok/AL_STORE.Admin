import { Injectable, Component } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface DialogConfig {
    title: string;
    message: string;
    confirmText?: string;
    cancelText?: string;
    type?: 'info' | 'warning' | 'error' | 'confirm';
}

@Injectable({
    providedIn: 'root'
})
export class DialogService {
    private dialogSubject = new BehaviorSubject<DialogConfig | null>(null);
    dialog$ = this.dialogSubject.asObservable();

    private resolveRef: ((value: boolean) => void) | null = null;

    show(config: DialogConfig): Promise<boolean> {
        this.dialogSubject.next(config);
        return new Promise((resolve) => {
            this.resolveRef = resolve;
        });
    }

    confirm(message: string, title = 'Confirm'): Promise<boolean> {
        return this.show({
            title,
            message,
            type: 'confirm',
            confirmText: 'Confirm',
            cancelText: 'Cancel'
        });
    }

    handleAction(confirmed: boolean) {
        if (this.resolveRef) {
            this.resolveRef(confirmed);
            this.resolveRef = null;
        }
        this.dialogSubject.next(null);
    }
}