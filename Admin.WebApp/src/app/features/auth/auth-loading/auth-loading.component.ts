// src/app/features/auth/components/auth-loading/auth-loading.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';

@Component({
    selector: 'app-auth-loading',
    standalone: true,
    imports: [CommonModule, LoadingSpinnerComponent],
    template: `
        @if (authState$ | async; as state) {
            @if (state.loading) {
                <div class="auth-loading-container">
                    <app-loading-spinner
                        [message]="state.loadingMessage || 'Authenticating...'"
                        [color]="'primary'"
                        [size]="36">
                    </app-loading-spinner>
                </div>
            }
        }
    `,
    styles: [`
        .auth-loading-container {
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 200px;
        }
    `]
})
export class AuthLoadingComponent {
    authState$;

    constructor(private readonly authService: AuthService) {
        this.authState$ = this.authService.authState$;
    }
}