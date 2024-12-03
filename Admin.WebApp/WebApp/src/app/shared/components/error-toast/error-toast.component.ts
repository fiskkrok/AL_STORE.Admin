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
          <button mat-mini-fab style="max-width: 10px; max-height: 10px;"
            class="absolute top-0 right-0 px-4 py-3"
            (click)="removeError(error)"
          >
            <span class="sr-only ">Close</span>
          </button>
        </div>
      }
    </div>
  `
})
export class ErrorToastComponent implements OnInit {
  errors: ErrorState[] = [];

  constructor(private readonly errorService: ErrorService) { }

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