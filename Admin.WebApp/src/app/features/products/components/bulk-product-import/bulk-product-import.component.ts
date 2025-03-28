// src/app/features/products/components/bulk-product-import/bulk-product-import.component.ts
import { Component, ViewChild, ElementRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatStepperModule } from '@angular/material/stepper';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FormsModule } from '@angular/forms';
import { read, utils, WorkBook, WorkSheet, write } from 'xlsx';
import { ProductService } from '../../../../core/services/product.service';
import { ErrorService } from '../../../../core/services/error.service';
import { CategoryService } from '../../../../core/services/category.service';
import { ProductTypeService } from '../../../../core/services/product-type.service';
import { Category } from '../../../../shared/models/category.model';
import { ProductType } from '../../../../shared/models/product-type.model';
import { forkJoin } from 'rxjs';

interface ImportRow {
  rowNumber: number;
  valid: boolean;
  errors: string[];
  data: Record<string, any>;
  productId?: string;
}

@Component({
  selector: 'app-bulk-product-import',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatStepperModule,
    MatCardModule,
    MatProgressBarModule,
    MatSnackBarModule
  ],
  template: `
    <div class="import-container">
      <mat-card>
        <mat-card-header>
          <mat-card-title>Bulk Product Import</mat-card-title>
          <mat-card-subtitle>
            Import multiple products at once from Excel or CSV files
          </mat-card-subtitle>
        </mat-card-header>
        
        <mat-card-content>
          <mat-stepper linear #stepper>
            <!-- Step 1: Upload File -->
            <mat-step [completed]="!!workbook">
              <ng-template matStepLabel>Upload File</ng-template>
              
              <div class="step-content">
                <h3>Upload Product Data</h3>
                <p class="description">Upload an Excel (.xlsx) or CSV file containing your product data.</p>
                
                <div 
                  class="upload-area"
                  [class.dragover]="isDragging"
                  (dragover)="onDragOver($event)"
                  (dragleave)="onDragLeave($event)"
                  (drop)="onDrop($event)">
                  
                  <div class="upload-prompt">
                    <mat-icon>cloud_upload</mat-icon>
                    <p>Drag & drop your file here or</p>
                    <button 
                      mat-stroked-button 
                      color="primary" 
                      (click)="fileInput.click()">
                      Browse Files
                    </button>
                    <input 
                      #fileInput 
                      type="file" 
                      accept=".xlsx,.csv"
                      style="display: none"
                      (change)="onFileSelected($event)">
                    
                    <p class="file-info" *ngIf="fileName">
                      Selected file: <strong>{{ fileName }}</strong>
                    </p>
                  </div>
                </div>
                
                <div class="template-section">
                  <h4>Need a template?</h4>
                  <p>Download our template file to get started:</p>
                  <button 
                    mat-stroked-button 
                    color="primary"
                    (click)="downloadTemplate()">
                    <mat-icon>download</mat-icon>
                    Download Template
                  </button>
                </div>
                
                <div class="step-actions">
                  <button 
                    mat-button 
                    matStepperNext
                    color="primary"
                    [disabled]="!workbook">
                    Next
                  </button>
                </div>
              </div>
            </mat-step>
            
            <!-- Step 2: Validate Data -->
            <mat-step [completed]="isValidationComplete">
              <ng-template matStepLabel>Validate Data</ng-template>
              
              <div class="step-content">
                <h3>Validate Product Data</h3>
                <p class="description">Review and validate your product data before importing.</p>
                
                <div class="validation-actions">
                  <button 
                    mat-raised-button 
                    color="primary"
                    [disabled]="isValidating"
                    (click)="validateData()">
                    <mat-icon>check_circle</mat-icon>
                    {{ isValidationComplete ? 'Re-validate Data' : 'Validate Data' }}
                  </button>
                </div>
                
                <div class="validation-progress" *ngIf="isValidating">
                  <mat-progress-bar mode="determinate" [value]="validationProgress"></mat-progress-bar>
                  <p>Validating... {{ validationProgress }}%</p>
                </div>
                
                <div class="validation-results" *ngIf="isValidationComplete">
                  <div class="results-summary">
                    <div class="summary-card valid">
                      <mat-icon>check_circle</mat-icon>
                      <div class="summary-values">
                        <span class="value">{{ validRows.length }}</span>
                        <span class="label">Valid Rows</span>
                      </div>
                    </div>
                    
                    <div class="summary-card invalid" *ngIf="invalidRows.length > 0">
                      <mat-icon>error</mat-icon>
                      <div class="summary-values">
                        <span class="value">{{ invalidRows.length }}</span>
                        <span class="label">Invalid Rows</span>
                      </div>
                    </div>
                    
                    <div class="summary-card total">
                      <mat-icon>list</mat-icon>
                      <div class="summary-values">
                        <span class="value">{{ processedRows.length }}</span>
                        <span class="label">Total Rows</span>
                      </div>
                    </div>
                  </div>
                  
                  <div class="results-table" *ngIf="invalidRows.length > 0">
                    <h4>Errors Found</h4>
                    <div class="table-wrapper">
                      <table>
                        <thead>
                          <tr>
                            <th>Row</th>
                            <th>Errors</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr *ngFor="let row of invalidRows">
                            <td>{{ row.rowNumber }}</td>
                            <td>
                              <ul class="error-list">
                                <li *ngFor="let error of row.errors">{{ error }}</li>
                              </ul>
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                  </div>
                </div>
                
                <div class="step-actions">
                  <button mat-button matStepperPrevious>Back</button>
                  <button 
                    mat-button 
                    matStepperNext
                    color="primary"
                    [disabled]="!isValidationComplete || validRows.length === 0">
                    Next
                  </button>
                </div>
              </div>
            </mat-step>
            
            <!-- Step 3: Import Data -->
            <mat-step>
              <ng-template matStepLabel>Import Products</ng-template>
              
              <div class="step-content">
                <h3>Import Products</h3>
                <p class="description">
                  Ready to import {{ validRows.length }} products. This process may take a few minutes.
                </p>
                
                <div class="import-options">
                  <div class="option">
                    <label>
                      <input type="checkbox" [(ngModel)]="importOptions.skipExisting">
                      Skip existing products (match by SKU)
                    </label>
                  </div>
                  
                  <div class="option">
                    <label>
                      <input type="checkbox" [(ngModel)]="importOptions.updateExisting">
                      Update existing products if found
                    </label>
                  </div>
                  
                  <div class="option">
                    <label>
                      <input type="checkbox" [(ngModel)]="importOptions.draftMode">
                      Import as draft (products will not be visible)
                    </label>
                  </div>
                </div>
                
                <div class="import-actions">
                  <button 
                    mat-raised-button 
                    color="primary"
                    [disabled]="isImporting || validRows.length === 0"
                    (click)="importProducts()">
                    <mat-icon>upload</mat-icon>
                    Import {{ validRows.length }} Products
                  </button>
                </div>
                
                <div class="import-progress" *ngIf="isImporting">
                  <mat-progress-bar mode="determinate" [value]="importProgress"></mat-progress-bar>
                  <p>Importing... {{ importProgress }}%</p>
                  <p class="import-status">{{ currentImportStatus }}</p>
                </div>
                
                <div class="import-results" *ngIf="isImportComplete">
                  <div class="results-summary">
                    <div class="summary-card success">
                      <mat-icon>check_circle</mat-icon>
                      <div class="summary-values">
                        <span class="value">{{ importResults.success }}</span>
                        <span class="label">Successfully Imported</span>
                      </div>
                    </div>
                    
                    <div class="summary-card skipped" *ngIf="importResults.skipped > 0">
                      <mat-icon>skip_next</mat-icon>
                      <div class="summary-values">
                        <span class="value">{{ importResults.skipped }}</span>
                        <span class="label">Skipped</span>
                      </div>
                    </div>
                    
                    <div class="summary-card failed" *ngIf="importResults.failed > 0">
                      <mat-icon>error</mat-icon>
                      <div class="summary-values">
                        <span class="value">{{ importResults.failed }}</span>
                        <span class="label">Failed</span>
                      </div>
                    </div>
                  </div>
                  
                  <div class="results-actions">
                    <button 
                      mat-stroked-button 
                      (click)="downloadResultsReport()">
                      <mat-icon>download</mat-icon>
                      Download Import Report
                    </button>
                    
                    <button 
                      mat-stroked-button 
                      color="primary"
                      routerLink="/products/list">
                      <mat-icon>view_list</mat-icon>
                      View Products
                    </button>
                  </div>
                </div>
                
                <div class="step-actions">
                  <button mat-button matStepperPrevious [disabled]="isImporting">Back</button>
                  <button 
                    mat-button 
                    (click)="resetImport()"
                    [disabled]="isImporting">
                    Start Over
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
    .import-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 1rem;
    }
    
    .step-content {
      margin: 1.5rem 0;
    }
    
    .description {
      color: #666;
      margin-bottom: 1.5rem;
    }
    
    .upload-area {
      border: 2px dashed #ccc;
      border-radius: 8px;
      padding: 2rem;
      text-align: center;
      transition: all 0.2s ease;
      background-color: #fafafa;
      cursor: pointer;
      margin-bottom: 1.5rem;
    }
    
    .upload-area.dragover {
      border-color: #3f51b5;
      background-color: rgba(63, 81, 181, 0.05);
    }
    
    .upload-prompt {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.5rem;
    }
    
    .upload-prompt mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: #3f51b5;
    }
    
    .file-info {
      margin-top: 1rem;
      background-color: #e3f2fd;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      font-size: 0.875rem;
    }
    
    .template-section {
      margin-top: 2rem;
      padding: 1rem;
      background-color: #f5f5f5;
      border-radius: 8px;
    }
    
    .template-section h4 {
      margin-top: 0;
    }
    
    .step-actions {
      display: flex;
      justify-content: flex-end;
      gap: 1rem;
      margin-top: 2rem;
    }
    
    .validation-actions,
    .import-actions {
      margin: 1.5rem 0;
      display: flex;
      justify-content: center;
    }
    
    .validation-progress,
    .import-progress {
      margin: 1.5rem 0;
      text-align: center;
    }
    
    .import-status {
      font-style: italic;
      margin-top: 0.5rem;
    }
    
    .results-summary {
      display: flex;
      flex-wrap: wrap;
      gap: 1rem;
      margin-bottom: 1.5rem;
    }
    
    .summary-card {
      flex: 1;
      min-width: 200px;
      padding: 1rem;
      border-radius: 8px;
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    
    .summary-card.valid,
    .summary-card.success {
      background-color: #e8f5e9;
    }
    
    .summary-card.invalid,
    .summary-card.failed {
      background-color: #ffebee;
    }
    
    .summary-card.total {
      background-color: #e3f2fd;
    }
    
    .summary-card.skipped {
      background-color: #fff8e1;
    }
    
    .summary-card mat-icon {
      font-size: 32px;
      width: 32px;
      height: 32px;
    }
    
    .summary-card.valid mat-icon,
    .summary-card.success mat-icon {
      color: #4caf50;
    }
    
    .summary-card.invalid mat-icon,
    .summary-card.failed mat-icon {
      color: #f44336;
    }
    
    .summary-card.total mat-icon {
      color: #2196f3;
    }
    
    .summary-card.skipped mat-icon {
      color: #ff9800;
    }
    
    .summary-values {
      display: flex;
      flex-direction: column;
    }
    
    .summary-values .value {
      font-size: 1.5rem;
      font-weight: bold;
    }
    
    .summary-values .label {
      font-size: 0.875rem;
      color: #666;
    }
    
    .table-wrapper {
      overflow-x: auto;
      margin-bottom: 1.5rem;
    }
    
    table {
      width: 100%;
      border-collapse: collapse;
    }
    
    th, td {
      padding: 0.75rem;
      text-align: left;
      border-bottom: 1px solid #eee;
    }
    
    th {
      background-color: #f5f5f5;
      font-weight: bold;
    }
    
    .error-list {
      margin: 0;
      padding-left: 1.5rem;
      color: #f44336;
    }
    
    .import-options {
      margin: 1.5rem 0;
      padding: 1rem;
      background-color: #f5f5f5;
      border-radius: 8px;
    }
    
    .option {
      margin-bottom: 0.5rem;
    }
    
    .results-actions {
      display: flex;
      gap: 1rem;
      margin-top: 1.5rem;
      justify-content: center;
    }
  `]
})
export class BulkProductImportComponent implements OnInit {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  // File upload
  isDragging = false;
  fileName = '';
  workbook: WorkBook | null = null;

