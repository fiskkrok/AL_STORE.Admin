// Example Tailwind-based Component Template
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormBuilder, Validators } from '@angular/forms';

// Material imports (include only what you need)
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';

/**
 * Example component template using Tailwind CSS
 * Use this as a starting point for new components or refactoring
 */
@Component({
  selector: 'app-example-component',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule
  ],
  template: `
    <!-- Page/Section Container -->
    <div class="p-6 max-w-screen-lg mx-auto">
      <!-- Component Header -->
      <div class="mb-6">
        <h2 class="text-xl font-semibold text-slate-900 dark:text-white">{{ title }}</h2>
        <p class="text-sm text-slate-500 dark:text-slate-400">{{ description }}</p>
      </div>
      
      <!-- Content Card -->
      <div class="bg-white dark:bg-slate-800 rounded-lg shadow-subtle overflow-hidden">
        <!-- Card Header (if needed) -->
        <div class="px-6 py-4 border-b border-slate-200 dark:border-slate-700 flex justify-between items-center">
          <h3 class="font-medium text-slate-800 dark:text-slate-200">Card Header</h3>
          <button 
            mat-icon-button 
            class="text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-full transition-colors">
            <mat-icon class="material-icons">more_vert</mat-icon>
          </button>
        </div>
        
        <!-- Card Content -->
        <div class="p-6">
          <!-- Form Example -->
          <form [formGroup]="form" (ngSubmit)="onSubmit()" *ngIf="showForm">
            <div class="space-y-4">
              <!-- Text Input -->
              <mat-form-field appearance="outline" class="w-full">
                <mat-label>Example Field</mat-label>
                <input matInput formControlName="exampleField">
                <mat-error *ngIf="form.get('exampleField')?.hasError('required')">
                  This field is required
                </mat-error>
              </mat-form-field>
              
              <!-- Form Actions -->
              <div class="flex justify-end space-x-3 pt-4">
                <button 
                  mat-stroked-button 
                  type="button"
                  class="border border-slate-300 dark:border-slate-600 px-4 py-1 rounded-md text-slate-700 dark:text-slate-200 hover:bg-slate-50 dark:hover:bg-slate-700"
                  (click)="onCancel()">
                  Cancel
                </button>
                <button 
                  mat-raised-button 
                  color="primary" 
                  type="submit"
                  class="bg-primary-600 text-white px-4 py-1 rounded-md hover:bg-primary-700 transition-colors"
                  [disabled]="form.invalid">
                  Submit
                </button>
              </div>
            </div>
          </form>
          
          <!-- List Example -->
          <div class="flow-root" *ngIf="showList">
            <ul class="divide-y divide-slate-200 dark:divide-slate-700">
              <li *ngFor="let item of items; trackBy: trackById" class="py-4">
                <div class="flex items-center space-x-4">
                  <!-- Item Icon/Image -->
                  <div class="flex-shrink-0">
                    <div class="p-2 bg-primary-100 dark:bg-primary-900 rounded-full">
                      <mat-icon class="material-icons text-primary-600 dark:text-primary-400">{{ item.icon }}</mat-icon>
                    </div>
                  </div>
                  
                  <!-- Item Content -->
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-medium text-slate-900 dark:text-white truncate">
                      {{ item.title }}
                    </p>
                    <p class="text-sm text-slate-500 dark:text-slate-400">
                      {{ item.description }}
                    </p>
                  </div>
                  
                  <!-- Item Actions -->
                  <div class="flex-shrink-0">
                    <button 
                      mat-icon-button 
                      class="text-slate-500 hover:text-primary-600 dark:text-slate-400 dark:hover:text-primary-400"
                      (click)="onItemAction(item)">
                      <mat-icon class="material-icons">chevron_right</mat-icon>
                    </button>
                  </div>
                </div>
              </li>
            </ul>
          </div>
          
          <!-- Empty State Example -->
          <div *ngIf="items.length === 0 && showList" class="flex flex-col items-center justify-center py-12 text-center">
            <div class="p-3 bg-slate-100 dark:bg-slate-700 rounded-full mb-4">
              <mat-icon class="text-3xl text-slate-400 dark:text-slate-500">inbox</mat-icon>
            </div>
            <h3 class="text-lg font-medium text-slate-900 dark:text-white mb-2">No items found</h3>
            <p class="text-sm text-slate-500 dark:text-slate-400 max-w-md mb-6">
              There are no items to display. Try adding a new item or changing your filters.
            </p>
            <button 
              mat-flat-button 
              color="primary"
              class="bg-primary-600 text-white px-4 py-2 rounded-md hover:bg-primary-700 transition-colors">
              Add New Item
            </button>
          </div>
          
          <!-- Loading State -->
          <div *ngIf="loading" class="flex justify-center items-center py-12">
            <div class="inline-block animate-spin rounded-full h-8 w-8 border-2 border-current border-t-transparent text-primary-500"></div>
          </div>
        </div>
        
        <!-- Card Footer (if needed) -->
        <div class="px-6 py-4 bg-slate-50 dark:bg-slate-750 border-t border-slate-200 dark:border-slate-700">
          <div class="flex justify-between items-center">
            <span class="text-sm text-slate-500 dark:text-slate-400">Showing {{ items.length }} items</span>
            <button 
              mat-button 
              color="primary"
              class="text-primary-600 hover:text-primary-700 dark:text-primary-400 dark:hover:text-primary-300">
              View All
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class ExampleComponent {
  @Input() title = 'Component Title';
  @Input() description = 'Component description goes here';
  @Input() showForm = true;
  @Input() showList = true;
  @Input() loading = false;
  @Input() items: any[] = [];

  @Output() formSubmit = new EventEmitter<any>();
  @Output() cancelClick = new EventEmitter<void>();
  @Output() itemClick = new EventEmitter<any>();

  form: FormGroup;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      exampleField: ['', Validators.required],
    });
  }

  onSubmit() {
    if (this.form.valid) {
      this.formSubmit.emit(this.form.value);
    }
  }

  onCancel() {
    this.cancelClick.emit();
  }

  onItemAction(item: any) {
    this.itemClick.emit(item);
  }

  trackById(index: number, item: any): any {
    return item.id || index;
  }
}