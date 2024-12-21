// src/app/shared/components/file-upload/file-upload.component.ts
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'app-file-upload',
    standalone: true,
    imports: [CommonModule, MatButtonModule, MatIconModule],
    template: `
    <div class="file-upload">
      <div class="upload-container" 
           (dragover)="onDragOver($event)"
           (drop)="onDrop($event)"
           [class.has-file]="currentImage || previewUrl">
        
        @if (currentImage || previewUrl) {
          <div class="image-preview">
            <img [src]="previewUrl || currentImage" alt="Preview">
            <button mat-icon-button color="warn" 
                    class="remove-button"
                    (click)="removeImage()">
              <mat-icon>close</mat-icon>
            </button>
          </div>
        } @else {
          <div class="upload-prompt" (click)="fileInput.click()">
            <mat-icon>cloud_upload</mat-icon>
            <p>Drag and drop an image here or click to browse</p>
            <span class="hint">Maximum file size: 5MB</span>
          </div>
        }

        <input #fileInput
               type="file"
               (change)="onFileSelected($event)"
               accept="image/*"
               style="display: none">
      </div>

      @if (error) {
        <div class="error-message">
          {{ error }}
        </div>
      }
    </div>
  `,
    styles: [`
    .file-upload {
      width: 100%;
    }

    .upload-container {
      border: 2px dashed var(--border);
      border-radius: 8px;
      padding: 1rem;
      text-align: center;
      cursor: pointer;
      transition: all 0.3s ease;

      &:hover {
        border-color: var(--primary);
        background-color: var(--bg-hover);
      }

      &.has-file {
        border-style: solid;
      }
    }

    .upload-prompt {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.5rem;
      color: var(--text-secondary);

      mat-icon {
        font-size: 2rem;
        width: 2rem;
        height: 2rem;
      }

      p {
        margin: 0;
        color: var(--text-primary);
      }

      .hint {
        font-size: 0.875rem;
      }
    }

    .image-preview {
      position: relative;
      display: inline-block;

      img {
        max-width: 100%;
        max-height: 200px;
        border-radius: 4px;
      }

      .remove-button {
        position: absolute;
        top: -12px;
        right: -12px;
        background-color: var(--bg-secondary);
      }
    }

    .error-message {
      color: var(--error);
      font-size: 0.875rem;
      margin-top: 0.5rem;
    }
  `]
})
export class FileUploadComponent {
    @Input() currentImage: string | undefined;
    @Output() fileSelected = new EventEmitter<File>();

    previewUrl: string | null = null;
    error: string | null = null;

    onFileSelected(event: Event) {
        const input = event.target as HTMLInputElement;
        if (input.files?.length) {
            this.handleFile(input.files[0]);
        }
    }

    onDragOver(event: DragEvent) {
        event.preventDefault();
        event.stopPropagation();
    }

    onDrop(event: DragEvent) {
        event.preventDefault();
        event.stopPropagation();

        const files = event.dataTransfer?.files;
        if (files?.length) {
            this.handleFile(files[0]);
        }
    }

    removeImage() {
        this.previewUrl = null;
        this.fileSelected.emit();
    }

    private handleFile(file: File) {
        if (!file.type.startsWith('image/')) {
            this.error = 'Please upload an image file';
            return;
        }

        if (file.size > 5 * 1024 * 1024) {  // 5MB
            this.error = 'File size must not exceed 5MB';
            return;
        }

        this.error = null;
        this.createPreview(file);
        this.fileSelected.emit(file);
    }

    private createPreview(file: File) {
        const reader = new FileReader();
        reader.onload = () => {
            this.previewUrl = reader.result as string;
        };
        reader.readAsDataURL(file);
    }
}