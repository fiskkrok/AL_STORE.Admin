import { Component, ViewChild, ElementRef } from '@angular/core';
import { FieldType, FieldTypeConfig, FormlyModule } from '@ngx-formly/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'formly-field-color-picker',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    FormlyModule
  ],
  template: `
    <mat-form-field class="w-full">
      <mat-label>{{ props.label }}</mat-label>
      <div class="flex items-center">
        <input
          matInput
          [formControl]="formControl"
          [formlyAttributes]="field"
          [placeholder]="props.placeholder || ''"
          class="flex-grow"
        />
      <input
      type="color"
      #colorPicker
      (input)="updateColor($event)"
      class="w-full h-10 border-none rounded-md cursor-pointer"
      />
      </div>
      <mat-hint *ngIf="props.description">{{ props.description }}</mat-hint>
      <mat-error *ngIf="formControl.invalid">{{ getErrorMessage() }}</mat-error>
    </mat-form-field>
  `
})
export class FormlyColorPickerTypeComponent extends FieldType<FieldTypeConfig> {
  @ViewChild('colorPicker') colorPickerRef!: ElementRef<HTMLInputElement>;



  updateColor(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input && input.value) {
      this.formControl.setValue(input.value);
    }
  }

  getErrorMessage(): string {
    const errors = this.formControl.errors;
    if (!errors) return '';

    if (errors['required']) {
      return 'This field is required';
    }

    if (errors['pattern']) {
      return 'Invalid color format. Use hex format (#RRGGBB)';
    }

    return Object.keys(errors).map(key => errors[key]).join(', ');
  }
}
