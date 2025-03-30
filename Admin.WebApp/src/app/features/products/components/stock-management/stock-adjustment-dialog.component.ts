// src/app/features/products/components/stock-management/stock-adjustment-dialog.component.ts
import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { Store } from '@ngrx/store';
import { StockActions } from '../../../../store/stock/stock.actions';

interface DialogData {
    productId: string;
    productName: string;
    currentStock: number;
}

@Component({
    selector: 'app-stock-adjustment-dialog',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatIconModule
    ],
    template: `
        <div class="bg-white dark:bg-slate-800 p-6 rounded-lg">
            <div class="flex items-center justify-between mb-6">
                <h2 class="text-xl font-semibold text-slate-900 dark:text-white">Adjust Stock</h2>
                <button 
                    mat-icon-button 
                    class="text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-full transition-colors"
                    [mat-dialog-close]="false">
                    <mat-icon>close</mat-icon>
                </button>
            </div>
            
            <div class="mb-6">
                <div class="flex items-center justify-between mb-2">
                    <span class="text-slate-500 dark:text-slate-400">Product</span>
                    <span class="font-medium text-slate-900 dark:text-white">{{ data.productName }}</span>
                </div>
                <div class="flex items-center justify-between">
                    <span class="text-slate-500 dark:text-slate-400">Current Stock</span>
                    <span 
                        class="px-2 py-1 bg-slate-100 dark:bg-slate-700 rounded-full text-slate-700 dark:text-slate-300 font-medium">
                        {{ data.currentStock }}
                    </span>
                </div>
            </div>
            
            <form [formGroup]="form" (ngSubmit)="onSubmit()">
                <div class="space-y-4">
                    <mat-form-field appearance="outline" class="w-full">
                        <mat-label>Adjustment</mat-label>
                        <input 
                            matInput 
                            type="number" 
                            formControlName="adjustment"
                            placeholder="Enter amount to add or subtract">
                        <mat-hint>Use positive numbers to add stock, negative to remove</mat-hint>
                        @if (form.get('adjustment')?.errors?.['required'] && form.get('adjustment')?.touched) {
                            <mat-error>Adjustment value is required</mat-error>
                        }
                        @if (form.get('adjustment')?.errors?.['max']) {
                            <mat-error>Cannot adjust by more than 10,000 units at once</mat-error>
                        }
                    </mat-form-field>

                    <mat-form-field appearance="outline" class="w-full">
                        <mat-label>Reason</mat-label>
                        <textarea 
                            matInput 
                            formControlName="reason" 
                            rows="3"
                            placeholder="Explain reason for adjustment"></textarea>
                        @if (form.get('reason')?.errors?.['required'] && form.get('reason')?.touched) {
                            <mat-error>Reason is required</mat-error>
                        }
                    </mat-form-field>
                </div>

                <div class="flex justify-end space-x-3 mt-6">
                    <button 
                        mat-stroked-button 
                        type="button" 
                        class="border border-slate-300 dark:border-slate-600 px-4 py-1 rounded-md text-slate-700 dark:text-slate-200 hover:bg-slate-50 dark:hover:bg-slate-700"
                        [mat-dialog-close]="false">
                        Cancel
                    </button>
                    <button 
                        mat-raised-button 
                        color="primary" 
                        type="submit"
                        class="bg-primary-600 text-white px-4 py-1 rounded-md hover:bg-primary-700 transition-colors"
                        [disabled]="form.invalid || form.pristine">
                        <span class="flex items-center">
                            Adjust Stock
                            <mat-icon class="ml-1">save</mat-icon>
                        </span>
                    </button>
                </div>
            </form>
        </div>
    `,
    styles: []
})
export class StockAdjustmentDialogComponent {
    form: FormGroup;

    constructor(
        private fb: FormBuilder,
        private dialogRef: MatDialogRef<StockAdjustmentDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: DialogData,
        private store: Store
    ) {
        this.form = this.fb.group({
            adjustment: [0, [Validators.required, Validators.max(10000)]],
            reason: ['', Validators.required]
        });
    }

    onSubmit() {
        if (this.form.valid) {
            const { adjustment, reason } = this.form.value;

            this.store.dispatch(StockActions.adjustStock({
                adjustment: {
                    productId: this.data.productId,
                    adjustment,
                    reason
                }
            }));

            this.dialogRef.close(true);
        }
    }
}