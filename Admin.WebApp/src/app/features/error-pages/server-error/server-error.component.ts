import { Component } from "@angular/core";
import { ErrorConfig, ErrorPageComponent } from "../error-page.component";

// src/app/features/error-pages/server-error/server-error.component.ts
@Component({
    selector: 'app-server-error',
    standalone: true,
    imports: [ErrorPageComponent],
    template: `
        <app-error-page [config]="errorConfig"></app-error-page>
    `
})
export class ServerErrorComponent {
    errorConfig: ErrorConfig = {
        title: 'Server Error',
        message: 'Sorry, something went wrong on our end. Please try again later.',
        icon: 'error',
        showRetryButton: true,
        showHomeButton: true
    };
}