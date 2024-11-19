// src/app/shared/formly/image-upload.type.ts
import { Component } from '@angular/core';
import { FieldType } from '@ngx-formly/core';

@Component({
  selector: 'formly-field-file',
  standalone: true,
  template: `
    <div class="file-container">
      <div
        class="file-box bg-opacity-10 hover:bg-opacity-20 transition-all"
        (click)="onFileInputClick()"
        (dragover)="onDragOver($event)"
        (drop)="onDrop($event)"
      >
        <input
          #fileInput
          type="file"
          [multiple]="true"
          accept="image/*"
          (change)="onFileSelected($event)"
          class="hidden"
        />
        <div class="flex flex-col items-center justify-center p-6 border-2 border-dashed rounded-lg border-primary">
          <i class="bi bi-cloud-upload text-4xl mb-2 text-primary"></i>
          <p class="text-sm text-primary">
            Drag and drop images here or click to browse
          </p>
          <p class="text-xs text-primary-light mt-1">
            Max file size: 5MB | Supported formats: JPG, PNG
          </p>
        </div>
      </div>

      @if (previewUrls.length > 0) {
        <div class="image-preview-grid mt-4">
          @for (url of previewUrls; track url) {
            <div class="image-preview relative">
              <img [src]="url" alt="Preview" class="w-full h-full object-cover rounded" />
              <button
                type="button"
                (click)="removeImage(url)"
                class="remove-image"
              >
                <i class="bi bi-x"></i>
              </button>
            </div>
          }
        </div>
      }

      @if (formControl.errors) {
        <div class="text-red-500 text-sm mt-2">
          @if (formControl.errors['required']) {
            <div>At least one image is required</div>
          }
          @if (formControl.errors['maxSize']) {
            <div>One or more images exceed the maximum file size</div>
          }
          @if (formControl.errors['maxFiles']) {
            <div>Cannot upload more than 5 images</div>
          }
        </div>
      }
    </div>
  `,
  styleUrls: ['./image-upload.type.scss']
})
export class FormlyImageUploadTypeComponent extends FieldType {
  previewUrls: string[] = [];

  onFileInputClick(): void {
    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    fileInput?.click();
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    const files = event.dataTransfer?.files;
    if (files) {
      this.handleFiles(Array.from(files));
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.handleFiles(Array.from(input.files));
    }
  }

  handleFiles(files: File[]): void {
    const imageFiles = files.filter(file =>
      file.type.startsWith('image/') &&
      file.size <= 5 * 1024 * 1024
    );

    if (this.previewUrls.length + imageFiles.length > 5) {
      this.formControl.setErrors({ ...this.formControl.errors, maxFiles: true });
      return;
    }

    imageFiles.forEach(file => {
      const reader = new FileReader();
      reader.onload = (e: ProgressEvent<FileReader>) => {
        const result = e.target?.result as string;
        if (result) {
          this.previewUrls = [...this.previewUrls, result];
          this.formControl.setValue(this.previewUrls);
          this.formControl.markAsTouched();
        }
      };
      reader.readAsDataURL(file);
    });
  }

  removeImage(url: string): void {
    this.previewUrls = this.previewUrls.filter(u => u !== url);
    this.formControl.setValue(this.previewUrls);
    this.formControl.markAsTouched();
  }
}