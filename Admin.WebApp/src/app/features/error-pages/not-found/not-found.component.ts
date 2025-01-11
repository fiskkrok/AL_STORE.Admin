import { Component } from "@angular/core";
import { ErrorConfig, ErrorPageComponent } from "../error-page.component";

// src/app/features/error-pages/not-found/not-found.component.ts
@Component({
    selector: 'app-not-found',
    standalone: true,
    imports: [ErrorPageComponent],
    template: `
        <app-error-page [config]="errorConfig"></app-error-page>
    `
})
export class NotFoundComponent {
    errorConfig: ErrorConfig = {
        title: 'Page Not Found',
        message: 'Sorry, the page you are looking for doesn\'t exist or has been moved.',
        icon: 'search_off',
        showHomeButton: true
    };
}