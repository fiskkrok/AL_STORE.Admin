// src/app/features/products/components/barcode-scanner/barcode-scanner.component.ts
import { Component, OnInit, OnDestroy, ViewChild, ElementRef, Output, EventEmitter, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

// ZXing imports
import {
  BrowserMultiFormatReader,
  BarcodeFormat,
  DecodeHintType,
  Result,
  NotFoundException
} from '@zxing/library';

@Component({
  selector: 'app-barcode-scanner',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule
  ],
  template: `
    <div class="barcode-scanner">
      <button 
        mat-raised-button 
        color="primary" 
        (click)="openScanner()">
        <mat-icon>qr_code_scanner</mat-icon>
        Scan Barcode
      </button>
      
      <div class="scanner-container" *ngIf="isScanning">
        <div class="scanner-view">
          <video #videoElement></video>
          <div class="scanner-overlay">
            <div class="scanner-target"></div>
          </div>
          <div class="scanner-actions">
            <button 
              mat-mini-fab 
              color="accent" 
              (click)="toggleFlashlight()">
              <mat-icon>flashlight_on</mat-icon>
            </button>
            <button 
              mat-mini-fab 
              color="warn" 
              (click)="stopScanner()">
              <mat-icon>close</mat-icon>
            </button>
          </div>
        </div>
        
        <div class="scanner-instructions">
          <p>Position barcode within the box</p>
        </div>
      </div>
      
      <ng-container *ngIf="lastScannedCode">
        <div class="scanned-result">
          <div class="result-header">
            <h4>Scanned Code:</h4>
            <span class="result-code">{{ lastScannedCode }}</span>
          </div>
          <p class="result-type" *ngIf="lastScannedType">Type: {{ lastScannedType }}</p>
          <div class="result-actions">
            <button 
              mat-button 
              color="primary" 
              (click)="useScannedCode()">
              Use Code
            </button>
            <button 
              mat-button 
              (click)="clearLastScan()">
              Clear
            </button>
          </div>
        </div>
      </ng-container>
    </div>
  `,
  styles: [`
    .barcode-scanner {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    
    .scanner-container {
      position: relative;
      border-radius: 8px;
      overflow: hidden;
      box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
      margin-top: 1rem;
    }
    
    .scanner-view {
      position: relative;
      width: 100%;
      aspect-ratio: 4/3;
      background-color: #000;
    }
    
    video {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
    
    .scanner-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
    }
    
    .scanner-target {
      width: 70%;
      height: 30%;
      border: 2px solid #3f51b5;
      border-radius: 8px;
      box-shadow: 0 0 0 2000px rgba(0, 0, 0, 0.3);
      position: relative;
    }
    
    .scanner-target::before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 2px;
      background-color: rgba(255, 255, 255, 0.7);
      animation: scan 2s linear infinite;
    }
    
    @keyframes scan {
      0% { top: 0; }
      50% { top: 100%; }
      100% { top: 0; }
    }
    
    .scanner-actions {
      position: absolute;
      bottom: 1rem;
      right: 1rem;
      display: flex;
      gap: 0.5rem;
    }
    
    .scanner-instructions {
      padding: 0.5rem;
      text-align: center;
      background-color: #f5f5f5;
      font-size: 0.875rem;
    }
    
    .scanned-result {
      background-color: #f5f5f5;
      border-radius: 8px;
      padding: 1rem;
      margin-top: 1rem;
    }
    
    .result-header {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-bottom: 0.5rem;
    }
    
    .result-header h4 {
      margin: 0;
    }
    
    .result-code {
      font-family: monospace;
      font-weight: bold;
    }
    
    .result-type {
      color: #666;
      font-size: 0.875rem;
      margin-bottom: 1rem;
    }
    
    .result-actions {
      display: flex;
      gap: 0.5rem;
    }
  `]
})
export class BarcodeScannerComponent implements OnInit, OnDestroy {
  @ViewChild('videoElement') videoElement!: ElementRef<HTMLVideoElement>;
  readonly acceptedFormats = input<BarcodeFormat[]>([
    BarcodeFormat.UPC_A,
    BarcodeFormat.UPC_E,
    BarcodeFormat.EAN_8,
    BarcodeFormat.EAN_13,
    BarcodeFormat.CODE_39,
    BarcodeFormat.CODE_128
  ]);

