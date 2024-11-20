import { CommonModule } from "@angular/common";
import { Component } from "@angular/core";
import { DialogConfig, DialogService } from "../../../core/services/dialog.service";
import { Observable } from "rxjs";

@Component({
  selector: 'app-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (dialog$ | async; as dialog) {
      <div class="dialog-overlay" (click)="onOverlayClick($event)">
        <div class="dialog-container" [ngClass]="dialog.type">
          @if (dialog.type === 'preview') {
            <img [src]="dialog.message" alt="Preview" />
          }
          @else{
          <h2>{{ dialog.title }}</h2>
          <p>{{ dialog.message }}</p>
          <div class="dialog-actions">
            @if (dialog.type === 'confirm') {
              <button class="btn btn-outline-secondary" (click)="onCancel()">
                {{ dialog.cancelText || 'Cancel' }}
              </button>
            }
            <button 
              class="btn" 
              [class.btn-primary]="dialog.type !== 'error'"
              [class.btn-danger]="dialog.type === 'error'"
              (click)="onConfirm()"
            >
              {{ dialog.confirmText || 'OK' }}
            </button>
          </div>
          }
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

    .dialog-container {
      background-color: var(--bg-secondary);
      border-radius: 8px;
      padding: 1.5rem;
      min-width: 320px;
      max-width: 90%;
      margin: 1rem;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);

      h2 {
        margin: 0 0 1rem;
        color: var(--text-primary);
      }

      p {
        margin: 0 0 1.5rem;
        color: var(--text-secondary);
      }
    }

    .dialog-actions {
      display: flex;
      justify-content: flex-end;
      gap: 1rem;
    }

    @media (max-width: 768px) {
      .dialog-container {
        width: 100%;
        margin: 1rem;
      }

      .dialog-actions {
        flex-direction: column-reverse;
        gap: 0.5rem;

        .btn {
          width: 100%;
        }
      }
    }
  `]
})
export class DialogComponent {
  dialog$ = new Observable<DialogConfig | null>();

  constructor(private readonly dialogService: DialogService) {
    this.dialog$ = this.dialogService.dialog$;
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