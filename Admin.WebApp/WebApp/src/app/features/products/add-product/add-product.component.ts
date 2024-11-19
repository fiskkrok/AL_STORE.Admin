import { Component, OnInit } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { FormlyFormOptions, FormlyFieldConfig, FormlyModule } from '@ngx-formly/core';
import { ProductService } from '../../../core/services/product.service';
import { Router } from '@angular/router';
import { getProductFormFields } from '../configs/product.formly.config';
import { CommonModule } from '@angular/common';
import { FormlyBootstrapModule } from '@ngx-formly/bootstrap';
import { Product } from '../../../shared/models/product.model';
import { FormlyImageUploadTypeComponent } from '../../../shared/formly/image-upload.type';
import { FieldType, FieldTypeConfig } from '@ngx-formly/core';
import { FileValueAccessor } from '../../../shared/formly/file-value-accessor';


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
    FormlyImageUploadTypeComponent
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

  constructor(
    private readonly productService: ProductService,
    private readonly router: Router
  ) { }

  ngOnInit(): void {
    // Get categories and initialize form fields
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
    if (this.form.valid) {
      const formValue = this.form.value as ProductFormModel;
      const images = formValue.images;

      // First upload images
      this.productService.uploadImages(images).subscribe({
        next: imageUrls => {
          // Create product with uploaded image URLs
          const product: Omit<Product, 'id'> = {
            name: formValue.basicInfo.name,
            description: formValue.basicInfo.description,
            price: formValue.pricing.price,
            currency: 'USD', // Assuming a default currency
            stock: formValue.pricing.stock,
            category: {
              id: '', // Assuming category ID will be set later
              name: formValue.basicInfo.category,
              description: ''
            },
            subCategory: null,
            images: imageUrls.map((url, index) => ({
              id: index.toString(),
              url,
              fileName: '',
              size: 0
            })),
            createdAt: new Date().toISOString(),
            createdBy: null,
            lastModifiedAt: null,
            lastModifiedBy: null
          };

          // Save product
          this.productService.addProduct(product).subscribe({
            next: () => {
              // Navigate to product list on success
              this.router.navigate(['/products/list']);
            },
            error: error => {
              console.error('Error creating product:', error);
              // TODO: Show error message to user
            }
          });
        },
        error: error => {
          console.error('Error uploading images:', error);
          // TODO: Show error message to user
        }
      });
    }
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