// src/app/features/products/components/dynamic-product-form/dynamic-product-form.component.ts
import { Component, OnInit, OnDestroy, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatStepperModule } from '@angular/material/stepper';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { FormlyModule, FormlyFieldConfig } from '@ngx-formly/core';
import { Subject, combineLatest, of } from 'rxjs';
import { finalize, takeUntil, startWith } from 'rxjs/operators';

import { ProductTypeService } from '../../../../core/services/product-type.service';
import { ProductService } from '../../../../core/services/product.service';
import { ErrorService } from '../../../../core/services/error.service';
import { ProductCreateCommand, ProductImage, ProductStatus, ProductVisibility } from '../../../../shared/models/product.model';
import { ProductType } from '../../../../shared/models/product-type.model';
import { CategoryService } from '../../../../core/services/category.service';
import { ProductImageManagerComponent } from '../../product-image-manager/product-image-manager.component';
import { Currency } from '../../../../shared/models/currency.enum';
import { environment } from 'src/environments/environment';

interface ProductFormModel {
  basicInfo: {
    name: string;
    description: string;
    shortDescription?: string;
    sku: string;
    barcode?: string;
    categoryId: string;
    subCategoryId?: string;
    productTypeId: string;
  };
  pricing: {
    price: number;
    currency: string;
    compareAtPrice?: number;
    stock: number;
    lowStockThreshold?: number;
  };
  attributes: Record<string, any>;
  status: ProductStatus;
  visibility: ProductVisibility;
  images: ProductImage[];
  seo?: {
    title?: string;
    description?: string;
    keywords?: string[];
  };
}

