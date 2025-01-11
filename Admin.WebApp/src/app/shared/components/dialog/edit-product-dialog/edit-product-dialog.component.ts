import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { Product, ProductImage } from 'src/app/shared/models/product.model';
import { ProductService } from 'src/app/core/services/product.service';
import { AuthService } from 'src/app/core/services/auth.service';
import { ErrorService } from 'src/app/core/services/error.service';
import { firstValueFrom } from 'rxjs';

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

  templateUrl: './edit-product-dialog.component.html',
  styleUrls: ['./edit-product-dialog.component.scss']
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
    private authService: AuthService,
    private errorService: ErrorService
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


  async onSubmit() {
    if (!this.editProductForm.valid) return;

    try {
      // Step 1: Handle image removals first
      if (this.imagesToRemove.length > 0) {
        await firstValueFrom(
          this.productService.deleteImages(this.imagesToRemove)
        );
      }

      // Step 2: Upload new images
      let newImages: ProductImage[] = [];
      if (this.newImages.length > 0) {
        newImages = await firstValueFrom(
          this.productService.uploadImages(this.newImages)
        );
      }

      // Step 3: Update product with remaining + new images
      const formValue = this.editProductForm.value;
      const remainingImages = this.data.images.filter(
        img => !this.imagesToRemove.includes(img.id)
      );

      const updateCommand = {
        id: this.data.id,
        name: formValue.name,
        description: formValue.description,
        price: formValue.price,
        currency: 'USD',
        categoryId: formValue.category,
        stock: formValue.stock,
        status: this.data.status,
        visibility: this.data.visibility,
        images: [...remainingImages, ...newImages]
      };

      await firstValueFrom(this.productService.updateProduct(updateCommand));
      this.dialogRef.close(true);
    } catch (error) {
      this.errorService.addError({
        code: '',
        message: 'Failed to update product: ' + (error as Error).message,
        severity: 'error'
      });
      this.dialogRef.close(false);
    }
  }
  onCancel() {
    this.dialogRef.close();
  }

  getErrorMessage(controlName: string): string {
    const control = this.editProductForm.get(controlName);
    if (control?.hasError('required')) {
      return 'You must enter a value';
    } else if (control?.hasError('minlength')) {
      return 'Value is too short';
    } else if (control?.hasError('min')) {
      return 'Value is too low';
    } else {
      return '';
    }
  }
}