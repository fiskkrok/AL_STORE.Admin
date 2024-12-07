import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { Product, ProductStatus, ProductVisibility } from 'src/app/shared/models/product.model';
import { ProductService, ProductUpdateCommand } from 'src/app/core/services/product.service';
import { AuthService } from 'src/app/core/services/auth.service';

@Component({
  selector: 'app-edit-product-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule
  ],
  template: `
    <div class="edit-product-dialog">
      <h2 mat-dialog-title>Edit Product</h2>
      
      <form [formGroup]="editProductForm" (ngSubmit)="onSubmit()">
        <mat-dialog-content>
          <div class="form-fields">
            <!-- Basic Info -->
            <mat-form-field appearance="outline">
              <mat-label>Product Name</mat-label>
              <input matInput formControlName="name" placeholder="Enter product name">
              <mat-error *ngIf="editProductForm.get('name')?.errors?.['required']">
                Name is required
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Description</mat-label>
              <textarea matInput formControlName="description" 
                        placeholder="Enter product description" rows="3">
              </textarea>
            </mat-form-field>

            <div class="form-row">
              <mat-form-field appearance="outline">
                <mat-label>Price</mat-label>
                <input matInput type="number" formControlName="price" placeholder="Enter price">
                <mat-error *ngIf="editProductForm.get('price')?.errors?.['required']">
                  Price is required
                </mat-error>
                <mat-error *ngIf="editProductForm.get('price')?.errors?.['min']">
                  Price must be positive
                </mat-error>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Stock</mat-label>
                <input matInput type="number" formControlName="stock" placeholder="Enter stock">
                <mat-error *ngIf="editProductForm.get('stock')?.errors?.['required']">
                  Stock is required
                </mat-error>
                <mat-error *ngIf="editProductForm.get('stock')?.errors?.['min']">
                  Stock must be positive
                </mat-error>
              </mat-form-field>
            </div>

            <mat-form-field appearance="outline">
              <mat-label>Category</mat-label>
              <mat-select formControlName="category">
                <mat-option [value]="data.category.id">
                  {{data.category.name}}
                </mat-option>
              </mat-select>
            </mat-form-field>

            <!-- Image Management -->
            <div class="images-section">
              <h3>Product Images</h3>
              
              <!-- Existing Images -->
              <div class="image-grid" *ngIf="data.images?.length">
                <div class="image-item" *ngFor="let image of data.images">
                  <img [src]="image.url" [alt]="data.name" class="product-image">
                  <div class="image-overlay">
                    <button type="button" 
                            mat-icon-button 
                            color="warn"
                            (click)="removeExistingImage(image.id)">
                      <mat-icon>delete</mat-icon>
                    </button>
                  </div>
                </div>
              </div>

              <!-- New Images Preview -->
              <div class="image-grid" *ngIf="newImagePreviews.length">
                <div class="image-item" *ngFor="let preview of newImagePreviews; let i = index">
                  <img [src]="preview" [alt]="'New image ' + (i + 1)" class="product-image">
                  <div class="image-overlay">
                    <button type="button" 
                            mat-icon-button 
                            color="warn"
                            (click)="removeNewImage(i)">
                      <mat-icon>delete</mat-icon>
                    </button>
                  </div>
                </div>
              </div>

              <!-- Upload Button -->
              <div class="upload-section">
                <input type="file" 
                       #fileInput 
                       accept="image/*" 
                       multiple 
                       (change)="onFileSelected($event)"
                       style="display: none">
                <button type="button" 
                        mat-stroked-button 
                        color="primary"
                        (click)="fileInput.click()">
                  <mat-icon>add_photo_alternate</mat-icon>
                  Add Images
                </button>
                <small class="hint">Max 5 images, each up to 5MB</small>
              </div>
            </div>
          </div>
        </mat-dialog-content>

        <mat-dialog-actions align="end">
          <button mat-button type="button" (click)="onCancel()">Cancel</button>
          <button mat-raised-button color="primary" 
                  type="submit" 
                  [disabled]="editProductForm.invalid || (!editProductForm.dirty && !hasImageChanges())">
            Save Changes
          </button>
        </mat-dialog-actions>
      </form>
    </div>
  `,
  styles: [`
    .edit-product-dialog {
      color: var(--text-primary);
    }

    .form-fields {
      display: flex;
      flex-direction: column;
      gap: 1rem;
      min-width: 400px;
      max-width: 600px;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
    }

    .images-section {
      background-color: var(--bg-secondary);
      border-radius: 8px;
      padding: 1rem;
      border: 1px solid var(--border);

      h3 {
        margin: 0 0 1rem 0;
        color: var(--text-primary);
      }
    }

    .image-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
      gap: 1rem;
      margin-bottom: 1rem;
    }

    .image-item {
      position: relative;
      aspect-ratio: 1;
      border-radius: 4px;
      overflow: hidden;
      background-color: var(--bg-primary);

      &:hover .image-overlay {
        opacity: 1;
      }
    }

    .product-image {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .image-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background-color: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      opacity: 0;
      transition: opacity 0.2s ease;
    }

    .upload-section {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.5rem;
      padding: 1rem;
      border: 2px dashed var(--border);
      border-radius: 4px;

      .hint {
        color: var(--text-secondary);
      }
    }

    .user-info {
      display: flex;
      align-items: center;
      cursor: pointer;
      color: var(--text-primary);

      i {
        margin-right: 0.5rem;
      }

      .bi-chevron-up,
      .bi-chevron-down {
        margin-left: auto;
      }
    }

    .user-menu {
      position: absolute;
      bottom: 100%;
      left: 0;
      background: var(--bg-secondary);
      border: 1px solid var(--border);
      border-radius: 4px 4px 0 0;
      overflow: hidden;

      a {
        display: block;
        padding: 0.5rem 1rem;
        color: var(--text-primary);
        text-decoration: none;

        &:hover {
          background: var(--bg-primary);
        }
      }
    }

    ::ng-deep {
      .mat-mdc-dialog-container {
        --mdc-dialog-container-color: var(--bg-secondary);
      }

      .mat-mdc-form-field {
        width: 100%;
      }

      .mat-mdc-text-field-wrapper {
        background-color: var(--bg-secondary);
      }

      .mat-mdc-form-field-label {
        color: var(--text-secondary);
      }

      .mat-mdc-input-element {
        color: var(--text-primary);
      }

      .mat-mdc-select-value {
        color: var(--text-primary);
      }

      .mat-mdc-select-arrow {
        color: var(--text-primary);
      }

      .mat-mdc-dialog-title {
        color: var(--text-primary);
      }

      .mat-mdc-dialog-content {
        color: var(--text-primary);
      }
    }

    @media (max-width: 600px) {
      .form-fields {
        min-width: unset;
      }

      .form-row {
        grid-template-columns: 1fr;
      }

      .image-grid {
        grid-template-columns: repeat(auto-fill, minmax(80px, 1fr));
      }
    }
  `]
})
export class EditProductDialogComponent {
  editProductForm: FormGroup;
  newImages: File[] = [];
  newImagePreviews: string[] = [];
  imagesToRemove: string[] = [];
  currentUser = { name: '' };
  isUserMenuOpen = false;

