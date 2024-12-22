// src/app/features/products/components/stock-management/stock-adjustment-dialog.component.ts
import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Store } from '@ngrx/store';
import { StockItem } from '../../../../shared/models/stock.model';
import { StockActions } from '../../../../store/stock/stock.actions';

@Component({
    selector: 'app-stock-adjustment-dialog',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule
    ],
    template: `
        <h2 mat-dialog-title>Adjust Stock</h2>
        
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
            <mat-dialog-content>
                <div class="form-content">
                    <p>Current Stock: {{ data.currentStock }}</p>
                    
                    <mat-form-field appearance="outline">
                        <mat-label>Adjustment</mat-label>
                        <input matInput type="number" formControlName="adjustment">
                        <mat-hint>Use positive numbers to add stock, negative to remove</mat-hint>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                        <mat-label>Reason</mat-label>
                        <textarea matInput formControlName="reason" rows="3"></textarea>
                    </mat-form-field>
                </div>
            </mat-dialog-content>

            <mat-dialog-actions align="end">
                <button mat-button type="button" (click)="onCancel()">Cancel</button>
                <button mat-raised-button color="primary" 
                        type="submit"
                        [disabled]="form.invalid || form.pristine">
                    Adjust Stock
                </button>
            </mat-dialog-actions>
        </form>
    `,
    styles: [`
        .form-content {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            min-width: 300px;
        }
    `]
})
export class StockAdjustmentDialogComponent {
    form: FormGroup;

    constructor(
        private fb: FormBuilder,
        private dialogRef: MatDialogRef<StockAdjustmentDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: { productId: string; currentStock: number },
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
            this.dialogRef.close();
        }
    }

    onCancel() {
        this.dialogRef.close();
    }
}