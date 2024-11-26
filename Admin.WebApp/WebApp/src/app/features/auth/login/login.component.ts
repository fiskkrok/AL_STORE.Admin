// login.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [CommonModule],
    template: `
        <div class="container">
            <div class="row justify-content-center mt-5">
                <div class="col-md-6 text-center">
                    <div class="card">
                        <div class="card-body">
                            <h2 class="card-title mb-4">Welcome to AL Store Admin</h2>
                            <p class="card-text">Please sign in to continue</p>
                            <button 
                                class="btn btn-primary btn-lg"
                                (click)="login()"
                                [disabled]="loading">
                                {{ loading ? 'Loading...' : 'Sign In' }}
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `,
    styles: [`
        .card {
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            border: none;
            padding: 2rem;
        }
        
        .btn-lg {
            padding: 1rem 2rem;
            font-size: 1.2rem;
        }
    `]
})
export class LoginComponent {
    loading = false;

    constructor(private authService: AuthService) { }

    async login() {
        this.loading = true;
        try {
            await this.authService.login();
            // The login method will redirect to Identity Server,
            // so we don't need to handle navigation here
        } catch (error) {
            console.error('Login error:', error);
            this.loading = false;
        }
    }
}