  @Output() codeScanned = new EventEmitter<string>();

  isScanning = false;
  lastScannedCode: string | null = null;
  lastScannedType: string | null = null;

  private codeReader: BrowserMultiFormatReader | null = null;
  private destroy$ = new Subject<void>();
  private hasFlashlight = false;
  private isFlashlightOn = false;
  private mediaStream: MediaStream | null = null;

  constructor(private dialog: MatDialog) { }

  ngOnInit(): void {
    // Initialize barcode reader
    this.codeReader = new BrowserMultiFormatReader();

    // Set hints/formats
    const hints = new Map();
    hints.set(2, this.acceptedFormats());
    this.codeReader.hints = hints;
  }

  ngOnDestroy(): void {
    this.stopScanner();
    this.destroy$.next();
    this.destroy$.complete();

    if (this.codeReader) {
      this.codeReader.reset();
    }
  }

  openScanner(): void {
    if (this.isScanning) return;

    this.isScanning = true;

    // Request camera permissions and start scanning
    setTimeout(() => {
      this.startScanner();
    }, 100);
  }

  private startScanner(): void {
    if (!this.codeReader || !this.videoElement) return;

    const videoElement = this.videoElement.nativeElement;

    // Try to use the back camera if available
    navigator.mediaDevices.getUserMedia({
      video: { facingMode: 'environment' }
    }).then(stream => {
      this.mediaStream = stream;

      // Check if flashlight is available
      const track = stream.getVideoTracks()[0];
      const capabilities = track.getCapabilities();
      this.hasFlashlight = !!('torch' in capabilities);

      // Start decoding from video
      this.codeReader!.decodeFromStream(stream, videoElement, (result, error) => {
        if (result) {
          this.handleScanResult(result);
        }

        if (error && !(error instanceof NotFoundException)) {
          console.error('Scan error:', error);
        }
      });
    }).catch(err => {
      console.error('Camera access error:', err);
      this.isScanning = false;
    });
  }

  stopScanner(): void {
    if (this.codeReader) {
      this.codeReader.reset();
    }

    if (this.mediaStream) {
      this.mediaStream.getTracks().forEach(track => track.stop());
      this.mediaStream = null;
    }

    this.isScanning = false;
    this.isFlashlightOn = false;
  }

  toggleFlashlight(): void {
    if (!this.mediaStream || !this.hasFlashlight) return;

    const track = this.mediaStream.getVideoTracks()[0];
    this.isFlashlightOn = !this.isFlashlightOn;

    track.applyConstraints({
      advanced: [{ torch: this.isFlashlightOn } as any]
    }).catch(err => {
      console.error('Flashlight error:', err);
    });
  }

  private handleScanResult(result: Result): void {
    const code = result.getText();
    const format = result.getBarcodeFormat();

    // Map format enum to readable string
    let formatName = '';
    switch (format) {
      case BarcodeFormat.UPC_A: formatName = 'UPC-A'; break;
      case BarcodeFormat.UPC_E: formatName = 'UPC-E'; break;
      case BarcodeFormat.EAN_8: formatName = 'EAN-8'; break;
      case BarcodeFormat.EAN_13: formatName = 'EAN-13'; break;
      case BarcodeFormat.CODE_39: formatName = 'Code 39'; break;
      case BarcodeFormat.CODE_128: formatName = 'Code 128'; break;
      default: formatName = 'Unknown';
    }

    this.lastScannedCode = code;
    this.lastScannedType = formatName;

    // Stop scanning after successful scan
    this.stopScanner();
  }

  useScannedCode(): void {
    if (this.lastScannedCode) {
      this.codeScanned.emit(this.lastScannedCode);
      this.clearLastScan();
    }
  }

  clearLastScan(): void {
    this.lastScannedCode = null;
    this.lastScannedType = null;
  }
}