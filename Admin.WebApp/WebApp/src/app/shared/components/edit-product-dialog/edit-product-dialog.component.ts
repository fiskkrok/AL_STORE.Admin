// filepath: /d:/AL_STORE/AL_STORE.Admin/src/Admin.WebApp/src/app/edit-product-dialog/edit-product-dialog.component.ts
import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Product } from '../../models/product.model';
import { ProductService } from '../../../core/services/product.service';

@Component({
  selector: 'app-edit-product-dialog',
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
      // ...additional form controls...
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