  // Validation
  isValidating = false;
  isValidationComplete = false;
  validationProgress = 0;
  processedRows: ImportRow[] = [];

  // Import
  isImporting = false;
  isImportComplete = false;
  importProgress = 0;
  currentImportStatus = '';
  importOptions = {
    skipExisting: true,
    updateExisting: false,
    draftMode: true
  };
  importResults = {
    success: 0,
    failed: 0,
    skipped: 0
  };

  // Cached data
  categories: Category[] = [];
  productTypes: ProductType[] = [];

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private productTypeService: ProductTypeService,
    private errorService: ErrorService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    // Load reference data
    forkJoin({
      categories: this.categoryService.getCategories(),
      productTypes: this.productTypeService.getProductTypes()
    }).subscribe({
      next: (data) => {
        this.categories = data.categories;
        this.productTypes = data.productTypes;
      },
      error: (error) => {
        this.errorService.addError({
          code: 'REFERENCE_DATA_ERROR',
          message: 'Failed to load reference data',
          severity: 'error'
        });
      }
    });
  }

  get validRows(): ImportRow[] {
    return this.processedRows.filter(row => row.valid);
  }

  get invalidRows(): ImportRow[] {
    return this.processedRows.filter(row => !row.valid);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;

    if (event.dataTransfer?.files.length) {
      this.handleFile(event.dataTransfer.files[0]);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.handleFile(input.files[0]);
    }
  }

  handleFile(file: File): void {
    // Check file type
    const validExtensions = ['.xlsx', '.csv'];
    const fileExt = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();

    if (!validExtensions.includes(fileExt)) {
      this.errorService.addError({
        code: 'INVALID_FILE_TYPE',
        message: 'Please upload an Excel (.xlsx) or CSV file',
        severity: 'warning'
      });
      return;
    }

    this.fileName = file.name;

    // Read file
    const reader = new FileReader();
    reader.onload = (e: ProgressEvent<FileReader>) => {
      try {
        const data = e.target?.result;
        this.workbook = read(data, { type: 'array' });

        // Reset validation state
        this.isValidationComplete = false;
        this.processedRows = [];
      } catch (error) {
        this.errorService.addError({
          code: 'FILE_READ_ERROR',
          message: 'Failed to read file. Please make sure it is a valid Excel or CSV file.',
          severity: 'error'
        });
        this.workbook = null;
        this.fileName = '';
      }
    };

    reader.readAsArrayBuffer(file);
  }

  downloadTemplate(): void {
    // Create template workbook
    const wb = utils.book_new();

    // Create headers
    const headers = [
      'name', 'description', 'shortDescription', 'sku', 'barcode',
      'price', 'compareAtPrice', 'currency', 'stock', 'lowStockThreshold',
      'categoryName', 'productType', 'status', 'visibility',
      'imageUrl1', 'imageUrl2', 'imageUrl3',
      'attribute1Name', 'attribute1Value', 'attribute2Name', 'attribute2Value',
      'seoTitle', 'seoDescription', 'seoKeywords'
    ];

    // Create sample data
    const sampleData = [
      {
        name: 'Sample T-Shirt',
        description: 'A comfortable cotton t-shirt',
        shortDescription: 'Cotton t-shirt',
        sku: 'TS-001',
        barcode: '123456789012',
        price: 19.99,
        compareAtPrice: 24.99,
        currency: 'USD',
        stock: 100,
        lowStockThreshold: 10,
        categoryName: 'Clothing',
        productType: 'clothing',
        status: 'active',
        visibility: 'visible',
        imageUrl1: 'https://example.com/image1.jpg',
        imageUrl2: 'https://example.com/image2.jpg',
        imageUrl3: '',
        attribute1Name: 'sizes',
        attribute1Value: 'S,M,L,XL',
        attribute2Name: 'colors',
        attribute2Value: 'Red,Blue,Green',
        seoTitle: 'Sample T-Shirt - High Quality',
        seoDescription: 'Buy our comfortable cotton t-shirt',
        seoKeywords: 'tshirt,cotton,clothing'
      },
      {
        name: 'Wireless Headphones',
        description: 'Premium wireless headphones with noise cancellation',
        shortDescription: 'Wireless headphones',
        sku: 'HP-001',
        barcode: '123456789013',
        price: 149.99,
        compareAtPrice: 199.99,
        currency: 'USD',
        stock: 50,
        lowStockThreshold: 5,
        categoryName: 'Electronics',
        productType: 'electronics',
        status: 'active',
        visibility: 'featured',
        imageUrl1: 'https://example.com/headphones1.jpg',
        imageUrl2: '',
        imageUrl3: '',
        attribute1Name: 'brand',
        attribute1Value: 'SoundMaster',
        attribute2Name: 'color',
        attribute2Value: 'Black',
        seoTitle: 'Premium Wireless Headphones',
        seoDescription: 'High-quality wireless headphones with noise cancellation',
        seoKeywords: 'headphones,wireless,electronics'
      }
    ];

    // Add instructions sheet
    const instructionsWs = utils.aoa_to_sheet([
      ['Product Import Template - Instructions'],
      [''],
      ['1. Do not modify the header row'],
      ['2. Each row represents one product'],
      ['3. Required fields: name, description, sku, price, currency, stock, categoryName, productType'],
      ['4. Multiple attribute values should be comma-separated'],
      ['5. Valid product types: ' + this.productTypes.map(t => t.id).join(', ')],
      ['6. Valid statuses: draft, active, out_of_stock, discontinued'],
      ['7. Valid visibility options: visible, hidden, featured'],
      ['8. Valid currencies: USD, EUR, GBP, CAD, AUD']
    ]);

    // Adjust column widths for instructions
    const instructionsCols = [{ wch: 80 }]; // Set width for first column
    instructionsWs['!cols'] = instructionsCols;

    // Add template sheet with headers and sample data
    const templateWs = utils.json_to_sheet(sampleData, { header: headers });

    // Adjust column widths for template
    const templateCols = headers.map(h => ({ wch: 20 }));
    templateWs['!cols'] = templateCols;

    // Add sheets to workbook
    utils.book_append_sheet(wb, instructionsWs, 'Instructions');
    utils.book_append_sheet(wb, templateWs, 'Template');

    // Generate buffer and create download link
    const wbout = write(wb, { bookType: 'xlsx', type: 'array' });

    // Create Blob and download
    const blob = new Blob([wbout], { type: 'application/octet-stream' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'product_import_template.xlsx';
    a.click();

    // Clean up
    URL.revokeObjectURL(url);
  }

  validateData(): void {
    if (!this.workbook || this.isValidating) return;

    this.isValidating = true;
    this.validationProgress = 0;
    this.processedRows = [];

    // Get the first sheet
    const sheetName = this.workbook.SheetNames[0];
    const worksheet = this.workbook.Sheets[sheetName];

    // Convert to JSON
    const rows = utils.sheet_to_json(worksheet, { raw: false }) as Record<string, any>[];

    if (rows.length === 0) {
      this.errorService.addError({
        code: 'EMPTY_FILE',
        message: 'The file contains no data rows',
        severity: 'warning'
      });
      this.isValidating = false;
      return;
    }

    // Process rows in batches to avoid blocking UI
    const batchSize = 10;
    const totalRows = rows.length;
    let processedCount = 0;

    const processBatch = (startIndex: number) => {
      const endIndex = Math.min(startIndex + batchSize, totalRows);

      for (let i = startIndex; i < endIndex; i++) {
        const row = rows[i];
        this.processedRows.push(this.validateRow(row, i + 1));
        processedCount++;
      }

      // Update progress
      this.validationProgress = Math.round((processedCount / totalRows) * 100);

      // Process next batch or finish
      if (processedCount < totalRows) {
        setTimeout(() => processBatch(endIndex), 0);
      } else {
        this.isValidating = false;
        this.isValidationComplete = true;

        if (this.invalidRows.length > 0) {
          this.snackBar.open(
            `Validation complete with ${this.invalidRows.length} errors`,
            'Dismiss',
            { duration: 5000 }
          );
        } else {
          this.snackBar.open(
            `All ${this.validRows.length} rows are valid`,
            'Dismiss',
            { duration: 3000 }
          );
        }
      }
    };

    // Start processing
    processBatch(0);
  }

  validateRow(row: Record<string, any>, rowNumber: number): ImportRow {
    const errors: string[] = [];
    const data = { ...row };

    // Required fields validation
    const requiredFields = ['name', 'description', 'sku', 'price', 'stock', 'categoryName', 'productType'];

    for (const field of requiredFields) {
      if (!row[field]) {
        errors.push(`Missing required field: ${field}`);
      }
    }

    // Price validation
    if (row['price'] && isNaN(parseFloat(row['price']))) {
      errors.push('Price must be a number');
    }

    // Stock validation
    if (row['stock'] && isNaN(parseInt(row['stock']))) {
      errors.push('Stock must be an integer');
    }

    // Category validation
    if (row['categoryName'] && !this.findCategoryByName(row['categoryName'])) {
      errors.push(`Category not found: ${row['categoryName']}`);
    }

    // Product type validation
    if (row['productType'] && !this.findProductTypeById(row['productType'])) {
      errors.push(`Product type not found: ${row['productType']}`);
    }

    // Status validation
    if (row['status'] && !['draft', 'active', 'out_of_stock', 'discontinued'].includes(row['status'].toLowerCase())) {
      errors.push(`Invalid status: ${row['status']}. Must be one of: draft, active, out_of_stock, discontinued`);
    }

    // Visibility validation
    if (row['visibility'] && !['visible', 'hidden', 'featured'].includes(row['visibility'].toLowerCase())) {
      errors.push(`Invalid visibility: ${row['visibility']}. Must be one of: visible, hidden, featured`);
    }

    // Currency validation
    if (row['currency'] && !['USD', 'EUR', 'GBP', 'CAD', 'AUD'].includes(row['currency'].toUpperCase())) {
      errors.push(`Invalid currency: ${row['currency']}. Must be one of: USD, EUR, GBP, CAD, AUD`);
    }

    return {
      rowNumber,
      valid: errors.length === 0,
      errors,
      data
    };
  }

  importProducts(): void {
    if (this.isImporting || this.validRows.length === 0) return;

    this.isImporting = true;
    this.isImportComplete = false;
    this.importProgress = 0;
    this.currentImportStatus = 'Preparing import...';

    // Reset results
    this.importResults = {
      success: 0,
      failed: 0,
      skipped: 0
    };

    // Process rows in batches
    const batchSize = 5; // Small batch size for API calls
    const totalRows = this.validRows.length;
    let processedCount = 0;

    const processBatch = async (startIndex: number) => {
      const endIndex = Math.min(startIndex + batchSize, totalRows);

      for (let i = startIndex; i < endIndex; i++) {
        const row = this.validRows[i];
        this.currentImportStatus = `Importing product ${i + 1} of ${totalRows}: ${row.data['name']}`;

        try {
          // If skip existing is enabled, check if product exists
          if (this.importOptions.skipExisting) {
            // In a real app, we would check if the product exists
            // For now, we'll simulate this with a random outcome
            const exists = Math.random() > 0.8;

            if (exists) {
              if (this.importOptions.updateExisting) {
                // Update existing product
                row.productId = 'updated-' + Math.random().toString(36).substring(2);
                this.importResults.success++;
              } else {
                // Skip existing product
                row.productId = 'skipped';
                this.importResults.skipped++;
                processedCount++;
                continue;
              }
            }
          }

          // Convert row data to product
          const product = this.convertRowToProduct(row.data);

          // Simulate API call
          await new Promise(resolve => setTimeout(resolve, 500 + Math.random() * 1000));

          // Simulate success (90% chance)
          if (Math.random() > 0.1) {
            row.productId = 'imported-' + Math.random().toString(36).substring(2);
            this.importResults.success++;
          } else {
            throw new Error('Simulated import failure');
          }
        } catch (error) {
          console.error('Import error for row', row.rowNumber, error);
          row.errors.push('Import failed: ' + (error as Error).message);
          this.importResults.failed++;
        }

        processedCount++;
        this.importProgress = Math.round((processedCount / totalRows) * 100);
      }

      // Process next batch or finish
      if (processedCount < totalRows) {
        setTimeout(() => processBatch(endIndex), 0);
      } else {
        this.isImporting = false;
        this.isImportComplete = true;
        this.currentImportStatus = 'Import complete!';

        this.snackBar.open(
          `Import complete: ${this.importResults.success} succeeded, ${this.importResults.failed} failed, ${this.importResults.skipped} skipped`,
          'Dismiss',
          { duration: 5000 }
        );
      }
    };

    // Start processing
    processBatch(0);
  }

  resetImport(): void {
    this.workbook = null;
    this.fileName = '';
    this.isValidationComplete = false;
    this.processedRows = [];
    this.isImportComplete = false;

    // Reset file input
    if (this.fileInput?.nativeElement) {
      this.fileInput.nativeElement.value = '';
    }
  }

  downloadResultsReport(): void {
    // Create report workbook
    const wb = utils.book_new();

    // Prepare data
    const reportData = this.processedRows.map(row => ({
      'Row': row.rowNumber,
      'Product Name': row.data['name'],
      'SKU': row.data['sku'],
      'Status': row.valid ? (row.productId ? 'Imported' : 'Ready') : 'Invalid',
      'Product ID': row.productId || '',
      'Errors': row.errors.join('; ')
    }));

    // Create summary sheet
    const summaryData = [
      ['Import Results Summary'],
      [''],
      ['Date:', new Date().toLocaleString()],
      ['File:', this.fileName],
      [''],
      ['Total Rows:', this.processedRows.length],
      ['Valid Rows:', this.validRows.length],
      ['Invalid Rows:', this.invalidRows.length],
      [''],
      ['Import Results:'],
      ['Successfully Imported:', this.importResults.success],
      ['Failed:', this.importResults.failed],
      ['Skipped:', this.importResults.skipped]
    ];

    const summaryWs = utils.aoa_to_sheet(summaryData);

    // Create details sheet
    const detailsWs = utils.json_to_sheet(reportData);

    // Add sheets to workbook
    utils.book_append_sheet(wb, summaryWs, 'Summary');
    utils.book_append_sheet(wb, detailsWs, 'Details');

    // Generate buffer and create download link
    const wbout = write(wb, { bookType: 'xlsx', type: 'array' });

    // Create Blob and download
    const blob = new Blob([wbout], { type: 'application/octet-stream' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `product_import_report_${new Date().toISOString().slice(0, 10)}.xlsx`;
    a.click();

    // Clean up
    URL.revokeObjectURL(url);
  }

  private findCategoryByName(name: string): Category | undefined {
    return this.categories.find(c =>
      c.name.toLowerCase() === name.toLowerCase()
    );
  }

  private findProductTypeById(id: string): ProductType | undefined {
    return this.productTypes.find(t => t.id === id);
  }

  private convertRowToProduct(row: Record<string, any>): any {
    // In a real app, this would convert the row data to a proper product object
    // For now, we'll just return a simplified version

    const category = this.findCategoryByName(row['categoryName']);

    return {
      name: row['name'],
      description: row['description'],
      shortDescription: row['shortDescription'],
      sku: row['sku'],
      barcode: row['barcode'],
      price: parseFloat(row['price']),
      currency: (row['currency'] || 'USD').toUpperCase(),
      compareAtPrice: row['compareAtPrice'] ? parseFloat(row['compareAtPrice']) : undefined,
      categoryId: category?.id,
      stock: parseInt(row['stock']),
      lowStockThreshold: row['lowStockThreshold'] ? parseInt(row['lowStockThreshold']) : undefined,
      status: row['status'] || (this.importOptions.draftMode ? 'draft' : 'active'),
      visibility: row['visibility'] || (this.importOptions.draftMode ? 'hidden' : 'visible'),
      productTypeId: row['productType'],
      // Images would be processed here
      // Attributes would be processed here
    };
  }
}