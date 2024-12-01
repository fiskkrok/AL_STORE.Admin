import { Component, OnInit } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { FormlyFormOptions, FormlyFieldConfig, FormlyModule } from '@ngx-formly/core';
import { ProductService } from '../../../core/services/product.service';
import { Router } from '@angular/router';
import { getProductFormFields } from '../configs/product.formly.config';
import { CommonModule } from '@angular/common';
import { FormlyBootstrapModule } from '@ngx-formly/bootstrap';
import { Product } from '../../../shared/models/product.model';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ErrorService } from 'src/app/core/services/error.service';
import { finalize } from 'rxjs';

interface ProductFormModel {
  basicInfo: {
    name: string;
    category: string;
    description: string;
  };
  pricing: {
    price: number;
    stock: number;
  };
  images: File[];
}

@Component({
  selector: 'app-add-product',
  templateUrl: './add-product.component.html',
  styleUrls: ['./add-product.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormlyModule,
    FormlyBootstrapModule,
    ReactiveFormsModule,
    MatButtonModule
  ],
})
export class AddProductComponent implements OnInit {
  form = new FormGroup({});
  model: ProductFormModel = {
    basicInfo: {
      name: '',
      category: '',
      description: ''
    },
    pricing: {
      price: 0,
      stock: 0
    },
    images: []
  };
  options: FormlyFormOptions = {};
  fields: FormlyFieldConfig[] = [];
  isSubmitting = false;  // Add this flag

  constructor(
    private readonly productService: ProductService,
    private readonly router: Router,
    private readonly snackBar: MatSnackBar,
    private readonly errorService: ErrorService
  ) { }

  ngOnInit(): void {
    this.productService.getCategories().subscribe(
      categories => {
        this.fields = getProductFormFields(categories.map(category => ({
          id: category.id,
          name: category.name
        })));
      }
    );
  }

  submit(): void {
    if (this.isSubmitting || !this.form.valid) {
      return;
    }

    this.isSubmitting = true;
    const formValue = this.form.value as ProductFormModel;
    const images = formValue.images;

    this.productService.uploadImages(images).subscribe({
      next: (imageResults) => {
        const product: Omit<Product, 'id'> = {
          name: formValue.basicInfo.name,
          description: formValue.basicInfo.description,
          price: formValue.pricing.price,
          currency: 'USD',
          stock: formValue.pricing.stock,
          category: {
            id: formValue.basicInfo.category,
            name: '',
            description: ''
          },
          subCategory: null,
          images: imageResults.map(result => ({
            id: '',
            url: result.url,
            fileName: result.fileName,
            size: result.size
          })),
          createdAt: new Date().toISOString(),
          createdBy: null,
          lastModifiedAt: null,
          lastModifiedBy: null
        };

        this.productService.addProduct(product).subscribe({
          next: () => {
            this.snackBar.open('Product created successfully', 'Close', {
              duration: 3000
            });
            this.router.navigate(['/products/list']);
          },
          error: (error) => {
            console.error('Error creating product:', error);
            this.errorService.addError({
              message: 'Failed to create product: ' + error.message,
              type: 'error'
            });
          },
          complete: () => {
            this.isSubmitting = false;
          }
        });
      },
      error: (error) => {
        console.error('Error uploading images:', error);
        this.errorService.addError({
          message: 'Failed to upload images: ' + error.message,
          type: 'error'
        });
        this.isSubmitting = false;
      }
    });
  }

  reset(): void {
    this.form.reset();
    this.model = {
      basicInfo: {
        name: '',
        category: '',
        description: ''
      },
      pricing: {
        price: 0,
        stock: 0
      },
      images: []
    };
  }
}