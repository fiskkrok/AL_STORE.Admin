// callback.component.ts
import { Component, OnInit } from '@angular/core';
import { AuthService } from 'src/app/core/services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-callback',
    standalone: true,
    imports: [CommonModule],
    template: `
        <div class="d-flex justify-content-center align-items-center" style="height: 100vh;">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    `
})
export class CallbackComponent implements OnInit {
    constructor(private authService: AuthService) { }

    ngOnInit() {
        this.authService.completeAuthentication()
            .catch(error => console.error('Error in callback:', error));
    }
}