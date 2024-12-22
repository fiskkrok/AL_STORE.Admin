import { Component, Output, EventEmitter, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CdkDragDrop, DragDropModule } from '@angular/cdk/drag-drop';
import { Category } from 'src/app/shared/models/categories/category.model';

@Component({
  selector: 'app-category-tree-node',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    DragDropModule
  ],
  template: `
    <div class="category-node-list" cdkDropList (cdkDropListDropped)="onDrop($event)">
      @for (category of categories(); track category.id) {
        <div class="category-node" cdkDrag>
          <div class="category-content">
            <div class="category-info">
              <mat-icon cdkDragHandle>drag_indicator</mat-icon>
              <span class="category-name">{{ category.name }}</span>
              <span class="product-count">({{ category.productCount }} products)</span>
            </div>
            <div class="category-actions">
              <button mat-icon-button (click)="onEdit(category)">
                <mat-icon>edit</mat-icon>
              </button>
              <button mat-icon-button color="warn" (click)="onDelete(category)">
                <mat-icon>delete</mat-icon>
              </button>
            </div>
          </div>
          @if (category.subCategories.length) {
            <div class="subcategories-container" [style.margin-left.px]="20">
              <app-category-tree-node
                [categories]="category.subCategories"
                (edit)="onEdit($event)"
                (delete)="onDelete($event)"
                (reorder)="onDrop($event)">
              </app-category-tree-node>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .category-node-list {
      min-height: 40px;
    }

    .category-node {
      margin-bottom: 0.5rem;

      &:last-child {
        margin-bottom: 0;
      }
    }

    .category-content {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.5rem;
      background-color: var(--bg-primary);
      border-radius: 4px;
      border: 1px solid var(--border);

      &:hover {
        background-color: var(--bg-hover);
      }
    }

    .category-info {
      display: flex;
      align-items: center;
      gap: 0.5rem;

      .category-name {
        font-weight: 500;
        color: var(--text-primary);
      }

      .product-count {
        color: var(--text-secondary);
        font-size: 0.875rem;
      }
    }

    .category-actions {
      display: flex;
      gap: 0.25rem;
    }

    .subcategories-container {
      margin-top: 0.5rem;
    }
  `]
})
export class CategoryTreeNodeComponent {
  readonly categories = input<Category[]>([]);
  @Output() edit = new EventEmitter<Category>();
  @Output() delete = new EventEmitter<Category>();
  @Output() reorder = new EventEmitter<CdkDragDrop<Category[]>>();

  onEdit(category: Category) {
    this.edit.emit(category);
  }

  onDelete(category: Category) {
    this.delete.emit(category);
  }

  onDrop(event: CdkDragDrop<Category[]>) {
    this.reorder.emit(event);
  }


}