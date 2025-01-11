// src/app/shared/components/loading-spinner/loading-spinner.component.ts
import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule, MatProgressSpinnerModule],
  template: `
        <div class="loading-container" [class.overlay]="overlay" [class.inline]="!overlay">
            <mat-spinner 
                [diameter]="size" 
                [strokeWidth]="strokeWidth"
                [color]="color">
            </mat-spinner>
            @if (message) {
                <span class="loading-message">{{ message }}</span>
            }
        </div>
    `,
  styles: [`
        .loading-container {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            gap: 1rem;
        }

        .overlay {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background-color: rgba(0, 0, 0, 0.5);
            z-index: 9999;
            
            .loading-message {
                color: white;
            }
        }

        .inline {
            padding: 1rem;
            
            .loading-message {
                color: var(--text-secondary);
            }
        }

        .loading-message {
            font-size: 0.875rem;
        }
    `]
})
export class LoadingSpinnerComponent {
  @Input() size = 40;
  @Input() strokeWidth = 4;
  @Input() color: 'primary' | 'accent' | 'warn' = 'primary';
  @Input() message?: string;
  @Input() overlay = false;
}