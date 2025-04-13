// src/app/features/products/add-product/add-product.component.ts
import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

// Material Imports
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatStepperModule } from '@angular/material/stepper';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { Currency } from 'src/app/shared/models/currency.enum';
// Custom Components
import { BarcodeScannerComponent } from '../barcode-scanner';
import { ProductImageManagerComponent } from '../product-image-manager/product-image-manager.component';

// Services
import { CategoryService } from '../../../../core/services/category.service';
import { ProductService } from '../../../../core/services/product.service';
import { ErrorService } from '../../../../core/services/error.service';

@Component({
  selector: 'app-add-product',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatStepperModule,
    MatTooltipModule,
    MatCheckboxModule,
    BarcodeScannerComponent,
    ProductImageManagerComponent
  ],
  templateUrl: './add-product.component.html',
  styleUrls: ['./add-product.component.scss']
})
export class AddProductComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly categoryService = inject(CategoryService);
  private readonly productService = inject(ProductService);
  private readonly errorService = inject(ErrorService);
  basicInfoForm!: FormGroup;
  pricingForm!: FormGroup;
  detailsForm!: FormGroup;

  categories: any[] = [];
  isSubmitting = false;
  images: any[] = [];

  private destroy$ = new Subject<void>();
  Currency = Currency; // Enum for currency options
  currencies = Object.entries(Currency).map(([code, label]) => ({ code, label }));

  constructor(

  ) {
    this.createForms();
  }

  ngOnInit(): void {
    this.loadCategories();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createForms(): void {
    this.basicInfoForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      sku: ['', Validators.required],
      barcode: [''],
      categoryId: ['', Validators.required]
    });

    this.pricingForm = this.fb.group({
      price: [0, [Validators.required, Validators.min(0)]],
      compareAtPrice: [null],
      cost: [null],
      stock: [0, [Validators.required, Validators.min(0)]],
      lowStockThreshold: [5],
      currency: ['USD', Validators.required]
    });

    this.detailsForm = this.fb.group({
      description: ['', Validators.required],
      shortDescription: [''],
      isActive: [true],
      isFeatured: [false],
      tags: ['']
    });
  }

  private loadCategories(): void {
    this.categoryService.getCategories()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (categories) => {
          this.categories = categories;
        },
        error: () => {
          this.errorService.addError({
            code: 'CATEGORIES_LOAD_ERROR',
            message: 'Failed to load categories',
            severity: 'error'
          });
        }
      });
  }

  onBarcodeScanned(code: string): void {
    this.basicInfoForm.get('barcode')?.setValue(code);

    // Optionally: Look up product info by barcode from a product database API
    // this.productService.getProductByBarcode(code).subscribe(...)
  }

  onImagesChange(images: any[]): void {
    this.images = images;
  }

  submitProduct(): void {
    if (this.isSubmitting) return;

    if (!this.validateAllForms()) {
      this.errorService.addError({
        code: 'VALIDATION_ERROR',
        message: 'Please fill in all required fields',
        severity: 'warning'
      });
      return;
    }

    this.isSubmitting = true;

    const productData = {
      ...this.basicInfoForm.value,
      ...this.pricingForm.value,
      ...this.detailsForm.value,
      images: this.images,
    };

    this.productService.createProduct(productData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.router.navigate(['/products/list']);
        },
        error: (error) => {
          this.isSubmitting = false;
          this.errorService.addError({
            code: 'PRODUCT_CREATE_ERROR',
            message: 'Failed to create product',
            severity: 'error'
          });
        },
        complete: () => {
          this.isSubmitting = false;
        }
      });
  }
  getCategoryName(categoryId: string): string {
    const category = this.categories.find(cat => cat.id === categoryId);
    return category ? category.name : 'Not specified';
  }

  validateAllForms(): boolean {
    return (
      this.basicInfoForm.valid &&
      this.pricingForm.valid &&
      this.detailsForm.valid
    );
  }

  reset(): void {
    this.basicInfoForm.reset();
    this.pricingForm.reset();
    this.detailsForm.reset();
    this.images = [];

    // Reset with defaults
    this.pricingForm.patchValue({
      price: 0,
      stock: 0,
      lowStockThreshold: 5,
      currency: 'USD'
    });

    this.detailsForm.patchValue({
      isActive: true,
      isFeatured: false
    });
  }
}