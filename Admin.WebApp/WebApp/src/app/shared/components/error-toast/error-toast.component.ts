import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ErrorService, ErrorState } from '../../../core/services/error.service';

@Component({
  selector: 'app-error-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed bottom-4 right-4 z-50">
      @for (error of errors; track error.timestamp) {
        <div 
          class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative mb-2"
          role="alert"
        >
          <span class="block sm:inline">{{ error.message }}</span>
          <button
            class="absolute top-0 right-0 px-4 py-3"
            (click)="removeError(error)"
          >
            <span class="sr-only">Close</span>
            <svg class="h-4 w-4 fill-current" role="button" viewBox="0 0 20 20">
              <path
                d="M14.348 14.849a1.2 1.2 0 0 1-1.697 0L10 11.819l-2.651 3.029a1.2 1.2 0 1 1-1.697-1.697l2.758-3.15-2.759-3.152a1.2 1.2 0 1 1 1.697-1.697L10 8.183l2.651-3.031a1.2 1.2 0 1 1 1.697 1.697l-2.758 3.152 2.758 3.15a1.2 1.2 0 0 1 0 1.698z"
              />
            </svg>
          </button>
        </div>
      }
    </div>
  `
})
export class ErrorToastComponent implements OnInit {
  errors: ErrorState[] = [];

  constructor(private errorService: ErrorService) { }

  ngOnInit() {
    this.errorService.errors$.subscribe(errors => {
      this.errors = errors;
    });
  }

  removeError(error: ErrorState) {
    const index = this.errors.findIndex(e => e.timestamp === error.timestamp);
    if (index !== -1) {
      this.errorService.removeError(index);
    }
  }
}