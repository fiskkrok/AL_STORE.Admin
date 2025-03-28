import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BarcodeScannerComponent } from '../components/barcode-scanner';

@Component({
  selector: 'app-add-product',
  standalone: true,
  imports: [
    CommonModule,
    BarcodeScannerComponent
  ],
  template: `
    <div class="add-product-container">
      <h2>Add New Product</h2>
      
      <!-- Barcode scanner integration -->
      <div class="scanner-section">
        <h3>Scan Product Barcode</h3>
        <app-barcode-scanner 
          (codeScanned)="onBarcodeScanned($event)">
        </app-barcode-scanner>
      </div>
      
      <!-- Rest of your add product form -->
      <!-- ...existing code... -->
    </div>
  `,
  styleUrls: ['./add-product.component.scss']
})
export class AddProductComponent {
  // ...existing code...

  onBarcodeScanned(code: string): void {
    console.log('Product barcode scanned:', code);
    // Here you can:
    // 1. Look up the product by barcode in your database
    // 2. Auto-fill form fields based on the scanned product
    // 3. Add the barcode to your form model
  }

  // ...existing code...
}
