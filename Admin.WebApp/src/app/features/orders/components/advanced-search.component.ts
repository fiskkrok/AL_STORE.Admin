import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatIconModule } from '@angular/material/icon';
import { Store } from '@ngrx/store';
import { OrderStatus } from 'src/app/shared/models/orders/order.model';
import { OrderActions } from 'src/app/store/order/order.actions';

interface FilterField {
    type: 'text' | 'select' | 'number' | 'date' | 'boolean';
    field: string;
    label: string;
    options?: { value: any; label: string; }[];
}

@Component({
    selector: 'app-advanced-search',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatExpansionModule,
        MatFormFieldModule,
        MatSelectModule,
        MatInputModule,
        MatChipsModule,
        MatDatepickerModule,
        MatIconModule
    ],
    template: `
        <div class="search-builder">
            <!-- Quick Filters -->
            <div class="quick-filters">
                <mat-chip-set>
                    @for (preset of searchPresets; track preset.name) {
                        <mat-chip 
                            (click)="applyPreset(preset)"
                            [attr.selected]="activePreset === preset.name ? true : null">
                            {{preset.label}}
                        </mat-chip>
                    }
                </mat-chip-set>
            </div>

            <!-- Advanced Search Panel -->
            <mat-expansion-panel>
                <mat-expansion-panel-header>
                    <mat-panel-title>
                        Advanced Search
                    </mat-panel-title>
                </mat-expansion-panel-header>

                <form [formGroup]="searchForm" (ngSubmit)="applySearch()">
                    <div class="filters-grid" formArrayName="filters">
                        @for (filter of filtersArray.controls; track filter; let i = $index) {
                            <div class="filter-row" [formGroupName]="i">
                                <!-- Field Selection -->
                                <mat-form-field>
                                    <mat-label>Field</mat-label>
                                    <mat-select formControlName="field" (selectionChange)="onFieldChange(i)">
                                        @for (field of availableFields; track field.field) {
                                            <mat-option [value]="field.field">
                                                {{field.label}}
                                            </mat-option>
                                        }
                                    </mat-select>
                                </mat-form-field>

                                <!-- Operator Selection -->
                                <mat-form-field>
                                    <mat-label>Operator</mat-label>
                                    <mat-select formControlName="operator">
                                        @for (op of getOperators(filter.value.field); track op.value) {
                                            <mat-option [value]="op.value">
                                                {{op.label}}
                                            </mat-option>
                                        }
                                    </mat-select>
                                </mat-form-field>

                                <!-- Value Input -->
                                @switch (getFieldType(filter.value.field)) {
                                    @case ('text') {
                                        <mat-form-field>
                                            <input matInput formControlName="value" placeholder="Value">
                                        </mat-form-field>
                                    }
                                    @case ('select') {
                                        <mat-form-field>
                                            <mat-select formControlName="value">
                                                @for (option of getFieldOptions(filter.value.field); track option.value) {
                                                    <mat-option [value]="option.value">
                                                        {{option.label}}
                                                    </mat-option>
                                                }
                                            </mat-select>
                                        </mat-form-field>
                                    }
                                    @case ('number') {
                                        <mat-form-field>
                                            <input matInput type="number" formControlName="value" placeholder="Value">
                                        </mat-form-field>
                                    }
                                    @case ('date') {
                                        <mat-form-field>
                                            <input matInput [matDatepicker]="picker" formControlName="value">
                                            <mat-datepicker-toggle matSuffix [for]="picker"></mat-datepicker-toggle>
                                            <mat-datepicker #picker></mat-datepicker>
                                        </mat-form-field>
                                    }
                                }

                                <!-- Remove Filter -->
                                <button mat-icon-button (click)="removeFilter(i)" type="button">
                                    <mat-icon>remove_circle</mat-icon>
                                </button>
                            </div>
                        }
                    </div>

                    <!-- Add Filter Button -->
                    <div class="filter-actions">
                        <button mat-button type="button" (click)="addFilter()">
                            <mat-icon>add</mat-icon> Add Filter
                        </button>
                        <button mat-button type="button" (click)="saveAsPreset()"
                                [disabled]="!searchForm.valid">
                            Save as Preset
                        </button>
                    </div>

                    <!-- Apply Filters -->
                    <div class="search-actions">
                        <button mat-button type="button" (click)="resetSearch()">
                            Reset
                        </button>
                        <button mat-raised-button color="primary" type="submit"
                                [disabled]="!searchForm.valid">
                            Apply Filters
                        </button>
                    </div>
                </form>
            </mat-expansion-panel>

            <!-- Active Filters Display -->
            @if (activeFilters.length > 0) {
                <div class="active-filters">
                    <h4>Active Filters:</h4>
                    <mat-chip-set>
                        @for (filter of activeFilters; track filter) {
                            <mat-chip (removed)="removeActiveFilter(filter)">
                                {{filter.label}}
                                <mat-icon matChipRemove>cancel</mat-icon>
                            </mat-chip>
                        }
                    </mat-chip-set>
                </div>
            }
        </div>
    `,
    styles: [`
        .search-builder {
            padding: 1rem;
        }

        .quick-filters {
            margin-bottom: 1rem;
        }

        .filters-grid {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            margin: 1rem 0;
        }

        .filter-row {
            display: grid;
            grid-template-columns: repeat(3, 1fr) auto;
            gap: 1rem;
            align-items: center;
        }

        .filter-actions {
            display: flex;
            gap: 1rem;
            margin: 1rem 0;
        }

        .search-actions {
            display: flex;
            justify-content: flex-end;
            gap: 1rem;
            margin-top: 1rem;
        }

        .active-filters {
            margin-top: 1rem;
            
            h4 {
                margin: 0 0 0.5rem;
                color: var(--text-secondary);
            }
        }
    `]
})
export class AdvancedSearchComponent implements OnInit {
    searchForm: FormGroup;
    activePreset: string | null = null;
    activeFilters: Array<{ label: string; field: string; }> = [];

