// src/app/shared/components/error-toast/error-toast.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ErrorService } from '../../../core/services/error.service';
import { ApiError } from '../../../core/models/error.models';
import { animate, style, transition, trigger } from '@angular/animations';

@Component({
  selector: 'app-error-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed bottom-4 right-4 z-50 flex flex-col gap-2">
      @for (error of errors; track error.id) {
        <div 
          class="error-toast" 
          [ngClass]="getToastClass(error)"
          [@slideIn]
        >
          <div class="flex items-center gap-2">
            <i [class]="getIconClass(error)" (click)="removeError(error)"></i>
            <div class="flex-1">
              <h4 class="font-semibold">{{ error.code }}</h4>
              <p>{{ error.message }}</p>
              @if (error.details) {
                <p class="text-sm opacity-75">{{ error.details }}</p>
              }
            </div>
            <!-- <button 
              (click)="removeError(error)"
              class="text-current opacity-50 hover:opacity-100"
            >
              <i class="bi bi-x text-xl"></i>
            </button> -->
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
   i:hover {
      cursor: pointer;
    }
    .error-toast {
      @apply p-4 rounded-lg shadow-lg max-w-md w-full;
      
      &.error {
        @apply bg-red-100 text-red-800;
      }
      
      &.warning {
        @apply bg-yellow-100 text-yellow-800;
      }
      
      &.info {
        @apply bg-blue-100 text-blue-800;
      }
    }
  `],
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateX(100%)', opacity: 0 }),
        animate('200ms ease-out', style({ transform: 'translateX(0)', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ transform: 'translateX(100%)', opacity: 0 }))
      ])
    ])
  ]
})
export class ErrorToastComponent implements OnInit {
  errors: ApiError[] = [];

  constructor(private errorService: ErrorService) { }

  ngOnInit() {
    this.errorService.errors$.subscribe(errors => {
      this.errors = errors;
    });
  }

  removeError(error: ApiError) {
    this.errorService.removeError(error.id);
  }

  getToastClass(error: ApiError): string {
    return error.severity;
  }

  getIconClass(error: ApiError): string {
    switch (error.severity) {
      case 'error': return 'bi bi-x-circle';
      case 'warning': return 'bi bi-exclamation-triangle';
      case 'info': return 'bi bi-info-circle';
      default: return 'bi bi-info-circle';
    }
  }
}