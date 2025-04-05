// src/app/features/categories/components/category-form-dialog/category-form-dialog.component.ts
import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Store } from '@ngrx/store';
import { Category } from 'src/app/shared/models/category.model';
import { selectAllCategories } from 'src/app/store/category/category.selectors';
import { FileUploadComponent } from 'src/app/shared/components/file-upload/file-upload.component';
interface DialogData {
  category?: Category;
  parentCategoryId?: string;
}

@Component({
  selector: 'app-category-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    FileUploadComponent
  ],
  template: `
    <div class="category-form-dialog">
      <!-- Dialog Header -->
      <div class="flex items-center justify-between px-6 py-4 border-b border-slate-200 dark:border-slate-700">
        <h2 class="text-xl font-medium text-slate-900 dark:text-white">{{ isEditing ? 'Edit' : 'Add' }} Category</h2>
        <button mat-icon-button (click)="onCancel()"
            class="text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-full transition-colors">
            <mat-icon>close</mat-icon>
        </button>
      </div>
      
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <!-- Dialog Content -->
        <div class="p-6">
          <div class="grid grid-cols-1 gap-4">
            <mat-form-field  class="w-full">
              <mat-label>Category Name</mat-label>
              <input matInput formControlName="name" placeholder="Enter category name">
              <mat-error class="text-xs text-red-500"  *ngIf="form.get('name')?.errors?.['required']">
                Name is required
              </mat-error>
              <mat-error class="text-xs text-red-500"  *ngIf="form.get('name')?.errors?.['maxlength']">
                Name cannot exceed 200 characters
              </mat-error>
            </mat-form-field>

            <mat-form-field  class="w-full">
              <mat-label>Description</mat-label>
              <textarea matInput formControlName="description" 
                        placeholder="Enter category description" rows="3">
              </textarea>
              <mat-error class="text-xs text-red-500"  *ngIf="form.get('description')?.errors?.['required']">
                Description is required
              </mat-error>
              <mat-error class="text-xs text-red-500"  *ngIf="form.get('description')?.errors?.['maxlength']">
                Description cannot exceed 2000 characters
              </mat-error>
            </mat-form-field>

            <mat-form-field  class="w-full">
              <mat-label>Parent Category</mat-label>
              <mat-select formControlName="parentCategoryId">
                <mat-option [value]="null">None</mat-option>
                @for (category of categories$ | async; track category.id) {
                  @if (!isEditing || category.id !== data.category?.id) {
                    <mat-option [value]="category.id">
                      {{ category.name }}
                    </mat-option>
                  }
                }
              </mat-select>
            </mat-form-field>

            <div class="mt-4">
              <h3 class="text-lg font-medium text-slate-800 dark:text-slate-200 mb-3">SEO Settings</h3>
              
              <mat-form-field  class="w-full">
                <mat-label>Meta Title</mat-label>
                <input matInput formControlName="metaTitle" 
                      placeholder="Enter meta title">
                <mat-hint class="text-xs text-orange-500">{{ form.get('metaTitle')?.value?.length || 0 }}/200</mat-hint>
                <mat-error class="text-xs text-red-500" *ngIf="form.get('metaTitle')?.errors?.['maxlength']">
                  Meta title cannot exceed 200 characters
                </mat-error>
              </mat-form-field>

              <mat-form-field  class="w-full">
                <mat-label>Meta Description</mat-label>
                <textarea matInput formControlName="metaDescription" 
                          placeholder="Enter meta description" rows="2">
                </textarea>
                <mat-hint class="text-xs text-orange-500">{{ form.get('metaDescription')?.value?.length || 0 }}/500</mat-hint>
                <mat-error class="text-xs text-red-500"  *ngIf="form.get('metaDescription')?.errors?.['maxlength']">
                  Meta description cannot exceed 500 characters
                </mat-error>
              </mat-form-field>
            </div>

            <app-file-upload
              [currentImage]="data.category?.imageUrl"
              (fileSelected)="onImageSelected($event)">
            </app-file-upload>
          </div>
        </div>

        <!-- Dialog Actions -->
        <div class="flex justify-end gap-3 px-6 py-4 border-t border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-800">
          <button mat-button type="button" (click)="onCancel()"
              class="border border-slate-300 dark:border-slate-600 px-4 py-1 rounded-md text-slate-700 dark:text-slate-200 hover:bg-slate-50 dark:hover:bg-slate-700">
              Cancel
          </button>
          <button mat-raised-button color="primary" 
                  type="submit"
                  [disabled]="form.invalid || form.pristine"
                  class="bg-primary-600 text-white px-4 py-1 rounded-md hover:bg-primary-700 transition-colors">
              {{ isEditing ? 'Update' : 'Create' }}
          </button>
        </div>
      </form>
    </div>
  `,
  styles: []
})
export class CategoryFormDialogComponent {
  form: FormGroup;
  categories$;
  selectedFile: File | null = null;

  get isEditing(): boolean {
    return !!this.data.category;
  }

  constructor(
    private readonly dialogRef: MatDialogRef<CategoryFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
    private readonly fb: FormBuilder,
    private readonly store: Store
  ) {
    this.categories$ = this.store.select(selectAllCategories);

    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.required, Validators.maxLength(2000)]],
      parentCategoryId: [data.parentCategoryId ?? null],
      metaTitle: ['', [Validators.maxLength(200)]],
      metaDescription: ['', [Validators.maxLength(500)]]
    });

    if (this.isEditing) {
      this.form.patchValue({
        name: data.category?.name,
        description: data.category?.description,
        parentCategoryId: data.category?.parentCategoryId,
        metaTitle: data.category?.metaTitle,
        metaDescription: data.category?.metaDescription
      });
    }
  }

  onImageSelected(file: File) {
    this.selectedFile = file;
  }

  onSubmit() {
    if (this.form.valid) {
      const formValue = this.form.value;

      const result = {
        ...formValue,
        file: this.selectedFile
      };

      this.dialogRef.close(result);
    }
  }

  onCancel() {
    this.dialogRef.close();
  }
}