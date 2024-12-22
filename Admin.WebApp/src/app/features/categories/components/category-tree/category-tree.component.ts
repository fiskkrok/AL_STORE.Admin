import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { MatTreeModule } from '@angular/material/tree';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { CategoryFormDialogComponent } from '../category-form-dialog/category-form-dialog.component';
import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { Category } from 'src/app/shared/models/categories/category.model';
import { CategoryActions } from '../../../../store/category/category.actions';
import { selectCategoryHierarchy, selectCategoriesLoading, selectCategoriesError } from '../../../../store/category/category.selectors';
import { DialogService } from '../../../../core/services/dialog.service';
import { CategoryTreeNodeComponent } from "./category-tree-node.component";

@Component({
  selector: 'app-category-tree',
  standalone: true,
  imports: [
    CommonModule,
    MatTreeModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    DragDropModule,
    CategoryTreeNodeComponent
  ],
  template: `
    <div class="category-tree-container">
      <div class="tree-header">
        <h2>Categories</h2>
        <button mat-raised-button color="primary" (click)="addCategory()">
          <mat-icon>add</mat-icon>
          Add Category
        </button>
      </div>

      @if (loading$ | async) {
        <div class="loading-container">
          <mat-spinner diameter="40"></mat-spinner>
        </div>
      }

      @if (error$ | async; as error) {
        <div class="error-message">
          {{ error }}
        </div>
      }

      @if (categories$ | async; as categories) {
        <div class="tree-content" cdkDropList (cdkDropListDropped)="drop($event)">
          @for (category of categories; track category.id) {
            <div class="category-node" cdkDrag>
              <div class="category-content">
                <div class="category-info">
                  <mat-icon cdkDragHandle>drag_indicator</mat-icon>
                  <span class="category-name">{{ category.name }}</span>
                  <span class="product-count">({{ category.productCount }} products)</span>
                </div>
                <div class="category-actions">
                  <button mat-icon-button (click)="editCategory(category)">
                    <mat-icon>edit</mat-icon>
                  </button>
                  <button mat-icon-button color="warn" (click)="deleteCategory(category)">
                    <mat-icon>delete</mat-icon>
                  </button>
                </div>
              </div>
              @if (category.subCategories.length) {
                <div class="subcategories-container" [style.margin-left.px]="20">
                  <app-category-tree-node 
                    [categories]="category.subCategories"
                    (edit)="editCategory($event)"
                    (delete)="deleteCategory($event)"
                    (reorder)="reorderSubcategories($event, category.id)">
                  </app-category-tree-node>
                </div>
              }
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .category-tree-container {
      padding: 1rem;
    }

    .tree-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;

      h2 {
        margin: 0;
        color: var(--text-primary);
      }
    }

    .loading-container {
      display: flex;
      justify-content: center;
      padding: 2rem;
    }

    .error-message {
      color: var(--error);
      padding: 1rem;
      margin: 1rem 0;
      background-color: var(--error-bg);
      border-radius: 4px;
    }

    .tree-content {
      background-color: var(--bg-secondary);
      border-radius: 8px;
      padding: 1rem;
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

    // Drag and drop styles
    .cdk-drag-preview {
      box-sizing: border-box;
      border-radius: 4px;
      box-shadow: 0 5px 5px -3px rgba(0, 0, 0, 0.2),
                  0 8px 10px 1px rgba(0, 0, 0, 0.14),
                  0 3px 14px 2px rgba(0, 0, 0, 0.12);
    }

    .cdk-drag-placeholder {
      opacity: 0;
    }

    .cdk-drag-animating {
      transition: transform 250ms cubic-bezier(0, 0, 0.2, 1);
    }
  `]
})
export class CategoryTreeComponent implements OnInit {
  categories$: Observable<Category[]>;
  loading$: Observable<boolean>;
  error$: Observable<string | null>;

  constructor(
    private readonly store: Store,
    private readonly dialogService: DialogService,
    private readonly dialog: MatDialog
  ) {
    this.categories$ = this.store.select(selectCategoryHierarchy);
    this.loading$ = this.store.select(selectCategoriesLoading);
    this.error$ = this.store.select(selectCategoriesError);
  }

  ngOnInit() {
    this.loadCategories();
  }

  loadCategories() {
    this.store.dispatch(CategoryActions.loadCategories());
  }

  async addCategory(parentCategoryId?: string) {
    const dialogRef = this.dialog.open(CategoryFormDialogComponent, {
      width: '600px',
      data: { parentCategoryId }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.store.dispatch(CategoryActions.createCategory({
          request: {
            name: result.name,
            description: result.description,
            parentCategoryId: result.parentCategoryId,
            metaTitle: result.metaTitle,
            metaDescription: result.metaDescription,
            imageUrl: result.file ? URL.createObjectURL(result.file) : undefined
          }
        }));
      }
    });
  }

  async editCategory(category: Category) {
    const dialogRef = this.dialog.open(CategoryFormDialogComponent, {
      width: '600px',
      data: { category }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.store.dispatch(CategoryActions.updateCategory({
          id: category.id,
          request: {
            name: result.name,
            description: result.description,
            parentCategoryId: result.parentCategoryId,
            metaTitle: result.metaTitle,
            metaDescription: result.metaDescription,
            imageUrl: result.file ? URL.createObjectURL(result.file) : undefined
          }
        }));
      }
    });
  }

  async deleteCategory(category: Category) {
    const confirmed = await this.dialogService.confirm(
      `Are you sure you want to delete ${category.name}? This action cannot be undone.`,
      'Delete Category'
    );

    if (confirmed) {
      this.store.dispatch(CategoryActions.deleteCategory({ id: category.id }));
    }
  }

  drop(event: CdkDragDrop<Category[]>) {
    if (event.previousIndex === event.currentIndex) return;

    const categories = [...(event.container.data || [])];
    moveItemInArray(categories, event.previousIndex, event.currentIndex);

    const reorderRequests = categories.map((category, index) => ({
      categoryId: category.id,
      newSortOrder: index
    }));

    this.store.dispatch(CategoryActions.reorderCategories({ requests: reorderRequests }));
  }

  reorderSubcategories(event: CdkDragDrop<Category[]>, parentId: string) {
    if (event.previousIndex === event.currentIndex) return;

    const categories = [...(event.container.data || [])];
    moveItemInArray(categories, event.previousIndex, event.currentIndex);

    const reorderRequests = categories.map((category, index) => ({
      categoryId: category.id,
      newSortOrder: index
    }));

    this.store.dispatch(CategoryActions.reorderCategories({ requests: reorderRequests }));
  }
}