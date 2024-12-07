import { Component, OnInit } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { FormlyFormOptions, FormlyFieldConfig, FormlyModule } from '@ngx-formly/core';
import { Router } from '@angular/router';
import { getProductFormFields } from '../configs/product.formly.config';
import { CommonModule } from '@angular/common';
import { FormlyBootstrapModule } from '@ngx-formly/bootstrap';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ErrorService } from 'src/app/core/services/error.service';
import { ProductService, ProductCreateCommand } from '../../../core/services/product.service';
import { Product, ProductStatus, ProductVisibility, Money } from '../../../shared/models/product.model';

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
        const product: ProductCreateCommand = {
          name: formValue.basicInfo.name,
          description: formValue.basicInfo.description,
          price: {
            amount: formValue.pricing.price,
            currency: 'USD'  // Or get from configuration
          },
          categoryId: formValue.basicInfo.category,
          stock: formValue.pricing.stock,
          sku: this.generateSku(),
          status: ProductStatus.Draft,
          visibility: ProductVisibility.Hidden,
          images: imageResults.map(result => ({
            id: result.id,
            url: result.url,
            fileName: result.fileName,
            size: result.size,
            isPrimary: false,
            sortOrder: 0,
            alt: result.fileName
          })),
          tags: []
        };

        this.productService.createProduct(product).subscribe({
          next: () => {
            this.snackBar.open('Product created successfully', 'Close', {
              duration: 3000
            });
            this.router.navigate(['/products/list']);
          },
          error: (error: Error) => {
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
      error: (error: Error) => {
        console.error('Error uploading images:', error);
        this.errorService.addError({
          message: 'Failed to upload images: ' + error.message,
          type: 'error'
        });
        this.isSubmitting = false;
      }
    });
  }

  private generateSku(): string {
    return 'SKU-' + Math.random().toString(36).substr(2, 9).toUpperCase();
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