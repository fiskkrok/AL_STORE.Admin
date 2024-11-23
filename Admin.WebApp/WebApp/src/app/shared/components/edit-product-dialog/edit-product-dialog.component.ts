
import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { Product } from '../../models/product.model';

import { ProductService } from '../../../core/services/product.service';
import { MatFormField, MatFormFieldModule } from '@angular/material/form-field';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatOptionModule } from '@angular/material/core';

@Component({
  selector: 'app-edit-product-dialog',
  imports: [MatFormFieldModule, CommonModule, ReactiveFormsModule, MatButtonModule,
    MatInputModule, MatOptionModule,
    MatCardModule],
  standalone: true,
  templateUrl: './edit-product-dialog.component.html',
  styleUrls: ['./edit-product-dialog.component.scss']
})
export class EditProductDialogComponent {
  editProductForm: FormGroup;

  constructor(
    public dialogRef: MatDialogRef<EditProductDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Product,
    private productService: ProductService
  ) {
    this.editProductForm = new FormGroup({
      name: new FormControl(data.name, Validators.required),
      price: new FormControl(data.price, [Validators.required, Validators.min(0)]),
      description: new FormControl(data.description),
      category: new FormControl(data.category, Validators.required),
      imageUrl: new FormControl(data.images, Validators.required),
    });
  }

  onSubmit() {
    if (this.editProductForm.valid) {
      const updatedProduct: Product = {
        ...this.data,
        ...this.editProductForm.value
      };
      this.productService.updateProduct(updatedProduct).subscribe(
        () => this.dialogRef.close(true),
        // _error => {

        // }
      );
    }
  }

  onCancel() {
    this.dialogRef.close(false);
  }
}