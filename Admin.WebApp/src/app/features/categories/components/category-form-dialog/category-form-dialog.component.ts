// src/app/features/categories/components/category-form-dialog/category-form-dialog.component.ts
import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { Store } from '@ngrx/store';
import { Category } from 'src/app/shared/models/Categories/category.model';
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
        FileUploadComponent
    ],
    template: `
    <div class="category-form-dialog">
      <h2 mat-dialog-title>{{ isEditing ? 'Edit' : 'Add' }} Category</h2>
      
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <mat-dialog-content>
          <div class="form-fields">
            <mat-form-field appearance="outline">
              <mat-label>Category Name</mat-label>
              <input matInput formControlName="name" placeholder="Enter category name">
              <mat-error *ngIf="form.get('name')?.errors?.['required']">
                Name is required
              </mat-error>
              <mat-error *ngIf="form.get('name')?.errors?.['maxlength']">
                Name cannot exceed 200 characters
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Description</mat-label>
              <textarea matInput formControlName="description" 
                        placeholder="Enter category description" rows="3">
              </textarea>
              <mat-error *ngIf="form.get('description')?.errors?.['required']">
                Description is required
              </mat-error>
              <mat-error *ngIf="form.get('description')?.errors?.['maxlength']">
                Description cannot exceed 2000 characters
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline">
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

            <div class="seo-section">
              <h3>SEO Settings</h3>
              
              <mat-form-field appearance="outline">
                <mat-label>Meta Title</mat-label>
                <input matInput formControlName="metaTitle" 
                       placeholder="Enter meta title">
                <mat-hint>{{ form.get('metaTitle')?.value?.length || 0 }}/200</mat-hint>
                <mat-error *ngIf="form.get('metaTitle')?.errors?.['maxlength']">
                  Meta title cannot exceed 200 characters
                </mat-error>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Meta Description</mat-label>
                <textarea matInput formControlName="metaDescription" 
                          placeholder="Enter meta description" rows="2">
                </textarea>
                <mat-hint>{{ form.get('metaDescription')?.value?.length || 0 }}/500</mat-hint>
                <mat-error *ngIf="form.get('metaDescription')?.errors?.['maxlength']">
                  Meta description cannot exceed 500 characters
                </mat-error>
              </mat-form-field>
            </div>

            <app-file-upload
              [currentImage]="data.category?.imageUrl"
              (fileSelected)="onImageSelected($event)">
            </app-file-upload>
          </div>
        </mat-dialog-content>

        <mat-dialog-actions align="end">
          <button mat-button type="button" (click)="onCancel()">Cancel</button>
          <button mat-raised-button color="primary" 
                  type="submit"
                  [disabled]="form.invalid || form.pristine">
            {{ isEditing ? 'Update' : 'Create' }}
          </button>
        </mat-dialog-actions>
      </form>
    </div>
  `,
    styles: [`
    .category-form-dialog {
      min-width: 500px;
    }

    .form-fields {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .seo-section {
      margin-top: 1rem;
      padding-top: 1rem;
      border-top: 1px solid var(--border);

      h3 {
        margin: 0 0 1rem;
        color: var(--text-primary);
        font-size: 1rem;
      }
    }

    mat-form-field {
      width: 100%;
    }

    ::ng-deep {
      .mat-mdc-form-field-subscript-wrapper {
        display: flex;
        justify-content: space-between;
      }
    }
  `]
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
            parentCategoryId: [data.parentCategoryId || null],
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