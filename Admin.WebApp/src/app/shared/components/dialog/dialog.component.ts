import { CommonModule } from "@angular/common";
import { Component } from "@angular/core";
import { DialogConfig, DialogService } from "../../../core/services/dialog.service";
import { Observable } from "rxjs";
import { MatIconModule } from "@angular/material/icon";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatButtonModule } from "@angular/material/button";
import { MatInputModule } from "@angular/material/input";

@Component({
  selector: 'app-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatFormFieldModule,
    MatButtonModule,
    MatInputModule
  ],
  template: `
    @if (dialog$ | async; as dialog) {
      <div class="dialog-overlay" (click)="onOverlayClick($event)">
        <div class="dialog-container bg-white dark:bg-slate-800 rounded-lg shadow-lg overflow-hidden">
          <!-- Dialog Header -->
          <div class="flex items-center justify-between px-6 py-4 border-b border-slate-200 dark:border-slate-700">
            <h2 class="text-xl font-medium text-slate-900 dark:text-white">{{ dialog.title }}</h2>
            <button
              mat-icon-button
              (click)="onCancel()"
              class="text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-full transition-colors">
              <mat-icon>close</mat-icon>
            </button>
          </div>

          <!-- Dialog Content -->
          <div class="p-6">
            <p class="text-slate-700 dark:text-slate-300">{{ dialog.message }}</p>
            
            @if (dialog.type === 'preview') {
              <div class="mt-4 border border-slate-200 dark:border-slate-700 rounded-lg p-4 bg-slate-50 dark:bg-slate-700 overflow-auto max-h-96">
                <pre class="text-sm text-slate-800 dark:text-slate-200 whitespace-pre-wrap">{{ dialog.data }}</pre>
              </div>
            }
          </div>

          <!-- Dialog Actions -->
          <div class="flex justify-end gap-3 px-6 py-4 border-t border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-700">
            @if (dialog.type !== 'info') {
              <button
                mat-button
                type="button"
                (click)="onCancel()"
                class="border border-slate-300 dark:border-slate-600 px-4 py-1 rounded-md text-slate-700 dark:text-slate-200 hover:bg-slate-50 dark:hover:bg-slate-700">
                {{ dialog.cancelText || 'Cancel' }}
              </button>
            }
            <button
              mat-raised-button
              [color]="getButtonColor(dialog.type)"
              type="button"
              (click)="onConfirm()"
              class="px-4 py-1 rounded-md transition-colors"
              [ngClass]="{
                'bg-primary-600 hover:bg-primary-700 text-white': dialog.type === 'confirm' || dialog.type === 'info' || !dialog.type,
                'bg-rose-600 hover:bg-rose-700 text-white': dialog.type === 'error',
                'bg-amber-600 hover:bg-amber-700 text-white': dialog.type === 'warning'
              }">
              {{ dialog.confirmText || 'OK' }}
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .dialog-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background-color: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }

    @media (max-width: 640px) {
      .dialog-container {
        width: calc(100% - 2rem);
        max-height: calc(100% - 2rem);
        margin: 1rem;
        display: flex;
        flex-direction: column;
      }
    }

    @media (min-width: 641px) {
      .dialog-container {
        min-width: 320px;
        max-width: 560px;
      }
    }
  `]
})
export class DialogComponent {
  dialog$ = new Observable<DialogConfig | null>();

  constructor(private readonly dialogService: DialogService) {
    this.dialog$ = this.dialogService.dialog$;
  }

  getButtonColor(type: string | undefined): string {
    switch (type) {
      case 'error': return 'warn';
      case 'warning': return 'accent';
      default: return 'primary';
    }
  }

  onConfirm() {
    this.dialogService.handleAction(true);
  }

  onCancel() {
    this.dialogService.handleAction(false);
  }

  onOverlayClick(event: MouseEvent) {
    if ((event.target as HTMLElement).classList.contains('dialog-overlay')) {
      this.onCancel();
    }
  }
}