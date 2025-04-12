// src/app/features/products/pages/enhanced-product-form/enhanced-product-form.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DynamicProductFormComponent } from '../components/dynamic-product-form/dynamic-product-form.component';
import { BarcodeScannerComponent } from '../components/barcode-scanner/barcode-scanner.component';
import { BulkProductImportComponent } from '../components/bulk-product-import/bulk-product-import.component';

@Component({
  selector: 'app-enhanced-product-form',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTabsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    DynamicProductFormComponent,
    BarcodeScannerComponent,
    BulkProductImportComponent
  ],
  template: `
    <div class="enhanced-product-form">
      <div class="page-header">
        <h1>{{ isEditMode ? 'Edit Product' : 'Add Product' }}</h1>
        <div class="actions">
          <button mat-button routerLink="/products/list">
            <mat-icon>arrow_back</mat-icon>
            Back to Products
          </button>
        </div>
      </div>
      
      <mat-card>
        <mat-card-content>
          <mat-tab-group [selectedIndex]="selectedTab" (selectedIndexChange)="onTabChange($event)">
            <mat-tab label="Single Product">
              <div class="tab-content">
                <app-dynamic-product-form
                  [attr.productId]="productId">
                </app-dynamic-product-form>
              </div>
            </mat-tab>
            
            <mat-tab label="Bulk Import">
              <div class="tab-content">
                <app-bulk-product-import></app-bulk-product-import>
              </div>
            </mat-tab>
            
            <mat-tab label="Barcode Scanner">
              <div class="tab-content">
                <div class="scanner-section">
                  <h2>Scan Barcode to Find or Create Product</h2>
                  <p class="description">
                    Use your device's camera to scan a product barcode. The system will automatically
                    search for the product, or allow you to create a new one if it doesn't exist.
                  </p>
                  
                  <app-barcode-scanner
                    (codeScanned)="onBarcodeScanned($event)">
                  </app-barcode-scanner>
                  
                  <div class="barcode-results" *ngIf="barcodeSearchResults.length > 0">
                    <h3>Search Results</h3>
                    <p *ngIf="barcodeSearchResults.length === 0">No products found with this barcode.</p>
                    
                    <div class="result-cards">
                      <mat-card *ngFor="let product of barcodeSearchResults" class="result-card">
                        <mat-card-header>
                          <mat-card-title>{{ product.name }}</mat-card-title>
                          <mat-card-subtitle>SKU: {{ product.sku }}</mat-card-subtitle>
                        </mat-card-header>
                        
                        <img *ngIf="product.images?.length" [src]="product.images[0].url" [alt]="product.name">
                        
                        <mat-card-content>
                          <p><strong>Price:</strong> {{ product.price | currency:product.currency }}</p>
                          <p><strong>Stock:</strong> {{ product.stock }}</p>
                          <p><strong>Category:</strong> {{ product.category?.name }}</p>
                        </mat-card-content>
                        
                        <mat-card-actions>
                          <button mat-button color="primary" [routerLink]="['/products/edit', product.id]">
                            <mat-icon>edit</mat-icon>
                            Edit
                          </button>
                        </mat-card-actions>
                      </mat-card>
                      
                      <mat-card class="result-card new-product-card" *ngIf="barcodeSearchResults.length === 0">
                        <mat-card-header>
                          <mat-card-title>Create New Product</mat-card-title>
                          <mat-card-subtitle>No products found with barcode: {{ lastScannedBarcode }}</mat-card-subtitle>
                        </mat-card-header>
                        
                        <mat-card-content>
                          <p>Would you like to create a new product with this barcode?</p>
                        </mat-card-content>
                        
                        <mat-card-actions>
                          <button mat-raised-button color="primary" (click)="createProductWithBarcode()">
                            <mat-icon>add</mat-icon>
                            Create Product
                          </button>
                        </mat-card-actions>
                      </mat-card>
                    </div>
                  </div>
                </div>
              </div>
            </mat-tab>
          </mat-tab-group>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .enhanced-product-form {
      max-width: 1200px;
      margin: 0 auto;
      padding: 1rem;
    }
    
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
    }
    
    .page-header h1 {
      margin: 0;
    }
    
    .tab-content {
      padding: 1.5rem 0;
    }
    
    .description {
      color: #666;
      margin-bottom: 1.5rem;
    }
    
    .scanner-section {
      max-width: 800px;
      margin: 0 auto;
    }
    
    .barcode-results {
      margin-top: 2rem;
    }
    
    .result-cards {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 1rem;
      margin-top: 1rem;
    }
    
    .result-card {
      display: flex;
      flex-direction: column;
    }
    
    .result-card img {
      max-height: 200px;
      object-fit: contain;
      margin: 1rem 0;
    }
    
    .new-product-card {
      border: 2px dashed #3f51b5;
      background-color: rgba(63, 81, 181, 0.05);
    }
  `]
})
export class ProductFormComponent implements OnInit, OnDestroy {
  isEditMode = false;
  productId: string = '';
  selectedTab = 0;

  // Barcode scanning
  lastScannedBarcode = '';
  barcodeSearchResults: any[] = [];

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    // Check if we're in edit mode
    this.route.paramMap.pipe(
      takeUntil(this.destroy$)
    ).subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.isEditMode = true;
        this.productId = id;
      }
    });

    // Check if a tab is specified in the query params
    this.route.queryParamMap.pipe(
      takeUntil(this.destroy$)
    ).subscribe(params => {
      const tab = params.get('tab');
      if (tab) {
        switch (tab) {
          case 'bulk':
            this.selectedTab = 1;
            break;
          case 'scanner':
            this.selectedTab = 2;
            break;
          default:
            this.selectedTab = 0;
        }
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onTabChange(index: number): void {
    this.selectedTab = index;

    // Update query params
    let tabParam: string;
    switch (index) {
      case 1:
        tabParam = 'bulk';
        break;
      case 2:
        tabParam = 'scanner';
        break;
      default:
        tabParam = '';
    }

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: tabParam ? { tab: tabParam } : {},
      queryParamsHandling: 'merge'
    });
  }

  onBarcodeScanned(barcode: string): void {
    this.lastScannedBarcode = barcode;

    // In a real app, we would search for products with this barcode
    // For now, we'll simulate a search with a random outcome
    setTimeout(() => {
      if (Math.random() > 0.7) {
        // Simulate finding a product
        this.barcodeSearchResults = [{
          id: 'sim-' + Math.random().toString(36).substring(2),
          name: 'Product ' + barcode.substring(0, 4),
          sku: 'SKU-' + barcode.substring(0, 6),
          barcode: barcode,
          price: 19.99 + Math.random() * 20,
          currency: 'USD',
          stock: Math.floor(Math.random() * 100),
          category: { name: 'Category ' + Math.floor(Math.random() * 5) },
          images: [
            { url: 'https://via.placeholder.com/150' }
          ]
        }];
      } else {
        // No products found
        this.barcodeSearchResults = [];
      }
    }, 1000);
  }

  createProductWithBarcode(): void {
    // In a real app, we would navigate to the product form with the barcode pre-filled
    this.selectedTab = 0;

    this.snackBar.open(
      `Ready to create product with barcode: ${this.lastScannedBarcode}`,
      'Dismiss',
      { duration: 3000 }
    );

    // Update URL without reloading
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: '', barcode: this.lastScannedBarcode },
      queryParamsHandling: 'merge'
    });
  }
}