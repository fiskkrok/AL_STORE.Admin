// src/app/shared/formly/image-upload.type.ts
import { Component } from '@angular/core';
import { FieldType, FieldTypeConfig } from '@ngx-formly/core';

@Component({
  selector: 'formly-field-file',
  standalone: true,
  templateUrl: './image-upload.type.html',
  styleUrls: ['./image-upload.type.scss']
})
export class FormlyImageUploadTypeComponent extends FieldType<FieldTypeConfig> {
  previewUrls: string[] = [];
  // Add this getter
  get isFormControlValid(): boolean {
    return !!this.formControl && this.formControl.valid;
  }

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
    if (!this.formControl) {
      console.error('Form control is not initialized');
      return;
    }

    const imageFiles = files.filter(file =>
      file.type.startsWith('image/') &&
      file.size <= 5 * 1024 * 1024
    );

    if (this.previewUrls.length + imageFiles.length > 5) {
      this.formControl.setErrors({ maxFiles: true });
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