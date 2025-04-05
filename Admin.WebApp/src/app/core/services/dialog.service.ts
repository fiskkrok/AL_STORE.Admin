import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface DialogConfig {
    title: string;
    message: string;
    confirmText?: string;
    cancelText?: string;
    type?: 'info' | 'warning' | 'error' | 'confirm' | 'preview';
    data?: any; // Added for preview type dialogs
}

@Injectable({
    providedIn: 'root'
})
export class DialogService {
    private readonly dialogSubject = new BehaviorSubject<DialogConfig | null>(null);
    dialog$ = this.dialogSubject.asObservable();

    private resolveRef: ((value: boolean) => void) | null = null;
    private isDialogActive = false;

    show(config: DialogConfig): Promise<boolean> {
        // If a dialog is already active, close it first
        if (this.isDialogActive) {
            this.dialogSubject.next(null);
        }

        this.isDialogActive = true;
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
        this.isDialogActive = false;
        this.dialogSubject.next(null);
    }
}