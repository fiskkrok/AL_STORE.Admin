// src/app/features/categories/components/category-tree/category-tree.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { MatTreeModule } from '@angular/material/tree';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CategoryFormDialogComponent } from '../category-form-dialog/category-form-dialog.component';
import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { Category } from 'src/app/shared/models/category.model';
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
    MatTooltipModule,
    DragDropModule,
    CategoryTreeNodeComponent
  ],
  templateUrl: './category-tree.component.html',
  styles: []
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

    this.categories$.subscribe(categories => {
      const reorderedCategories = [...categories];
      moveItemInArray(reorderedCategories, event.previousIndex, event.currentIndex);

      const reorderRequests = reorderedCategories.map((category, index) => ({
        categoryId: category.id,
        newSortOrder: index
      }));

      this.store.dispatch(CategoryActions.reorderCategories({ requests: reorderRequests }));
    }).unsubscribe();
  }

  reorderSubcategories(event: CdkDragDrop<Category[]>, parentId: string) {
    if (event.previousIndex === event.currentIndex) return;

    this.categories$.subscribe(categories => {
      const parentCategory = categories.find(c => c.id === parentId);
      if (!parentCategory || !parentCategory.subCategories) return;

      const reorderedSubcategories = [...parentCategory.subCategories];
      moveItemInArray(reorderedSubcategories, event.previousIndex, event.currentIndex);

      const reorderRequests = reorderedSubcategories.map((category, index) => ({
        categoryId: category.id,
        newSortOrder: index
      }));

      this.store.dispatch(CategoryActions.reorderCategories({ requests: reorderRequests }));
    }).unsubscribe();
  }
}