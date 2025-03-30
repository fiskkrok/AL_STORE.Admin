// src/app/features/products/product-list/image-preview-dialog.component.ts
import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-image-preview-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class="relative">
      <!-- Close button -->
      <button 
        mat-icon-button 
        class="absolute top-2 right-2 text-white bg-slate-800 bg-opacity-50 hover:bg-opacity-70 z-10 transition-colors"
        [mat-dialog-close]="true">
        <mat-icon>close</mat-icon>
      </button>
      
      <!-- Image preview -->
      <div class="image-preview-container">
        <img 
          [src]="data.url" 
          [alt]="data.alt || 'Image preview'" 
          class="max-w-full max-h-[80vh] object-contain rounded-md"
          loading="lazy">
      </div>
      
      <!-- Image metadata if available -->
      <div *ngIf="data.fileName || data.size" class="p-4 bg-slate-50 dark:bg-slate-800 border-t border-slate-200 dark:border-slate-700">
        <div *ngIf="data.fileName" class="text-sm text-slate-500 dark:text-slate-400">
          <span class="font-medium text-slate-700 dark:text-slate-300">Filename:</span> {{ data.fileName }}
        </div>
        <div *ngIf="data.size" class="text-sm text-slate-500 dark:text-slate-400">
          <span class="font-medium text-slate-700 dark:text-slate-300">Size:</span> {{ formatFileSize(data.size) }}
        </div>
      </div>
    </div>
  `,
  styles: [`
    .image-preview-container {
      display: flex;
      justify-content: center;
      align-items: center;
      padding: 1rem;
      background-color: rgba(0, 0, 0, 0.025);
      min-height: 300px;
    }
    
    :host ::ng-deep .mat-mdc-dialog-container {
      padding: 0 !important;
      overflow: hidden;
      border-radius: 0.5rem;
    }
  `]
})
export class ImagePreviewDialogComponent {
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: {
      url: string;
      alt?: string;
      fileName?: string;
      size?: number;
    },
    private dialogRef: MatDialogRef<ImagePreviewDialogComponent>
  ) { }

  // Helper method to format file size
  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }
}