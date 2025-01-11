import { Component, Input } from "@angular/core";
import { ErrorConfig, ErrorPageComponent } from "../error-page.component";

// src/app/features/error-pages/generic-error/generic-error.component.ts
@Component({
    selector: 'app-generic-error',
    standalone: true,
    imports: [ErrorPageComponent],
    template: `
        <app-error-page [config]="errorConfig"></app-error-page>
    `
})
export class GenericErrorComponent {
    @Input() message?: string;

    get errorConfig(): ErrorConfig {
        return {
            title: 'Error',
            message: this.message || 'An unexpected error occurred. Please try again.',
            icon: 'warning',
            showRetryButton: true,
            showHomeButton: true
        };
    }
}