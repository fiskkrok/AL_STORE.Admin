// src/app/features/products/components/product-image-manager/product-image-manager.component.ts
import { Component, Output, EventEmitter, OnDestroy, inject, input, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { DragDropModule, CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { of, Subject } from 'rxjs';
import { takeUntil, finalize, catchError } from 'rxjs/operators';
import { ErrorService } from 'src/app/core/services/error.service';
import { ProductService } from 'src/app/core/services/product.service';
import { ProductImage } from 'src/app/shared/models/product.model';
@Component({
  selector: 'app-product-image-manager',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatTooltipModule,
    DragDropModule
  ],
  template: `
    <div class="image-manager-container">
      <h3>Product Images</h3>
      
      <!-- Image upload area -->
      <div 
        class="upload-area" 
        [class.dragover]="isDragging"
        (dragover)="onDragOver($event)"
        (dragleave)="onDragLeave($event)"
        (drop)="onDrop($event)">
        
        <div class="upload-prompt">
          <mat-icon>cloud_upload</mat-icon>
          <p>Drag & drop images here or</p>
          <button 
            mat-stroked-button 
            color="primary" 
            (click)="fileInput.click()">
            Browse Files
          </button>
          <input 
            #fileInput 
            type="file" 
            multiple 
            accept="image/*" 
            style="display: none"
            (change)="onFileSelected($event)">
        </div>
        
        <div class="upload-info" *ngIf="!isUploading">
          <p>Maximum 10 images. Supported formats: JPG, PNG, WEBP</p>
          <p>Maximum file size: 5MB</p>
        </div>
        
        <div class="upload-progress" *ngIf="isUploading">
          <mat-icon class="spin">sync</mat-icon>
          <p>Uploading {{ uploadProgress }}%</p>
        </div>
      </div>
      
      <!-- Image preview and management area -->
      <div 
        cdkDropList 
        class="image-preview-grid" 
        (cdkDropListDropped)="drop($event)"
        *ngIf="images.length > 0">
        
        <div 
          *ngFor="let image of images; let i = index" 
          class="image-preview" 
          cdkDrag
          [class.primary]="i === 0">
          
          <div class="image-wrapper">
            <img [src]="image.url" [alt]="image.fileName">
            
            <!-- Primary badge -->
            <div class="primary-indicator" *ngIf="i === 0">
              <mat-icon>star</mat-icon>
              <span>Primary</span>
            </div>
            
            <!-- Image actions -->
            <div class="image-actions">
              <button 
                mat-icon-button 
                [matMenuTriggerFor]="menu"
                color="primary">
                <mat-icon>more_vert</mat-icon>
              </button>
              
              <mat-menu #menu="matMenu">
                <button 
                  mat-menu-item 
                  *ngIf="i !== 0"
                  (click)="setAsPrimary(i)">
                  <mat-icon>star</mat-icon>
                  <span>Set as Primary</span>
                </button>
                <button 
                  mat-menu-item
                  (click)="editImageDetails(image, i)">
                  <mat-icon>edit</mat-icon>
                  <span>Edit Details</span>
                </button>
                <button 
                  mat-menu-item
                  (click)="removeImage(i)">
                  <mat-icon>delete</mat-icon>
                  <span>Remove</span>
                </button>
              </mat-menu>
            </div>
          </div>
          
          <div class="image-info">
            <span class="file-name">{{ image.fileName }}</span>
            <span class="file-size">{{ formatFileSize(image.size) }}</span>
          </div>
        </div>
      </div>
      
      <div class="help-text" *ngIf="images.length > 0">
        <mat-icon>info</mat-icon>
        <span>Drag images to reorder. The first image will be used as the primary product image.</span>
      </div>
    </div>
  `,
  styles: [`
    .image-manager-container {
      display: flex;
      flex-direction: column;
      gap: 1rem;
      margin-bottom: 1.5rem;
    }
    
    .upload-area {
      border: 2px dashed #ccc;
      border-radius: 8px;
      padding: 2rem;
      text-align: center;
      transition: all 0.2s ease;
      cursor: pointer;
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
      margin-bottom: 1rem;
    }
    
    .upload-prompt mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: #3f51b5;
    }
    
    .upload-info p {
      margin: 0.25rem 0;
      color: #666;
      font-size: 0.875rem;
    }
    
    .upload-progress {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
    }
    
    .spin {
      animation: spin 1.5s linear infinite;
    }
    
    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
    
    .image-preview-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
      gap: 1rem;
      margin-top: 1rem;
    }
    
    .image-preview {
      border-radius: 8px;
      overflow: hidden;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      transition: all 0.2s ease;
      background-color: white;
    }
    
    .image-preview.primary {
      border: 2px solid #3f51b5;
    }
    
    .image-wrapper {
      position: relative;
      aspect-ratio: 1;
    }
    
    .image-wrapper img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
    
    .primary-indicator {
      position: absolute;
      top: 0.5rem;
      left: 0.5rem;
      background-color: #3f51b5;
      color: white;
      font-size: 0.75rem;
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
      display: flex;
      align-items: center;
      gap: 0.25rem;
    }
    
    .primary-indicator mat-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
    }
    
    .image-actions {
      position: absolute;
      top: 0.5rem;
      right: 0.5rem;
      opacity: 0;
      transition: opacity 0.2s ease;
    }
    
    .image-wrapper:hover .image-actions {
      opacity: 1;
    }
    
    .image-info {
      padding: 0.5rem;
      display: flex;
      flex-direction: column;
      font-size: 0.75rem;
    }
    
    .file-name {
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    
    .file-size {
      color: #666;
    }
    
    .help-text {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.875rem;
      color: #666;
      margin-top: 0.5rem;
    }
    
    .cdk-drag-preview {
      box-shadow: 0 5px 15px rgba(0,0,0,0.2);
    }
    
    .cdk-drag-placeholder {
      opacity: 0.3;
    }
    
    .cdk-drag-animating {
      transition: transform 250ms cubic-bezier(0, 0, 0.2, 1);
    }
  `]
})
export class ProductImageManagerComponent implements OnDestroy {
  private readonly productService = inject(ProductService)
  private readonly errorService = inject(ErrorService)
  @Input() images: ProductImage[] = [];
  @Output() imagesChange = new EventEmitter<ProductImage[]>();

  isDragging = false;
  isUploading = false;
  uploadProgress = 0;

  private readonly destroy$ = new Subject<void>();

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
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

    if (event.dataTransfer?.files) {
      this.handleFiles(event.dataTransfer.files);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.handleFiles(input.files);
    }
  }

  handleFiles(files: FileList): void {
    // Filter for images and check size limits
    const validFiles: File[] = [];
    const maxSize = 5 * 1024 * 1024; // 5MB
    const maxImages = 10;
    const availableSlots = maxImages - this.images.length;

    if (availableSlots <= 0) {
      this.errorService.addError({
        code: 'IMAGES_LIMIT_EXCEEDED',
        message: 'Maximum of 10 images allowed per product',
        severity: 'warning'
      });
      return;
    }

    // Validate files
    for (let i = 0; i < Math.min(files.length, availableSlots); i++) {
      const file = files[i];

      // Check mime type
      if (!file.type.startsWith('image/')) {
        this.errorService.addError({
          code: 'INVALID_FILE_TYPE',
          message: `${file.name} is not a valid image file`,
          severity: 'warning'
        });
        continue;
      }

      // Check file size
      if (file.size > maxSize) {
        this.errorService.addError({
          code: 'FILE_TOO_LARGE',
          message: `${file.name} exceeds the 5MB size limit`,
          severity: 'warning'
        });
        continue;
      }

      validFiles.push(file);
    }

    if (validFiles.length === 0) return;

    // Upload valid files
    this.isUploading = true;
    this.uploadProgress = 0;

    // Simulate progress updates
    const progressInterval = setInterval(() => {
      if (this.uploadProgress < 90) {
        this.uploadProgress += 10;
      }
    }, 300);

    this.productService.uploadImages(validFiles)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          clearInterval(progressInterval);
          this.isUploading = false;
          this.uploadProgress = 0;
        }),
        catchError(() => {
          this.errorService.addError({
            code: 'UPLOAD_FAILED',
            message: 'Failed to upload images. Please try again.',
            severity: 'error'
          });
          return of([]);
        })
      )
      .subscribe(uploadedImages => {
        if (uploadedImages.length > 0) {
          const updatedImages = [...this.images, ...uploadedImages];
          this.images = updatedImages;
          this.imagesChange.emit(updatedImages);
        }
      });
  }

  drop(event: CdkDragDrop<ProductImage[]>): void {
    const images = this.images;
    moveItemInArray(images, event.previousIndex, event.currentIndex);
    this.imagesChange.emit([...images]);
  }

  setAsPrimary(index: number): void {
    if (index === 0 || index >= this.images.length) return;

    // Move the selected image to the front
    const image = this.images[index];
    this.images.splice(index, 1);
    this.images.unshift(image);

    this.imagesChange.emit([...this.images]);
  }

  editImageDetails(image: ProductImage, index: number): void {
    // This would typically open a dialog to edit alt text and other metadata
    // For now, we'll just update a placeholder value
    const updatedImage = {
      ...image,
      alt: prompt('Enter alt text for this image:', image.alt) || image.alt
    };

    const images = this.images;
    images[index] = updatedImage;
    this.imagesChange.emit([...images]);
  }

  removeImage(index: number): void {
    if (index < 0 || index >= this.images.length) return;

    this.images.splice(index, 1);
    this.imagesChange.emit([...this.images]);
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }
}