import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';

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
        MatCheckboxModule,
        MatIconModule
    ],
    template: `
        <div class="save-preset-dialog">
            <!-- Dialog Header -->
            <div class="flex items-center justify-between px-6 py-4 border-b border-slate-200 dark:border-slate-700">
                <h2 class="text-xl font-medium text-slate-900 dark:text-white">Save Search Preset</h2>
                <button mat-icon-button (click)="onCancel()"
                    class="text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-700 rounded-full transition-colors">
                    <mat-icon>close</mat-icon>
                </button>
            </div>
            
            <form [formGroup]="form" (ngSubmit)="onSubmit()">
                <!-- Dialog Content -->
                <div class="p-6">
                    <div class="grid grid-cols-1 gap-4">
                        <mat-form-field  class="w-full">
                            <mat-label>Preset Name</mat-label>
                            <input matInput formControlName="name" placeholder="Enter preset name">
                            <mat-error class="text-xs text-red-500"  *ngIf="form.get('name')?.errors?.['required']">
                                Name is required
                            </mat-error>
                            <mat-error class="text-xs text-red-500"  *ngIf="form.get('name')?.errors?.['minlength']">
                                Name should be at least 3 characters
                            </mat-error>
                        </mat-form-field>

                        <mat-form-field  class="w-full">
                            <mat-label>Description</mat-label>
                            <textarea matInput formControlName="description" 
                                    placeholder="Enter description (optional)"
                                    rows="3"></textarea>
                        </mat-form-field>

                        <div class="mt-2">
                            <mat-checkbox formControlName="isGlobal" 
                                        class="text-slate-700 dark:text-slate-300">
                                Share with all admin users
                            </mat-checkbox>
                        </div>
                    </div>
                </div>

                <!-- Dialog Actions -->
                <div class="flex justify-end gap-3 px-6 py-4 border-t border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-700">
                    <button mat-button type="button" (click)="onCancel()"
                        class="border border-slate-300 dark:border-slate-600 px-4 py-1 rounded-md text-slate-700 dark:text-slate-200 hover:bg-slate-50 dark:hover:bg-slate-700">
                        Cancel
                    </button>
                    <button mat-raised-button color="primary" 
                            type="submit"
                            [disabled]="!form.valid"
                            class="bg-primary-600 text-white px-4 py-1 rounded-md hover:bg-primary-700 transition-colors">
                        Save Preset
                    </button>
                </div>
            </form>
        </div>
    `,
    styles: []
})
export class SavePresetDialogComponent {
    form: FormGroup;

    constructor(
        private readonly dialogRef: MatDialogRef<SavePresetDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: { filters: any[] },
        private readonly fb: FormBuilder
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