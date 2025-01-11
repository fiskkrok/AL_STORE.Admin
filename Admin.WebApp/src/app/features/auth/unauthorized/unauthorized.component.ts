// src/app/features/auth/unauthorized/unauthorized.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'app-unauthorized',
    standalone: true,
    imports: [CommonModule, MatButtonModule, MatIconModule],
    template: `
        <div class="unauthorized-container">
            <div class="unauthorized-content">
                <mat-icon class="error-icon">security</mat-icon>
                
                <h1>Access Denied</h1>
                
                <p class="message">
                    Sorry, you don't have permission to access this page. 
                    If you believe this is a mistake, please contact your administrator.
                </p>
                
                <div class="action-buttons">
                    <button 
                        mat-stroked-button 
                        (click)="goBack()"
                    >
                        Go Back
                    </button>
                    
                    <button 
                        mat-flat-button 
                        color="primary"
                        (click)="goHome()"
                    >
                        Return Home
                    </button>
                </div>
            </div>
        </div>
    `,
    styles: [`
        .unauthorized-container {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            min-height: 80vh;
            padding: 1rem;
            text-align: center;
        }

        .unauthorized-content {
            max-width: 400px;
        }

        .error-icon {
            font-size: 64px;
            height: 64px;
            width: 64px;
            color: var(--error);
            margin-bottom: 1.5rem;
        }

        h1 {
            font-size: 2rem;
            font-weight: bold;
            margin-bottom: 1rem;
            color: var(--text-primary);
        }

        .message {
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
export class UnauthorizedComponent {
    constructor(private router: Router) { }

    goBack() {
        window.history.back();
    }

    goHome() {
        this.router.navigate(['/']);
    }
}