    // Available fields for filtering
    availableFields: FilterField[] = [
        { field: 'orderNumber', label: 'Order Number', type: 'text' },
        {
            field: 'status', label: 'Status', type: 'select',
            options: Object.entries(OrderStatus).map(([value, label]) => ({ value, label }))
        },
        { field: 'createdAt', label: 'Order Date', type: 'date' },
        { field: 'total', label: 'Order Total', type: 'number' },
        { field: 'customerEmail', label: 'Customer Email', type: 'text' },
        {
            field: 'shippingCarrier', label: 'Shipping Carrier', type: 'select',
            options: [
                { value: 'fedex', label: 'FedEx' },
                { value: 'ups', label: 'UPS' },
                { value: 'usps', label: 'USPS' }
            ]
        },
        { field: 'shippingCountry', label: 'Shipping Country', type: 'text' },
        {
            field: 'paymentMethod', label: 'Payment Method', type: 'select',
            options: [
                { value: 'credit_card', label: 'Credit Card' },
                { value: 'paypal', label: 'PayPal' },
                { value: 'bank_transfer', label: 'Bank Transfer' }
            ]
        }
    ];

    // Common search presets
    searchPresets = [
        {
            name: 'pending_orders',
            label: 'Pending Orders',
            filters: [
                { field: 'status', operator: 'equals', value: 'pending' }
            ]
        },
        {
            name: 'todays_orders',
            label: 'Today\'s Orders',
            filters: [
                { field: 'createdAt', operator: 'equals', value: new Date() }
            ]
        },
        {
            name: 'high_value',
            label: 'High Value Orders',
            filters: [
                { field: 'total', operator: 'greater_than', value: 1000 }
            ]
        }
    ];

    constructor(
        private fb: FormBuilder,
        private store: Store
    ) {
        this.searchForm = this.fb.group({
            filters: this.fb.array([])
        });
    }

    get filtersArray() {
        return this.searchForm.get('filters') as FormArray;
    }

    ngOnInit() {
        // Add initial empty filter
        this.addFilter();
    }

    addFilter() {
        const filterGroup = this.fb.group({
            field: [''],
            operator: [''],
            value: ['']
        });

        this.filtersArray.push(filterGroup);
    }

    removeFilter(index: number) {
        this.filtersArray.removeAt(index);
    }

    onFieldChange(index: number) {
        const filter = this.filtersArray.at(index);
        filter.patchValue({ operator: '', value: '' });
    }

    getFieldType(fieldName: string): string {
        return this.availableFields.find(f => f.field === fieldName)?.type || 'text';
    }

    getFieldOptions(fieldName: string) {
        return this.availableFields.find(f => f.field === fieldName)?.options || [];
    }

    getOperators(fieldName: string) {
        const type = this.getFieldType(fieldName);
        switch (type) {
            case 'text':
                return [
                    { value: 'equals', label: 'Equals' },
                    { value: 'contains', label: 'Contains' },
                    { value: 'starts_with', label: 'Starts with' },
                    { value: 'ends_with', label: 'Ends with' }
                ];
            case 'number':
                return [
                    { value: 'equals', label: 'Equals' },
                    { value: 'greater_than', label: 'Greater than' },
                    { value: 'less_than', label: 'Less than' },
                    { value: 'between', label: 'Between' }
                ];
            case 'date':
                return [
                    { value: 'equals', label: 'On' },
                    { value: 'after', label: 'After' },
                    { value: 'before', label: 'Before' },
                    { value: 'between', label: 'Between' }
                ];
            default:
                return [
                    { value: 'equals', label: 'Equals' },
                    { value: 'not_equals', label: 'Does not equal' }
                ];
        }
    }

    applyPreset(preset: any) {
        this.activePreset = preset.name;
        this.filtersArray.clear();

        preset.filters.forEach((filter: any) => {
            this.filtersArray.push(this.fb.group({
                field: [filter.field],
                operator: [filter.operator],
                value: [filter.value]
            }));
        });

        this.applySearch();
    }

    applySearch() {
        const filters = this.searchForm.value.filters;
        this.store.dispatch(OrderActions.setFilters({ filters }));

        // Update active filters display
        this.updateActiveFilters(filters);
    }

    private updateActiveFilters(filters: any[]) {
        this.activeFilters = filters.map(filter => {
            const field = this.availableFields.find(f => f.field === filter.field);
            const operator = this.getOperators(filter.field)
                .find(op => op.value === filter.operator);

            return {
                field: filter.field,
                label: `${field?.label} ${operator?.label} ${filter.value}`
            };
        });
    }

    removeActiveFilter(filter: any) {
        const index = this.filtersArray.controls
            .findIndex(control => control.value.field === filter.field);

        if (index !== -1) {
            this.removeFilter(index);
            this.applySearch();
        }
    }

    resetSearch() {
        this.activePreset = null;
        this.filtersArray.clear();
        this.addFilter();
        this.activeFilters = [];
        this.store.dispatch(OrderActions.resetFilters());
    }

    async saveAsPreset() {
        const dialogRef = this.dialog.open(SavePresetDialogComponent, {
            data: {
                filters: this.searchForm.value.filters
            }
        });

        const result = await dialogRef.afterClosed().toPromise();
        if (result) {
            // Save preset logic
        }
    }
}