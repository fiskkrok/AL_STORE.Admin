// src/app/features/orders/components/add-note-dialog/add-note-dialog.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

@Component({
    selector: 'app-add-note-dialog',
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
        <h2 mat-dialog-title>Add Admin Note</h2>
        
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
            <mat-dialog-content>
                <div class="form-fields">
                    <mat-form-field >
                        <mat-label>Note Type</mat-label>
                        <mat-select formControlName="type">
                            <mat-option value="general">General Note</mat-option>
                            <mat-option value="customer_service">Customer Service</mat-option>
                            <mat-option value="shipping">Shipping</mat-option>
                            <mat-option value="payment">Payment</mat-option>
                        </mat-select>
                        <mat-error class="text-xs text-red-500"  *ngIf="form.get('type')?.errors?.['required']">
                            Note type is required
                        </mat-error>
                    </mat-form-field>

                    <mat-form-field >
                        <mat-label>Note Content</mat-label>
                        <textarea matInput formControlName="content" 
                                  placeholder="Enter note content"
                                  rows="4"></textarea>
                        <mat-error class="text-xs text-red-500"  *ngIf="form.get('content')?.errors?.['required']">
                            Note content is required
                        </mat-error>
                        <mat-error class="text-xs text-red-500"  *ngIf="form.get('content')?.errors?.['minlength']">
                            Note must be at least 3 characters
                        </mat-error>
                        <mat-hint class="text-xs text-orange-500" align="end">
                            {{form.get('content')?.value?.length || 0}}/1000
                        </mat-hint>
                    </mat-form-field>

                    <mat-form-field >
                        <mat-label>Visibility</mat-label>
                        <mat-select formControlName="isInternal">
                            <mat-option [value]="true">Internal Only</mat-option>
                            <mat-option [value]="false">Visible to Customer</mat-option>
                        </mat-select>
                    </mat-form-field>
                </div>
            </mat-dialog-content>

            <mat-dialog-actions align="end">
                <button mat-button type="button" (click)="onCancel()">Cancel</button>
                <button mat-raised-button color="primary" 
                        type="submit"
                        [disabled]="form.invalid || form.pristine">
                    Add Note
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

        ::ng-deep {
            .mat-mdc-form-field-subscript-wrapper {
                display: flex;
                justify-content: space-between;
            }
        }
    `]
})
export class AddNoteDialogComponent {
    form: FormGroup;

    constructor(
        private dialogRef: MatDialogRef<AddNoteDialogComponent>,
        private fb: FormBuilder
    ) {
        this.form = this.fb.group({
            type: ['general', Validators.required],
            content: ['', [
                Validators.required,
                Validators.minLength(3),
                Validators.maxLength(1000)
            ]],
            isInternal: [true]
        });
    }

    onSubmit() {
        if (this.form.valid) {
            this.dialogRef.close({
                ...this.form.value,
                createdAt: new Date().toISOString()
            });
        }
    }

    onCancel() {
        this.dialogRef.close();
    }
}