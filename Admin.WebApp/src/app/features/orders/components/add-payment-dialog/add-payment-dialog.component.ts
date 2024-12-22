// src/app/features/orders/components/add-payment-dialog/add-payment-dialog.component.ts
import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

interface DialogData {
    orderId: string;
    amount: number;
    currency: string;
}

@Component({
    selector: 'app-add-payment-dialog',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule
    ],
    template: `
        <h2 mat-dialog-title>Add Payment</h2>
        
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
            <mat-dialog-content>
                <div class="form-fields">
                    <mat-form-field appearance="outline">
                        <mat-label>Transaction ID</mat-label>
                        <input matInput formControlName="transactionId" placeholder="Enter transaction ID">
                        <mat-error *ngIf="form.get('transactionId')?.errors?.['required']">
                            Transaction ID is required
                        </mat-error>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                        <mat-label>Payment Method</mat-label>
                        <mat-select formControlName="method">
                            <mat-option value="credit_card">Credit Card</mat-option>
                            <mat-option value="bank_transfer">Bank Transfer</mat-option>
                            <mat-option value="paypal">PayPal</mat-option>
                        </mat-select>
                        <mat-error *ngIf="form.get('method')?.errors?.['required']">
                            Payment method is required
                        </mat-error>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                        <mat-label>Amount</mat-label>
                        <input matInput type="number" formControlName="amount" 
                               [placeholder]="'Amount in ' + data.currency">
                        <mat-error *ngIf="form.get('amount')?.errors?.['required']">
                            Amount is required
                        </mat-error>
                        <mat-error *ngIf="form.get('amount')?.errors?.['min']">
                            Amount must be greater than 0
                        </mat-error>
                        <mat-error *ngIf="form.get('amount')?.errors?.['max']">
                            Amount cannot exceed order total
                        </mat-error>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                        <mat-label>Reference</mat-label>
                        <input matInput formControlName="reference" placeholder="Enter payment reference">
                    </mat-form-field>
                </div>
            </mat-dialog-content>

            <mat-dialog-actions align="end">
                <button mat-button type="button" (click)="onCancel()">Cancel</button>
                <button mat-raised-button color="primary" type="submit"
                        [disabled]="form.invalid || form.pristine">
                    Add Payment
                </button>
            </mat-dialog-actions>
        </form>
    `,
    styles: [`
        .form-fields {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            min-width: 400px;
        }
    `]
})
export class AddPaymentDialogComponent {
    form: FormGroup;

    constructor(
        private dialogRef: MatDialogRef<AddPaymentDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: DialogData,
        private fb: FormBuilder
    ) {
        this.form = this.fb.group({
            transactionId: ['', Validators.required],
            method: ['', Validators.required],
            amount: [data.amount, [
                Validators.required,
                Validators.min(0.01),
                Validators.max(data.amount)
            ]],
            reference: [''],
            currency: [data.currency]
        });
    }

    onSubmit() {
        if (this.form.valid) {
            this.dialogRef.close(this.form.value);
        }
    }

    onCancel() {
        this.dialogRef.close();
    }
}