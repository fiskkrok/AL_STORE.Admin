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
  template: `
    <div class="container mx-auto p-4 md:p-6">
      <!-- Page Header -->
      <div class="flex flex-col md:flex-row justify-between items-start md:items-center mb-6">
        <div>
          <h1 class="text-2xl font-bold text-slate-900 dark:text-white">Categories</h1>
          <p class="text-sm text-slate-500 dark:text-slate-400">Organize your products with categories</p>
        </div>
        
        <button 
          (click)="addCategory()"
          class="mt-4 md:mt-0 px-4 py-2 bg-primary-600 text-white rounded-md shadow-sm flex items-center hover:bg-primary-700 transition-colors">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-1.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Add Category
        </button>
      </div>
      
      <!-- Loading State -->
      <div *ngIf="loading$ | async" class="flex justify-center items-center py-12">
        <div class="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-primary-400 border-r-transparent dark:border-primary-600 dark:border-r-transparent"></div>
      </div>
      
      <!-- Error State -->
      <div *ngIf="error$ | async as error" class="bg-rose-50 dark:bg-slate-800 border border-rose-200 dark:border-rose-900 rounded-lg p-4 mb-6">
        <div class="flex items-start">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-rose-500 dark:text-rose-500 mt-0.5 mr-3" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
          </svg>
          <div>
            <p class="text-sm font-medium text-rose-800 dark:text-rose-200">{{ error }}</p>
            <p class="mt-1 text-sm text-rose-700 dark:text-rose-300">Please try again or contact support if the issue persists.</p>
          </div>
        </div>
      </div>
      
      <!-- Category Tree -->
      <div *ngIf="(categories$ | async)?.length === 0 && !(loading$ | async)" class="bg-white dark:bg-slate-800 rounded-lg shadow-sm p-12 text-center border border-slate-200 dark:border-slate-700">
        <div class="inline-flex items-center justify-center w-16 h-16 rounded-full bg-slate-100 dark:bg-slate-700 mb-4">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 text-slate-400 dark:text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
          </svg>
        </div>
        <h3 class="text-lg font-medium text-slate-900 dark:text-white mb-2">No Categories Found</h3>
        <p class="text-slate-500 dark:text-slate-400 mb-6 max-w-md mx-auto">Categories help you organize your products. Create your first category to get started.</p>
        <button 
          (click)="addCategory()"
          class="px-4 py-2 bg-primary-600 text-white rounded-md shadow-sm flex items-center hover:bg-primary-700 transition-colors mx-auto">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-1.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Create First Category
        </button>
      </div>
      
      <div *ngIf="(categories$ | async)?.length && !(loading$ | async)" class="bg-white dark:bg-slate-800 rounded-lg shadow-sm border border-slate-200 dark:border-slate-700">
        <div class="border-b border-slate-200 dark:border-slate-700 px-6 py-4">
          <h2 class="font-medium text-slate-900 dark:text-white">Categories</h2>
        </div>
        
        <div class="p-6" cdkDropList (cdkDropListDropped)="drop($event)">
          <div *ngFor="let category of categories$ | async; track category?.id" class="mb-4 last:mb-0">
            <div class="bg-slate-50 dark:bg-slate-750 rounded-lg border border-slate-200 dark:border-slate-700">
              <!-- Category Header -->
              <div class="flex items-center justify-between p-4 cursor-move" cdkDrag>
                <div class="flex items-center">
                  <div class="p-2 mr-2 text-slate-400 dark:text-slate-500 cursor-move" cdkDragHandle>
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 8h16M4 16h16" />
                    </svg>
                  </div>
                  <div>
                    <h3 class="font-medium text-slate-900 dark:text-white">{{ category.name }}</h3>
                    <p class="text-xs text-slate-500 dark:text-slate-400 mt-1">{{ category.productCount }} products</p>
                  </div>
                </div>
                
                <div class="flex items-center space-x-2">
                  <button 
                    (click)="editCategory(category)"
                    matTooltip="Edit Category"
                    class="p-2 text-slate-500 hover:text-primary-600 dark:text-slate-400 dark:hover:text-primary-400 rounded-full hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                  </button>
                  
                  <button 
                    (click)="deleteCategory(category)"
                    matTooltip="Delete Category"
                    class="p-2 text-slate-500 hover:text-rose-600 dark:text-slate-400 dark:hover:text-rose-400 rounded-full hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                  </button>
                </div>
              </div>
              
              <!-- Subcategories -->
              <div *ngIf="category.subCategories && category.subCategories.length > 0" class="px-4 py-3 border-t border-slate-200 dark:border-slate-700">
                <div class="pl-8">
                  <app-category-tree-node 
                    [categories]="category.subCategories"
                    (edit)="editCategory($event)"
                    (delete)="deleteCategory($event)"
                    (reorder)="reorderSubcategories($event, category.id)">
                  </app-category-tree-node>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
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