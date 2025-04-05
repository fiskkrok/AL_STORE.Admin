// src/app/features/orders/components/update-shipping-dialog/update-shipping-dialog.component.ts
import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';

interface DialogData {
    orderId: string;
    currentShipping?: {
        carrier: string;
        trackingNumber: string;
        estimatedDeliveryDate: string;
    };
}

interface CarrierOption {
    value: string;
    label: string;
    trackingUrlPattern?: string;
}

@Component({
    selector: 'app-update-shipping-dialog',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatDatepickerModule,
        MatNativeDateModule
    ],
    template: `
        <h2 mat-dialog-title>Update Shipping Information</h2>
        
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
            <mat-dialog-content>
                <div class="form-fields">
                    <mat-form-field >
                        <mat-label>Shipping Carrier</mat-label>
                        <mat-select formControlName="carrier">
                            @for (carrier of carriers; track carrier.value) {
                                <mat-option [value]="carrier.value">
                                    {{carrier.label}}
                                </mat-option>
                            }
                        </mat-select>
                        <mat-error class="text-xs text-red-500"  *ngIf="form.get('carrier')?.errors?.['required']">
                            Carrier is required
                        </mat-error>
                    </mat-form-field>

                    <mat-form-field >
                        <mat-label>Tracking Number</mat-label>
                        <input matInput formControlName="trackingNumber" 
                               placeholder="Enter tracking number">
                        <mat-error class="text-xs text-red-500"  *ngIf="form.get('trackingNumber')?.errors?.['required']">
                            Tracking number is required
                        </mat-error>
                        @if (trackingUrl) {
                            <mat-hint class="text-xs text-orange-500">
                                <a [href]="trackingUrl" target="_blank" rel="noopener noreferrer">
                                    Track Package
                                </a>
                            </mat-hint>
                        }
                    </mat-form-field>

                    <mat-form-field >
                        <mat-label>Estimated Delivery Date</mat-label>
                        <input matInput [matDatepicker]="picker" 
                               formControlName="estimatedDeliveryDate">
                        <mat-datepicker-toggle matIconSuffix [for]="picker">
                        </mat-datepicker-toggle>
                        <mat-datepicker #picker></mat-datepicker>
                        <mat-error class="text-xs text-red-500"  *ngIf="form.get('estimatedDeliveryDate')?.errors?.['required']">
                            Estimated delivery date is required
                        </mat-error>
                        <mat-error class="text-xs text-red-500"  *ngIf="form.get('estimatedDeliveryDate')?.errors?.['min']">
                            Date cannot be in the past
                        </mat-error>
                    </mat-form-field>

                    <mat-form-field >
                        <mat-label>Notes</mat-label>
                        <textarea matInput formControlName="notes" 
                                  placeholder="Enter shipping notes"
                                  rows="3"></textarea>
                    </mat-form-field>
                </div>
            </mat-dialog-content>

            <mat-dialog-actions align="end">
                <button mat-button type="button" (click)="onCancel()">Cancel</button>
                <button mat-raised-button color="primary" 
                        type="submit"
                        [disabled]="form.invalid || form.pristine">
                    Update Shipping
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
            max-width: 600px;
        }

        textarea {
            min-height: 100px;
        }
    `]
})
export class UpdateShippingDialogComponent {
    form: FormGroup;
    carriers: CarrierOption[] = [
        {
            value: 'fedex',
            label: 'FedEx',
            trackingUrlPattern: 'https://www.fedex.com/fedextrack/?trknbr={tracking}'
        },
        {
            value: 'ups',
            label: 'UPS',
            trackingUrlPattern: 'https://www.ups.com/track?tracknum={tracking}'
        },
        {
            value: 'usps',
            label: 'USPS',
            trackingUrlPattern: 'https://tools.usps.com/go/TrackConfirmAction?tLabels={tracking}'
        },
        {
            value: 'dhl',
            label: 'DHL',
            trackingUrlPattern: 'https://www.dhl.com/en/express/tracking.html?AWB={tracking}'
        }
    ];

    get trackingUrl(): string | null {
        const carrier = this.carriers.find(c => c.value === this.form.get('carrier')?.value);
        const tracking = this.form.get('trackingNumber')?.value;

        if (carrier?.trackingUrlPattern && tracking) {
            return carrier.trackingUrlPattern.replace('{tracking}', tracking);
        }

        return null;
    }

    constructor(
        private dialogRef: MatDialogRef<UpdateShippingDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: DialogData,
        private fb: FormBuilder
    ) {
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        this.form = this.fb.group({
            carrier: [data.currentShipping?.carrier || '', Validators.required],
            trackingNumber: [data.currentShipping?.trackingNumber || '', Validators.required],
            estimatedDeliveryDate: [
                data.currentShipping?.estimatedDeliveryDate ? new Date(data.currentShipping.estimatedDeliveryDate) : null,
                [
                    Validators.required,
                    (control) => {
                        const date = control.value;
                        return date && date < today ? { min: true } : null;
                    }
                ]
            ],
            notes: ['']
        });

        // Update tracking URL when carrier or tracking number changes
        this.form.get('carrier')?.valueChanges.subscribe(() => this.updateTrackingUrl());
        this.form.get('trackingNumber')?.valueChanges.subscribe(() => this.updateTrackingUrl());
    }

    private updateTrackingUrl() {
        // Force change detection for tracking URL
        this.form.updateValueAndValidity();
    }

    onSubmit() {
        if (this.form.valid) {
            const formValue = this.form.value;
            this.dialogRef.close({
                ...formValue,
                estimatedDeliveryDate: formValue.estimatedDeliveryDate.toISOString()
            });
        }
    }

    onCancel() {
        this.dialogRef.close();
    }
}