@Component({
  selector: 'app-dynamic-product-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatStepperModule,
    MatIconModule,
    MatCardModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
    FormlyModule,
    ProductImageManagerComponent
  ],
  template: `
    <div class="product-form-container">
      <mat-card>
        <mat-card-header>
          <mat-card-title>{{ isEditMode ? 'Edit Product' : 'Add New Product' }}</mat-card-title>
          <mat-card-subtitle>
            Fill in the details to {{ isEditMode ? 'update' : 'create' }} your product
          </mat-card-subtitle>
        </mat-card-header>
        
        <mat-card-content>
          <mat-stepper linear #stepper>
            <!-- Step 1: Basic Product Information -->
            <mat-step [stepControl]="basicInfoForm">
              <ng-template matStepLabel>Basic Information</ng-template>
              
              <div class="step-content">
                <h3>Basic Product Details</h3>
                
                <!-- Product Type Selection -->
                <mat-form-field  appearance="fill" class="full-width">
                  <mat-label>Product Type</mat-label>
                  <mat-select 
                    [value]="selectedProductType?.id" 
                    (selectionChange)="onProductTypeChange($event.value)">
                    <mat-option *ngFor="let type of productTypes" [value]="type.id">
                      {{ type.name }}
                    </mat-option>
                  </mat-select>
                  <mat-hint class="text-xs text-orange-500">Select a product type to load relevant fields</mat-hint>
                </mat-form-field>
                
                <!-- Basic Info Form -->
                <form [formGroup]="basicInfoForm">
                  <formly-form
                    [form]="basicInfoForm"
                    [fields]="basicInfoFields"
                    [model]="model.basicInfo">
                  </formly-form>
                </form>
                
                <div class="step-actions">
                  <button 
                    mat-button 
                    matStepperNext
                    color="primary"
                    [disabled]="!basicInfoForm.valid">
                    Next
                  </button>
                </div>
              </div>
            </mat-step>
            
            <!-- Step 2: Pricing & Inventory -->
            <mat-step [stepControl]="pricingForm">
              <ng-template matStepLabel>Pricing & Inventory</ng-template>
              
              <div class="step-content">
                <h3>Pricing & Stock Information</h3>
                
                <form [formGroup]="pricingForm">
                  <formly-form
                    [form]="pricingForm"
                    [fields]="pricingFields"
                    [model]="model.pricing">
                  </formly-form>
                </form>
                
                <div class="step-actions">
                  <button mat-button matStepperPrevious>Back</button>
                  <button 
                    mat-button 
                    matStepperNext
                    color="primary"
                    [disabled]="!pricingForm.valid">
                    Next
                  </button>
                </div>
              </div>
            </mat-step>
            
            <!-- Step 3: Product Attributes (Dynamic based on product type) -->
            <mat-step [stepControl]="attributesForm" *ngIf="attributesFields.length > 0">
              <ng-template matStepLabel>Attributes</ng-template>
              
              <div class="step-content">
                <h3>Product Attributes</h3>
                <p class="step-description">
                  Configure specific attributes for {{ selectedProductType?.name }}
                </p>
                
                <form [formGroup]="attributesForm">
                  <formly-form
                    [form]="attributesForm"
                    [fields]="attributesFields"
                    [model]="model.attributes">
                  </formly-form>
                </form>
                
                <div class="step-actions">
                  <button mat-button matStepperPrevious>Back</button>
                  <button 
                    mat-button 
                    matStepperNext
                    color="primary"
                    [disabled]="!attributesForm.valid">
                    Next
                  </button>
                </div>
              </div>
            </mat-step>
            
            <!-- Step 4: Images -->
            <mat-step>
              <ng-template matStepLabel>Images</ng-template>
              
              <div class="step-content">
                <h3>Product Images</h3>
                <p class="step-description">
                  Upload and manage product images. The first image will be used as the primary display image.
                </p>
                
                <app-product-image-manager
                  [images]="model.images"
                  (imagesChange)="onImagesChange($event)">
                </app-product-image-manager>
                
                <div class="step-actions">
                  <button mat-button matStepperPrevious>Back</button>
                  <button 
                    mat-button 
                    matStepperNext
                    color="primary"
                    [disabled]="model.images.length === 0">
                    Next
                  </button>
                </div>
              </div>
            </mat-step>
            
            <!-- Step 5: SEO & Visibility -->
            <mat-step [stepControl]="seoForm">
              <ng-template matStepLabel>SEO & Visibility</ng-template>
              
              <div class="step-content">
                <h3>SEO Settings & Visibility</h3>
                
                <form [formGroup]="seoForm">
                  <formly-form
                    [form]="seoForm"
                    [fields]="seoFields"
                    [model]="model">
                  </formly-form>
                </form>
                
                <div class="step-actions">
                  <button mat-button matStepperPrevious>Back</button>
                  <button 
                    mat-button 
                    matStepperNext
                    color="primary"
                    [disabled]="!seoForm.valid">
                    Review
                  </button>
                </div>
              </div>
            </mat-step>
            
            <!-- Step 6: Review & Submit -->
            <mat-step>
              <ng-template matStepLabel>Review & Submit</ng-template>
              
              <div class="step-content">
                <h3>Review Product Information</h3>
                <p class="step-description">
                  Please review all information before submitting.
                </p>
                
                <div class="product-summary">
                  <div class="summary-section bg-slate-100 dark:bg-slate-800">
                    <h4>Basic Information</h4>
                    <p><strong>Name:</strong> {{ model.basicInfo.name }}</p>
                    <p><strong>Type:</strong> {{ selectedProductType?.name }}</p>
                    <p><strong>SKU:</strong> {{ model.basicInfo.sku }}</p>
                    <p *ngIf="model.basicInfo.barcode"><strong>Barcode:</strong> {{ model.basicInfo.barcode }}</p>
                  </div>
                  
                  <div class="summary-section bg-slate-100 dark:bg-slate-800">
                    <h4>Pricing & Inventory</h4>
                    <p><strong>Price:</strong> {{ model.pricing.price | currency:model.pricing.currency }}</p>
                    <p *ngIf="model.pricing.compareAtPrice">
                      <strong>Compare At:</strong> {{ model.pricing.compareAtPrice | currency:model.pricing.currency }}
                    </p>
                    <p><strong>Stock:</strong> {{ model.pricing.stock }}</p>
                  </div>
                  
                  <div class="summary-section bg-slate-100 dark:bg-slate-800" *ngIf="attributesFields.length > 0">
                    <h4>Attributes</h4>
                    <p *ngFor="let field of attributesFields">
                      <strong>{{ field.props?.label }}:</strong>
                      {{ formatAttributeValue(field.key ?? "", model.attributes[getKeyAsString(field.key ?? "")]) }}
                    </p>
                  </div>
                  
                  <div class="summary-section bg-slate-100 dark:bg-slate-800">
                    <h4>Images</h4>
                    <div class="summary-images">
                      <div *ngFor="let image of model.images.slice(0, 3)" class="summary-image">
                        <img [src]="image.url" [alt]="image.fileName">
                      </div>
                      <div *ngIf="model.images.length > 3" class="more-images">
                        +{{ model.images.length - 3 }} more
                      </div>
                    </div>
                  </div>
                  
                  <div class="summary-section bg-slate-100 dark:bg-slate-800">
                    <h4>Visibility</h4>
                    <p><strong>Status:</strong> {{ formatEnumValue(model.status) }}</p>
                    <p><strong>Visibility:</strong> {{ formatEnumValue(model.visibility) }}</p>
                  </div>
                </div>
                
                <div class="step-actions final-actions">
                  <button mat-button matStepperPrevious>Back</button>
                  <button 
                    mat-button 
                    (click)="stepper.reset()">
                    Reset
                  </button>
                  <button 
                    mat-raised-button 
                    color="primary"
                    [disabled]="isSubmitting"
                    (click)="submitProduct()">
                    <mat-icon *ngIf="isSubmitting" class="spin">sync</mat-icon>
                    {{ isEditMode ? 'Update' : 'Create' }} Product
                  </button>
                </div>
              </div>
            </mat-step>
          </mat-stepper>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .product-form-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 1rem;
    }
    
    .step-content {
      margin: 1.5rem 0;
      max-width: 800px;
    }
    
    .step-description {
      color: #666;
      margin-bottom: 1.5rem;
    }
    
    .full-width {
      width: 100%;
      margin-bottom: 1.5rem;
    }
    
    .step-actions {
      display: flex;
      justify-content: flex-end;
      gap: 1rem;
      margin-top: 2rem;
    }
    
    .product-summary {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 1.5rem;
      margin: 2rem 0;
    }
    
    .summary-section {
      border-radius: 8px;
      padding: 1.5rem;
    }
    
    .summary-section h4 {
      margin-top: 0;
      margin-bottom: 1rem;
      padding-bottom: 0.5rem;
      border-bottom: 1px solid #eee;
    }
    
    .summary-images {
      display: flex;
      gap: 0.5rem;
    }
    
    .summary-image {
      width: 60px;
      height: 60px;
      border-radius: 4px;
      overflow: hidden;
    }
    
    .summary-image img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
    
    .more-images {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 60px;
      height: 60px;
      background-color: #eee;
      border-radius: 4px;
      font-size: 0.75rem;
    }
    
    .final-actions {
      justify-content: space-between;
    }
    
    .spin {
      animation: spin 1.5s linear infinite;
    }
    
    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
  `]
})
export class DynamicProductFormComponent implements OnInit, OnDestroy {
  // Forms for each step
  basicInfoForm = new FormGroup({});
  pricingForm = new FormGroup({});
  attributesForm = new FormGroup({});
  seoForm = new FormGroup({});

