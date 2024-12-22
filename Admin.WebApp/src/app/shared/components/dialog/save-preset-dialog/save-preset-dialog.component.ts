import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';

@Component({
    selector: 'app-save-preset-dialog',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatCheckboxModule
    ],
    template: `
        <h2 mat-dialog-title>Save Search Preset</h2>
        
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
            <mat-dialog-content>
                <div class="form-fields">
                    <mat-form-field appearance="outline">
                        <mat-label>Preset Name</mat-label>
                        <input matInput formControlName="name" placeholder="Enter preset name">
                        <mat-error *ngIf="form.get('name')?.errors?.['required']">
                            Name is required
                        </mat-error>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                        <mat-label>Description</mat-label>
                        <textarea matInput formControlName="description" 
                                  placeholder="Enter description (optional)"
                                  rows="3"></textarea>
                    </mat-form-field>

                    <mat-checkbox formControlName="isGlobal">
                        Share with all admin users
                    </mat-checkbox>
                </div>
            </mat-dialog-content>

            <mat-dialog-actions align="end">
                <button mat-button type="button" (click)="onCancel()">Cancel</button>
                <button mat-raised-button color="primary" 
                        type="submit"
                        [disabled]="!form.valid">
                    Save Preset
                </button>
            </mat-dialog-actions>
        </form>
    `,
    styles: [`
        .form-fields {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            min-width: 300px;
            max-width: 500px;
        }

        mat-checkbox {
            margin-top: 1rem;
        }
    `]
})
export class SavePresetDialogComponent {
    form: FormGroup;

    constructor(
        private dialogRef: MatDialogRef<SavePresetDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: { filters: any[] },
        private fb: FormBuilder
    ) {
        this.form = this.fb.group({
            name: ['', [Validators.required, Validators.minLength(3)]],
            description: [''],
            isGlobal: [false]
        });
    }

    onSubmit() {
        if (this.form.valid) {
            this.dialogRef.close({
                ...this.form.value,
                filters: this.data.filters
            });
        }
    }

    onCancel() {
        this.dialogRef.close();
    }
}