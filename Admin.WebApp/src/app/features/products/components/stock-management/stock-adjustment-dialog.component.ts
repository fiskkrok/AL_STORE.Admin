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
        <div class="stock-adjustment-dialog">
            <!-- Dialog Header -->
            <div class="flex items-center justify-between px-6 py-4 border-b border-slate-200 dark:border-slate-700">
                <h2 class="text-xl font-medium text-slate-900 dark:text-white">Adjust Stock</h2>
                <button mat-icon-button [mat-dialog-close]="false"
                    class="text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-full transition-colors">
                    <mat-icon>close</mat-icon>
                </button>
            </div>

            <!-- Product Info -->
            <div class="px-6 pt-6">
                <div class="bg-slate-50 dark:bg-slate-700 rounded-lg p-4 mb-6 border border-slate-200 dark:border-slate-700">
                    <div class="grid grid-cols-2 gap-2">
                        <p class="text-sm text-slate-500 dark:text-slate-400">Product:</p>
                        <p class="text-sm font-medium text-slate-900 dark:text-white">{{ data.productName }}</p>

                        <p class="text-sm text-slate-500 dark:text-slate-400">Current Stock:</p>
                        <p class="text-sm font-medium">
                            <span class="px-2 py-1 bg-slate-100 dark:bg-slate-700 rounded-full text-slate-700 dark:text-slate-300 font-medium">
                                {{ data.currentStock }}
                            </span>
                        </p>
                    </div>
                </div>
            </div>

            <!-- Form -->
            <form [formGroup]="form" (ngSubmit)="onSubmit()" class="px-6 pb-6">
                <div class="grid grid-cols-1 gap-4">
                    <mat-form-field  class="w-full">
                        <mat-label>Adjustment Amount</mat-label>
                        <input matInput type="number" formControlName="adjustment" placeholder="Enter adjustment value">
                        <mat-hint class="text-xs text-orange-500">Use positive value to add stock, negative to remove</mat-hint>
                        <mat-error class="text-xs text-red-500"  *ngIf="form.get('adjustment')?.errors?.['required']">
                            Adjustment amount is required
                        </mat-error>
                        <mat-error class="text-xs text-red-500"  *ngIf="form.get('adjustment')?.errors?.['max']">
                            Adjustment cannot exceed 10,000 units
                        </mat-error>
                    </mat-form-field>

                    <mat-form-field  class="w-full">
                        <mat-label>Reason</mat-label>
                        <textarea matInput formControlName="reason" rows="3"
                            placeholder="Explain reason for adjustment"></textarea>
                        <mat-error class="text-xs text-red-500"  *ngIf="form.get('reason')?.errors?.['required']">
                            Reason is required
                        </mat-error>
                    </mat-form-field>
                </div>

                <!-- Dialog Actions -->
                <div class="flex justify-end gap-3 mt-6 pt-6 border-t border-slate-200 dark:border-slate-700">
                    <button mat-button type="button" [mat-dialog-close]="false"
                        class="border border-slate-300 dark:border-slate-600 px-4 py-1 rounded-md text-slate-700 dark:text-slate-200 hover:bg-slate-50 dark:hover:bg-slate-700">
                        Cancel
                    </button>
                    <button mat-raised-button color="primary" type="submit"
                        class="bg-primary-600 text-white px-4 py-1 rounded-md hover:bg-primary-700 transition-colors flex items-center"
                        [disabled]="form.invalid || form.pristine">
                        <span>Adjust Stock</span>
                        <mat-icon class="ml-1">save</mat-icon>
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
        private readonly fb: FormBuilder,
        private readonly dialogRef: MatDialogRef<StockAdjustmentDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: DialogData,
        private readonly store: Store
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