// src/app/features/products/components/dynamic-product-form/dynamic-product-form.component.ts
import { Component, OnInit, OnDestroy, input, signal, computed, inject } from '@angular/core';
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
import { Router, ActivatedRoute } from '@angular/router';
import { FormlyModule, FormlyFieldConfig } from '@ngx-formly/core';
import { Subject, combineLatest, of } from 'rxjs';
import { finalize, takeUntil, startWith } from 'rxjs/operators';

import { ProductTypeService } from '../../../../core/services/product-type.service';
import { ProductService } from '../../../../core/services/product.service';
import { ErrorService } from '../../../../core/services/error.service';
import { Product, ProductCreateCommand, ProductImage, ProductStatus, ProductVisibility } from '../../../../shared/models/product.model';
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
  templateUrl: './dynamic-product-form.component.html',
  styleUrls: ['./dynamic-product-form.component.scss'],
})
export class DynamicProductFormComponent implements OnInit, OnDestroy {
  // Forms for each step
  basicInfoForm = new FormGroup({});
  pricingForm = new FormGroup({});
  attributesForm = new FormGroup({});
  seoForm = new FormGroup({});

  // Formly fields configuration
  basicInfoFields = signal<FormlyFieldConfig[]>([]);
  pricingFields = signal<FormlyFieldConfig[]>([]);
  attributesFields = signal<FormlyFieldConfig[]>([]);
  seoFields = signal<FormlyFieldConfig[]>([]);

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

  // Component state as signals
  isEditMode = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);
  productId = input<string>();
  productTypes = signal<ProductType[]>([]);
  selectedProductType = signal<ProductType | undefined>(undefined);

  // Computed properties
  hasAttributes = computed(() => this.attributesFields().length > 0);
  canSubmit = computed(() => !this.isSubmitting());

  private readonly destroy$ = new Subject<void>();

  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly productService = inject(ProductService);
  private readonly productTypeService = inject(ProductTypeService);
  private readonly categoryService = inject(CategoryService);
  private readonly errorService = inject(ErrorService);
  private readonly snackBar = inject(MatSnackBar);

  ngOnInit(): void {
    this.loadProductTypes();
    this.initBasicInfoFields();
    this.initPricingFields();
    this.initSeoFields();

    // Try to load saved form data if it exists
    if (!environment.production) {
      this.loadSavedFormData();
    }

    // Get product ID either from input or route parameters
    const routeParamId = this.route.snapshot.paramMap.get('id');
    const effectiveProductId = routeParamId || this.productId();

    if (effectiveProductId) {
      this.isEditMode.set(true);
      this.productService.getProduct(effectiveProductId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(product => {
          if (product) {
            this.loadExistingProduct(product);
          }
        });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadProductTypes(): void {
    this.productTypeService.getProductTypes()
      .pipe(takeUntil(this.destroy$))
      .subscribe(types => {
        this.productTypes.set(types);
        if (types.length > 0 && !this.selectedProductType()) {
          this.onProductTypeChange(types[0].id);
        }
      });
  }

  onProductTypeChange(typeId: string): void {
    const selectedType = this.productTypes().find(type => type.id === typeId);
    this.selectedProductType.set(selectedType);
    this.model.basicInfo.productTypeId = typeId;

    // Reset attributes when product type changes
    this.model.attributes = {};
    this.attributesForm = new FormGroup({});

    // Load attribute fields for the selected product type
    if (selectedType) {
      this.productTypeService.getAttributeFieldsByType(typeId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(fields => {
          this.attributesFields.set(fields);
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
      this.basicInfoFields.set([
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
      ]);
    });
  }

  private initPricingFields(): void {
    this.pricingFields.set([
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
    ]);
  }

  private initSeoFields(): void {
    this.seoFields.set([
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
    ]);
  }

  onImagesChange(images: ProductImage[]): void {
    this.model.images = images;
  }

  submitProduct(): void {
    if (this.isSubmitting()) return;

    // Validate all forms
    if (!this.validateAllForms()) {
      this.errorService.addError({
        code: 'VALIDATION_ERROR',
        message: 'Please fill in all required fields',
        severity: 'warning'
      });
      return;
    }

    this.isSubmitting.set(true);

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
    const request$ = this.isEditMode() && this.productId()
      ? this.productService.updateProduct({ id: this.productId()!, ...productData })
      : this.productService.createProduct(productData);

    request$.pipe(
      takeUntil(this.destroy$),
      finalize(() => {
        this.isSubmitting.set(false);
      })
    ).subscribe({
      next: () => {
        const action = this.isEditMode() ? 'updated' : 'created';
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
          message: `Failed to ${this.isEditMode() ? 'update' : 'create'} product: ${error.message}`,
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
    const field = this.attributesFields().find(f => f.key === key);
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
    if (!this.selectedProductType()) return 'string';

    const attribute = this.selectedProductType()?.attributes.find(a => a.id === attributeName);

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

    if (!this.selectedProductType()) return;

    switch (this.selectedProductType()?.id) {
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

  /**
   * Loads an existing product data into the form
   * @param product The product to load
   */
  private loadExistingProduct(product: Product): void {
    // Update the selected product type first to ensure attributes are loaded
    if (product.category) {
      this.onProductTypeChange(product.category.id);
    }

    // Map product data to our form model
    this.model = {
      basicInfo: {
        name: product.name,
        description: product.description,
        shortDescription: product.shortDescription || '',
        sku: product.sku,
        barcode: product.barcode || '',
        categoryId: product.category.id,
        subCategoryId: product.subCategory?.id || '',
        productTypeId: product.category.id
      },
      pricing: {
        price: product.price,
        currency: product.currency || 'USD',
        compareAtPrice: product.compareAtPrice,
        stock: product.stock,
        lowStockThreshold: product.lowStockThreshold
      },
      attributes: {},
      status: product.status || ProductStatus.Draft,
      visibility: product.visibility || ProductVisibility.Hidden,
      images: product.images || [],
      seo: product.seo || {
        title: product.name,
        description: product.shortDescription || ''
      }
    };

    // Handle product attributes
    if (product.attributes && Array.isArray(product.attributes)) {
      product.attributes.forEach((attr) => {
        // Convert attribute value based on its type
        let value: string = attr.value;

        switch (attr.type) {
          case 'number':
            value = attr.value;
            break;
          case 'boolean':
            value = attr.value;
            break;
          case 'array':
            try {
              value = Array.isArray(attr.value) ? attr.value : JSON.parse(attr.value);
            } catch {
              value = attr.value;
            }
            break;
        }

        this.model.attributes[attr.name] = value;
      });
    }

    // Update all form groups with the model data
    this.basicInfoForm.patchValue(this.model.basicInfo);
    this.pricingForm.patchValue(this.model.pricing);

    // Need to wait a bit for attributesFields to be populated before patching attributes
    setTimeout(() => {
      this.attributesForm.patchValue(this.model.attributes);
    }, 500);

    // Update SEO form
    if (this.model.seo) {
      this.seoForm.patchValue({ seo: this.model.seo });

      // Also update status and visibility which are in the SEO form
      this.seoForm.patchValue({
        status: this.model.status,
        visibility: this.model.visibility
      });
    }

    console.log('Product loaded successfully for editing', this.model);
  }
}