  constructor(
    public dialogRef: MatDialogRef<EditProductDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Product,
    private productService: ProductService,
    private authService: AuthService
  ) {
    this.editProductForm = new FormGroup({
      name: new FormControl(data.name, [Validators.required, Validators.minLength(3)]),
      description: new FormControl(data.description),
      price: new FormControl(data.price, [Validators.required, Validators.min(0)]),
      stock: new FormControl(data.stock, [Validators.required, Validators.min(0)]),
      category: new FormControl(data.category.id, [Validators.required])
    });
    this.currentUser.name = this.authService.getCurrentUserName();
  }

  hasImageChanges(): boolean {
    return this.newImages.length > 0 || this.imagesToRemove.length > 0;
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      const remainingSlots = 5 - (this.data.images.length - this.imagesToRemove.length) - this.newImages.length;
      const files = Array.from(input.files).slice(0, remainingSlots);

      files.forEach(file => {
        if (file.size <= 5 * 1024 * 1024) { // 5MB limit
          this.newImages.push(file);
          const reader = new FileReader();
          reader.onload = (e: ProgressEvent<FileReader>) => {
            if (e.target?.result) {
              this.newImagePreviews.push(e.target.result as string);
            }
          };
          reader.readAsDataURL(file);
        }
      });
    }
  }

  removeExistingImage(imageId: string) {
    this.imagesToRemove.push(imageId);
  }

  removeNewImage(index: number) {
    this.newImages.splice(index, 1);
    this.newImagePreviews.splice(index, 1);
  }

  toggleUserMenu() {
    this.isUserMenuOpen = !this.isUserMenuOpen;
  }

  logout() {
    this.authService.logout();
    this.dialogRef.close();
  }

  onSubmit() {
    if (this.editProductForm.valid && (this.editProductForm.dirty || this.hasImageChanges())) {
      const formValue = this.editProductForm.value;

      const updateCommand: ProductUpdateCommand = {
        id: this.data.id,
        name: formValue.name,
        description: formValue.description,
        price: {
          amount: formValue.price,
          currency: 'USD'  // Or get from configuration
        },
        categoryId: formValue.category,
        stock: formValue.stock,
        status: this.data.status || ProductStatus.Draft,
        visibility: this.data.visibility || ProductVisibility.Hidden,
        newImages: this.newImages,
        imageIdsToRemove: this.imagesToRemove,
        imageUpdates: this.data.images.map((img, index) => ({
          id: img.id,
          isPrimary: index === 0,
          sortOrder: index,
          alt: img.fileName
        }))
      };

      this.productService.updateProduct(updateCommand).subscribe({
        next: () => {
          this.dialogRef.close(true);
        },
        error: (error: Error) => {
          console.error('Error updating product:', error);
          this.dialogRef.close(false);
        }
      });
    }
  }

  onCancel() {
    this.dialogRef.close();
  }
}