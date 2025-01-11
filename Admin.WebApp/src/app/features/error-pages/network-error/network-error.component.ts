import { Component } from "@angular/core";
import { ErrorConfig, ErrorPageComponent } from "../error-page.component";

// src/app/features/error-pages/network-error/network-error.component.ts
@Component({
    selector: 'app-network-error',
    standalone: true,
    imports: [ErrorPageComponent],
    template: `
        <app-error-page [config]="errorConfig"></app-error-page>
    `
})
export class NetworkErrorComponent {
    errorConfig: ErrorConfig = {
        title: 'Connection Error',
        message: 'Unable to connect to the server. Please check your internet connection and try again.',
        icon: 'wifi_off',
        showRetryButton: true
    };
}