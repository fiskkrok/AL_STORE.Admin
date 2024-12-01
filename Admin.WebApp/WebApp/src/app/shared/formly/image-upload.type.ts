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
  private files: File[] = [];  // Store actual File objects

  onFileInputClick(): void {
    const fileInput = document.createElement('input');
    fileInput.type = 'file';
    fileInput.multiple = true;
    fileInput.accept = 'image/*';
    fileInput.onchange = (e: Event) => {
      const input = e.target as HTMLInputElement;
      if (input.files) {
        this.handleFiles(Array.from(input.files));
      }
    };
    fileInput.click();
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

  handleFiles(newFiles: File[]): void {
    if (!this.formControl) {
      console.error('Form control is not initialized');
      return;
    }

    const imageFiles = newFiles.filter(file =>
      file.type.startsWith('image/') &&
      file.size <= 5 * 1024 * 1024
    );

    if (this.files.length + imageFiles.length > 5) {
      this.formControl.setErrors({ maxFiles: true });
      return;
    }

    imageFiles.forEach(file => {
      // Store the actual File object
      this.files.push(file);

      // Create preview URL
      const reader = new FileReader();
      reader.onload = (e: ProgressEvent<FileReader>) => {
        const result = e.target?.result as string;
        if (result) {
          this.previewUrls = [...this.previewUrls, result];
        }
      };
      reader.readAsDataURL(file);
    });

    // Set the actual File objects as the form value
    this.formControl.setValue(this.files);
    this.formControl.markAsTouched();
  }

  removeImage(previewUrl: string): void {
    const index = this.previewUrls.indexOf(previewUrl);
    if (index !== -1) {
      this.previewUrls.splice(index, 1);
      this.files.splice(index, 1);
      this.formControl?.setValue(this.files);
      this.formControl?.markAsTouched();
    }
  }
}