  // Formly fields configuration
  basicInfoFields: FormlyFieldConfig[] = [];
  pricingFields: FormlyFieldConfig[] = [];
  attributesFields: FormlyFieldConfig[] = [];
  seoFields: FormlyFieldConfig[] = [];

  // Product model
  model: ProductFormModel = {
    basicInfo: {
      name: '',
      description: '',
      sku: '',
      categoryId: '',
      productTypeId: ''
    },
    pricing: {
      price: 0,
      currency: 'USD',
      stock: 0
    },
    attributes: {},
    status: ProductStatus.Draft,
    visibility: ProductVisibility.Hidden,
    images: []
  };

  // Component state
  isEditMode = false;
  isSubmitting = false;
  productId? = input<string>('productId');
  productTypes: ProductType[] = [];
  selectedProductType?: ProductType;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly router: Router,
    private readonly productService: ProductService,
    private readonly productTypeService: ProductTypeService,
    private readonly categoryService: CategoryService,
    private readonly errorService: ErrorService,
    private readonly snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.loadProductTypes();
    this.initBasicInfoFields();
    this.initPricingFields();
    this.initSeoFields();

    // Try to load saved form data if it exists
    if (!environment.production) {
      this.loadSavedFormData();
    }

    // If we're in edit mode (check for route param)
    // this.route.paramMap.pipe(
    //   takeUntil(this.destroy$),
    //   switchMap(params => {
    //     const id = params.get('id');
    //     if (id) {
    //       this.isEditMode = true;
    //       this.productId = id;
    //       return this.productService.getProduct(id);
    //     }
    //     return of(null);
    //   })
    // ).subscribe(product => {
    //   if (product) {
    //     this.loadExistingProduct(product);
    //   }
    // });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadProductTypes(): void {
    this.productTypeService.getProductTypes()
      .pipe(takeUntil(this.destroy$))
      .subscribe(types => {
        this.productTypes = types;
        if (types.length > 0 && !this.selectedProductType) {
          this.onProductTypeChange(types[0].id);
        }
      });
  }

  onProductTypeChange(typeId: string): void {
    this.selectedProductType = this.productTypes.find(type => type.id === typeId);
    this.model.basicInfo.productTypeId = typeId;

    // Reset attributes when product type changes
    this.model.attributes = {};
    this.attributesForm = new FormGroup({});

    // Load attribute fields for the selected product type
    if (this.selectedProductType) {
      this.productTypeService.getAttributeFieldsByType(typeId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(fields => {
          this.attributesFields = fields;
        });
    }
  }

  private initBasicInfoFields(): void {
    combineLatest([
      this.categoryService.getCategories().pipe(startWith([])),
      of(['USD', 'EUR', 'GBP', 'CAD', 'AUD']), // Currencies
    ]).pipe(
      takeUntil(this.destroy$)
    ).subscribe(([categories, currencies]) => {
      this.basicInfoFields = [
        {
          key: 'name',
          type: 'input',
          props: {
            label: 'Product Name',
            placeholder: 'Enter product name',
            required: true,
            minLength: 3,
            maxLength: 100
          },
          validation: {
            messages: {
              required: 'Product name is required',
              minlength: 'Name must be at least 3 characters',
              maxlength: 'Name cannot be more than 100 characters'
            }
          }
        },
        {
          key: 'description',
          type: 'textarea',
          props: {
            label: 'Description',
            placeholder: 'Enter product description',
            required: true,
            rows: 5,
            maxLength: 2000
          },
          validation: {
            messages: {
              required: 'Description is required',
              maxlength: 'Description cannot be more than 2000 characters'
            }
          }
        },
        {
          key: 'shortDescription',
          type: 'textarea',
          props: {
            label: 'Short Description',
            placeholder: 'Enter a brief summary (optional)',
            rows: 2,
            maxLength: 500
          }
        },
        {
          key: 'sku',
          type: 'input',
          props: {
            label: 'SKU (Stock Keeping Unit)',
            placeholder: 'Enter product SKU',
            required: true,
            maxLength: 50
          },
          validation: {
            messages: {
              required: 'SKU is required',
              maxlength: 'SKU cannot be more than 50 characters'
            }
          }
        },
        {
          key: 'barcode',
          type: 'input',
          props: {
            label: 'Barcode (UPC, EAN, etc.)',
            placeholder: 'Enter product barcode (optional)',
            maxLength: 50
          }
        },
        {
          key: 'categoryId',
          type: 'select',
          props: {
            label: 'Category',
            placeholder: 'Select a category',
            required: true,
            options: categories.map(cat => ({
              label: cat.name,
              value: cat.id
            }))
          },
          validation: {
            messages: {
              required: 'Category is required'
            }
          }
        },
        {
          key: 'subCategoryId',
          type: 'select',
          props: {
            label: 'Subcategory',
            placeholder: 'Select a subcategory (optional)',
            options: []
          },
          expressionProperties: {
            'props.options': (model, formState) => {
              const categoryId = model.categoryId;
              if (!categoryId) return [];

              const category = categories.find(c => c.id === categoryId);
              if (!category?.subCategories) return [];

              return category.subCategories.map(sub => ({
                label: sub.name,
                value: sub.id
              }));
            }
          }
        }
      ];
    });
  }

  private initPricingFields(): void {
    // First import the Currency enum

    this.pricingFields = [
      {
        fieldGroupClassName: 'row',
        fieldGroup: [
          {
            className: 'col-sm-6',
            key: 'price',
            type: 'input',
            props: {
              type: 'number',
              label: 'Price',
              placeholder: 'Enter price',
              required: true,
              min: 0,
              step: 0.01,
              addonLeft: {
                text: '$'
              }
            },
            validation: {
              messages: {
                required: 'Price is required',
                min: 'Price must be greater than or equal to 0'
              }
            }
          },
          {
            className: 'col-sm-6',
            key: 'currency',
            type: 'select',
            props: {
              label: 'Currency',
              placeholder: 'Select currency',
              required: true,
              options: Object.entries(Currency).map(([code, name]) => ({
                label: `${code} - ${name}`,
                value: code
              }))
            }
          }
        ]
      },
      {
        key: 'compareAtPrice',
        type: 'input',
        props: {
          type: 'number',
          label: 'Compare-at Price',
          placeholder: 'Enter original price (optional)',
          min: 0,
          step: 0.01,
          description: 'Used to show a markdown or sale price'
        }
      },
      {
        fieldGroupClassName: 'row',
        fieldGroup: [
          {
            className: 'col-sm-6',
            key: 'stock',
            type: 'input',
            props: {
              type: 'number',
              label: 'Initial Stock',
              placeholder: 'Enter initial stock',
              required: true,
              min: 0
            },
            validation: {
              messages: {
                required: 'Initial stock is required',
                min: 'Stock must be greater than or equal to 0'
              }
            }
          },
          {
            className: 'col-sm-6',
            key: 'lowStockThreshold',
            type: 'input',
            props: {
              type: 'number',
              label: 'Low Stock Threshold',
              placeholder: 'Enter low stock threshold (optional)',
              min: 0,
              description: 'Alerts when stock falls below this number'
            }
          }
        ]
      }
    ];
  }

  private initSeoFields(): void {
    this.seoFields = [
      {
        key: 'seo',
        fieldGroup: [
          {
            key: 'title',
            type: 'input',
            props: {
              label: 'SEO Title',
              placeholder: 'Enter SEO title (optional)',
              maxLength: 70,
              description: 'Override page title for search engines'
            }
          },
          {
            key: 'description',
            type: 'textarea',
            props: {
              label: 'SEO Description',
              placeholder: 'Enter SEO description (optional)',
              maxLength: 160,
              rows: 3,
              description: 'Meta description for search engines'
            }
          },
          {
            key: 'keywords',
            type: 'input',
            props: {
              label: 'SEO Keywords',
              placeholder: 'Enter keywords separated by commas (optional)',
              description: 'Keywords for search engines'
            }
          }
        ]
      },
      {
        key: 'status',
        type: 'select',
        props: {
          label: 'Product Status',
          placeholder: 'Select status',
          required: true,
          options: [
            { label: 'Draft', value: ProductStatus.Draft },
            { label: 'Active', value: ProductStatus.Active },
            { label: 'Out of Stock', value: ProductStatus.OutOfStock },
            { label: 'Discontinued', value: ProductStatus.Discontinued }
          ]
        }
      },
      {
        key: 'visibility',
        type: 'select',
        props: {
          label: 'Product Visibility',
          placeholder: 'Select visibility',
          required: true,
          options: [
            { label: 'Visible', value: ProductVisibility.Visible },
            { label: 'Hidden', value: ProductVisibility.Hidden },
            { label: 'Featured', value: ProductVisibility.Featured }
          ]
        }
      }
    ];
  }

  onImagesChange(images: ProductImage[]): void {
    this.model.images = images;
  }

  submitProduct(): void {
    if (this.isSubmitting) return;

    // Validate all forms
    if (!this.validateAllForms()) {
      this.errorService.addError({
        code: 'VALIDATION_ERROR',
        message: 'Please fill in all required fields',
        severity: 'warning'
      });
      return;
    }

    this.isSubmitting = true;

    // Prepare product data
    const productData: ProductCreateCommand = {
      name: this.model.basicInfo.name,
      description: this.model.basicInfo.description,
      shortDescription: this.model.basicInfo.shortDescription,
      sku: this.model.basicInfo.sku,
      barcode: this.model.basicInfo.barcode,
      price: this.model.pricing.price,
      currency: this.model.pricing.currency,
      compareAtPrice: this.model.pricing.compareAtPrice,
      categoryId: this.model.basicInfo.categoryId,
      subCategoryId: this.model.basicInfo.subCategoryId,
      stock: this.model.pricing.stock,
      lowStockThreshold: this.model.pricing.lowStockThreshold,
      status: this.model.status,
      visibility: this.model.visibility,
      images: this.model.images,
      attributes: Object.entries(this.model.attributes).map(([name, value]) => ({
        name,
        value: String(value),
        type: this.getAttributeType(name)
      })),
      tags: [],
      seo: this.model.seo
    };

    // Process attributes for specific product types
    this.processTypeSpecificAttributes(productData);

    // Create or update product
    const request$ = this.isEditMode && this.productId
      ? this.productService.updateProduct({ id: this.productId(), ...productData })
      : this.productService.createProduct(productData); request$.pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.isSubmitting = false;
        })
      ).subscribe({
        next: () => {
          const action = this.isEditMode ? 'updated' : 'created';
          this.snackBar.open(`Product ${action} successfully`, 'Close', {
            duration: 3000
          });

          // Clear any saved form data on successful submission
          this.clearSavedFormData();

          this.router.navigate(['/products/list']);
        },
        error: (error) => {
          // On error, keep the form data and don't navigate away
          this.errorService.addError({
            code: 'PRODUCT_SUBMISSION_ERROR',
            message: `Failed to ${this.isEditMode ? 'update' : 'create'} product: ${error.message}`,
            severity: 'error'
          });

          // Save form data to session storage as a backup
          if (!environment.production) {
            this.saveFormData();
            this.snackBar.open('Your data has been preserved. You can continue editing.', 'OK', {
              duration: 5000
            });
          }
        }
      });
  }

  private validateAllForms(): boolean {
    return (
      this.basicInfoForm.valid &&
      this.pricingForm.valid &&
      this.attributesForm.valid &&
      this.seoForm.valid &&
      this.model.images.length > 0
    );
  }

  formatAttributeValue(key: string | number | (string | number)[], value: any): string {
    if (value === undefined || value === null) {
      return 'Not specified';
    }

    if (key === undefined) {
      return String(value);
    }

    // Find the attribute field
    const field = this.attributesFields.find(f => f.key === key);
    if (!field) return String(value);

    // Format based on field type
    switch (field.type) {
      case 'checkbox':
        return value ? 'Yes' : 'No';
      case 'select':
        // Handle multiselect
        if (Array.isArray(value)) {
          if (field.props?.options) {
            const options = field.props.options as { label: string, value: any }[];
            return value.map(v => options.find(o => o.value === v)?.label ?? v).join(', ');
          }
          return value.join(', ');
        } else if (field.props?.options) {
          // Single select - find the label for the value
          const options = field.props.options as { label: string, value: any }[];
          const option = options.find(o => o.value === value);
          return option ? option.label : String(value);
        }
        return String(value);
      case 'color-picker':
        // Format color values appropriately
        return `<span style="color: ${value}">■</span> ${value}`;
      default:
        return String(value);
    }
  }

  formatEnumValue(value: string): string {
    if (!value) return '';

    // Convert from enum format (e.g., "OutOfStock") to display format (e.g., "Out of Stock")
    return value
      // Insert space before capitals
      .replace(/([A-Z])/g, ' $1')
      // Handle the first character
      .replace(/^./, str => str.toUpperCase())
      .trim();
  }

  // Helper method to convert field keys to string representation for use in template
  getKeyAsString(key: string | number | (string | number)[]): string {
    if (typeof key === 'string' || typeof key === 'number') {
      return String(key);
    } else if (Array.isArray(key)) {
      return key.join('.');
    }
    return '';
  }

  getAttributeType(attributeName: string): string {
    if (!this.selectedProductType) return 'string';

    const attribute = this.selectedProductType.attributes.find(a => a.id === attributeName);

    if (!attribute) return 'string';

    // Map attribute types to API expected types
    switch (attribute.type) {
      case 'number':
        return 'number';
      case 'boolean':
        return 'boolean';
      case 'multiselect':
        return 'array';
      default:
        return 'string';
    }
  }

  processTypeSpecificAttributes(productData: any): void {
    // This method will process attributes specific to certain product types
    // For example, adding special handling for clothing sizes, electronics specs, etc.

    if (!this.selectedProductType) return;

    switch (this.selectedProductType.id) {
      case 'clothing':
        // Process clothing-specific attributes
        this.processClothingAttributes(productData);
        break;
      case 'electronics':
        // Process electronics-specific attributes
        this.processElectronicsAttributes(productData);
        break;
      case 'books':
        // Process book-specific attributes
        this.processBooksAttributes(productData);
        break;
      default:
        // No special processing needed
        break;
    }
  }

  private processClothingAttributes(productData: any): void {
    // Example: Convert size/color attributes to variants if needed
    const sizes = this.model.attributes['size'];
    const colors = this.model.attributes['color'];

    if (Array.isArray(sizes) && sizes.length > 0 && colors) {
      // This is just an example of what could be done
      // In a real application, we might create product variants here
      productData.hasVariants = true;
      productData.variantAttributes = ['size', 'color'];
    }
  }

  private processElectronicsAttributes(productData: any): void {
    // Example: Add technical specifications section
    if (this.model.attributes['brand'] || this.model.attributes['model']) {
      productData.specifications = {
        brand: this.model.attributes['brand'],
        model: this.model.attributes['model'],
        warrantyMonths: this.model.attributes['warranty']
      };
    }
  }

  private processBooksAttributes(productData: any): void {
    // Example: Add publishing information
    if (this.model.attributes['author']) {
      productData.publishingDetails = {
        author: this.model.attributes['author'],
        isbn: this.model.attributes['isbn'],
        pages: this.model.attributes['pages']
      };
    }
  }

  /**
   * Save the current form data to session storage
   * This is used to preserve data during development when backend errors occur
   */
  private saveFormData(): void {
    try {
      const formData = {
        basicInfo: this.basicInfoForm.value,
        pricing: this.pricingForm.value,
        attributes: this.attributesForm.value,
        seo: this.seoForm.value,
        images: this.model.images,
        status: this.model.status,
        visibility: this.model.visibility,
        timestamp: new Date().toISOString(),
      };
      sessionStorage.setItem('product_form_backup', JSON.stringify(formData));
      console.log('Product form data saved to session storage');
    } catch (error) {
      console.error('Failed to save form data to session storage', error);
    }
  }

  /**
   * Load saved form data from session storage
   * Returns true if data was loaded successfully
   */
  private loadSavedFormData(): boolean {
    try {
      const savedData = sessionStorage.getItem('product_form_backup');
      if (!savedData) return false;

      const formData = JSON.parse(savedData);

      // Check if the saved data is still relevant (e.g., not too old)
      const savedTime = new Date(formData.timestamp);
      const currentTime = new Date();
      const hoursSinceLastSave = (currentTime.getTime() - savedTime.getTime()) / (1000 * 60 * 60);

      // Only load data if it's less than 12 hours old
      if (hoursSinceLastSave > 12) {
        this.clearSavedFormData();
        return false;
      }

      // Update the model with saved data
      if (formData.basicInfo) {
        this.model.basicInfo = { ...this.model.basicInfo, ...formData.basicInfo };
        this.basicInfoForm.patchValue(formData.basicInfo);
      }

      if (formData.pricing) {
        this.model.pricing = { ...this.model.pricing, ...formData.pricing };
        this.pricingForm.patchValue(formData.pricing);
      }

      if (formData.attributes) {
        this.model.attributes = { ...formData.attributes };
        this.attributesForm.patchValue(formData.attributes);
      }

      if (formData.seo) {
        this.model.seo = formData.seo;
        this.seoForm.patchValue(formData.seo);
      }

      if (formData.images && Array.isArray(formData.images)) {
        this.model.images = formData.images;
      }

      if (formData.status) {
        this.model.status = formData.status;
      }

      if (formData.visibility) {
        this.model.visibility = formData.visibility;
      }

      this.snackBar.open('Form data has been restored from a previous session', 'Dismiss', {
        duration: 5000
      });

      return true;
    } catch (error) {
      console.error('Failed to load form data from session storage', error);
      return false;
    }
  }

  /**
   * Clear any saved form data from session storage
   */
  private clearSavedFormData(): void {
    try {
      sessionStorage.removeItem('product_form_backup');
    } catch (error) {
      console.error('Failed to clear form data from session storage', error);
    }
  }

}