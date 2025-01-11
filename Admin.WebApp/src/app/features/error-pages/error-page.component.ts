// src/app/features/error-pages/error-page.component.ts
import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ErrorConfig {
    title: string;
    message: string;
    icon: string;
    actionText?: string;
    showHomeButton?: boolean;
    showRetryButton?: boolean;
}

@Component({
    selector: 'app-error-page',
    standalone: true,
    imports: [CommonModule, MatButtonModule, MatIconModule],
    template: `
        <div class="error-container">
            <div class="error-content">
                <mat-icon class="error-icon" [ngClass]="config.icon">{{config.icon}}</mat-icon>
                
                <h1>{{config.title}}</h1>
                <p class="error-message">{{config.message}}</p>
                
                <div class="action-buttons">
                    @if (config.showRetryButton) {
                        <button 
                            mat-stroked-button 
                            (click)="retryLastAction()"
                        >
                            Try Again
                        </button>
                    }
                    
                    @if (config.showHomeButton) {
                        <button 
                            mat-flat-button 
                            color="primary"
                            (click)="goHome()"
                        >
                            Return Home
                        </button>
                    }
                </div>
            </div>
        </div>
    `,
    styles: [`
        .error-container {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            min-height: 80vh;
            padding: 1rem;
            text-align: center;
        }

        .error-content {
            max-width: 400px;
        }

        .error-icon {
            font-size: 64px;
            height: 64px;
            width: 64px;
            margin-bottom: 1.5rem;

            &.error { color: var(--error); }
            &.warning { color: var(--warning); }
            &.info { color: var(--info); }
        }

        h1 {
            font-size: 2rem;
            font-weight: bold;
            margin-bottom: 1rem;
            color: var(--text-primary);
        }

        .error-message {
            color: var(--text-secondary);
            margin-bottom: 2rem;
        }

        .action-buttons {
            display: flex;
            gap: 1rem;
            justify-content: center;
        }
    `]
})
export class ErrorPageComponent {
    @Input() config!: ErrorConfig;

    constructor(private router: Router) { }

    retryLastAction() {
        window.location.reload();
    }

    goHome() {
        this.router.navigate(['/']);
    }
}






