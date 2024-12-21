// src/app/shared/components/loading-spinner/loading-spinner.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../../core/services/loading.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (loading$ | async) {
      <div class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center">
        <div class="animate-spin rounded-full h-32 w-32 border-t-2 border-b-2 border-primary"></div>
      </div>
    }
  `
})
export class LoadingSpinnerComponent {


  loading$: Observable<boolean>;

  constructor(private loadingService: LoadingService) {
    this.loading$ = this.loadingService.loading$;
  }
}