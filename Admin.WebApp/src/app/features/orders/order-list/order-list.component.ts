// src/app/features/orders/order-list/order-list.component.ts
import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatMenuModule } from '@angular/material/menu';
import { Store } from '@ngrx/store';
import { Observable, Subject, combineLatest } from 'rxjs';
import { debounceTime, distinctUntilChanged, map, takeUntil } from 'rxjs/operators';
import { OrderActions } from '../../../store/order/order.actions';
import { selectAllOrders, selectOrdersLoading, selectOrdersError, selectOrderPagination } from '../../../store/order/order.selectors';
import { StatusBadgeComponent } from 'src/app/shared/components/badges/status-badge.component';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { Order, OrderStatus } from 'src/app/shared/models/order.model';
import { SelectionModel } from '@angular/cdk/collections';

@Component({
    selector: 'app-order-list',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatTableModule,
        MatPaginatorModule,
        MatSortModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatSelectModule,
        MatDatepickerModule,
        MatMenuModule,
        MatCheckboxModule,
        StatusBadgeComponent
    ],
    template: `
        <div class="order-list-container">
            <!-- Filters -->
            <div class="filters-section">
                <mat-form-field>
                    <mat-label>Search</mat-label>
                    <input matInput [formControl]="searchControl" placeholder="Search orders...">
                </mat-form-field>

                <mat-form-field>
                    <mat-label>Status</mat-label>
                    <mat-select [formControl]="statusFilter">
                        <mat-option [value]="null">All</mat-option>
                        <mat-option *ngFor="let status of orderStatuses" [value]="status">
                            {{status}}
                        </mat-option>
                    </mat-select>
                </mat-form-field>

                <mat-form-field>
                    <mat-label>From Date</mat-label>
                    <input matInput [matDatepicker]="fromPicker" [formControl]="fromDateFilter">
                    <mat-datepicker-toggle matSuffix [for]="fromPicker"></mat-datepicker-toggle>
                    <mat-datepicker #fromPicker></mat-datepicker>
                </mat-form-field>

                <mat-form-field>
                    <mat-label>To Date</mat-label>
                    <input matInput [matDatepicker]="toPicker" [formControl]="toDateFilter">
                    <mat-datepicker-toggle matSuffix [for]="toPicker"></mat-datepicker-toggle>
                    <mat-datepicker #toPicker></mat-datepicker>
                </mat-form-field>
            </div>

            <!-- Orders Table -->
            <table mat-table [dataSource]="(orders$ | async) ?? []" matSort (matSortChange)="sortChange($event)">
                 <!-- Selection Column -->
            <ng-container matColumnDef="select">
                <th mat-header-cell *matHeaderCellDef>
                    <mat-checkbox
                        (change)="$event ? masterToggle() : null"
                        [checked]="selection.hasValue() && isAllSelected()"
                        [indeterminate]="selection.hasValue() && !isAllSelected()"
                        [aria-label]="checkboxLabel()">
                    </mat-checkbox>
                </th>
                <td mat-cell *matCellDef="let row">
                    <mat-checkbox
                        (click)="$event.stopPropagation()"
                        (change)="$event ? selection.toggle(row) : null"
                        [checked]="selection.isSelected(row)"
                        [aria-label]="checkboxLabel(row)">
                    </mat-checkbox>
                </td>
            </ng-container>
                <!-- Order Number Column -->
                <ng-container matColumnDef="orderNumber">
                    <th mat-header-cell *matHeaderCellDef mat-sort-header>Order #</th>
                    <td mat-cell *matCellDef="let order">{{order.orderNumber}}</td>
                </ng-container>

                <!-- Status Column -->
                <ng-container matColumnDef="status">
                    <th mat-header-cell *matHeaderCellDef mat-sort-header>Status</th>
                    <td mat-cell *matCellDef="let order">
                        <app-status-badge [status]="order.status"></app-status-badge>
                    </td>
                </ng-container>

                <!-- Customer Column -->
                <ng-container matColumnDef="customer">
                    <th mat-header-cell *matHeaderCellDef>Customer</th>
                    <td mat-cell *matCellDef="let order">
                        {{order.customerName}}
                    </td>
                </ng-container>

                <!-- Total Column -->
                <ng-container matColumnDef="total">
                    <th mat-header-cell *matHeaderCellDef mat-sort-header>Total</th>
                    <td mat-cell *matCellDef="let order">
                        {{order.total | currency:order.currency}}
                    </td>
                </ng-container>

                <!-- Date Column -->
                <ng-container matColumnDef="createdAt">
                    <th mat-header-cell *matHeaderCellDef mat-sort-header>Date</th>
                    <td mat-cell *matCellDef="let order">
                        {{order.createdAt | date:'short'}}
                    </td>
                </ng-container>

                <!-- Actions Column -->
                <ng-container matColumnDef="actions">
                    <th mat-header-cell *matHeaderCellDef>Actions</th>
                    <td mat-cell *matCellDef="let order">
                        <button mat-button [matMenuTriggerFor]="statusMenu">
                            Update Status
                        </button>
                        <mat-menu #statusMenu="matMenu">
                            <button mat-menu-item *ngFor="let status of orderStatuses"
                                    [disabled]="order.status === status"
                                    (click)="updateStatus(order.id, status)">
                                {{status}}
                            </button>
                        </mat-menu>
                    </td>
                </ng-container>

                <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
                <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>

                <!-- No Data Row -->
                <tr class="mat-row" *matNoDataRow>
                    <td class="mat-cell" colspan="6">No orders found</td>
                </tr>
            </table>

            <mat-paginator 
                [length]="(pagination$ | async)?.totalItems"
                [pageSize]="10"
                [pageSizeOptions]="[10, 25, 50]"
                (page)="onPageChange($event)">
            </mat-paginator>
        </div>
    `,
    styles: [`
        .order-list-container {
            padding: 1rem;
        }

        .filters-section {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 1rem;
            margin-bottom: 1rem;
        }

        table {
            width: 100%;
        }

        .mat-column-actions {
            width: 100px;
            text-align: center;
        }

        .mat-column-status {
            width: 120px;
        }

        .mat-column-total {
            width: 120px;
            text-align: right;
        }

        .mat-column-createdAt {
            width: 150px;
        }
    `]
})
export class OrderListComponent implements OnInit, OnDestroy {
    selection = new SelectionModel<Order>(true, []);
    @ViewChild(MatSort) sort!: MatSort;
    @ViewChild(MatPaginator) paginator!: MatPaginator;

    displayedColumns = ['orderNumber', 'status', 'customer', 'total', 'createdAt', 'actions'];
    orderStatuses = Object.values(OrderStatus);
    private destroy$ = new Subject<void>();

    orders$: Observable<Order[]>;
    loading$: Observable<boolean>;
    error$: Observable<string | null>;
    pagination$: Observable<any>;

    // Filters
    searchControl = new FormControl('');
    statusFilter = new FormControl<OrderStatus | null>(null);
    fromDateFilter = new FormControl<Date | null>(null);
    toDateFilter = new FormControl<Date | null>(null);

    constructor(private readonly store: Store) {
        this.orders$ = this.store.select(selectAllOrders);
        this.loading$ = this.store.select(selectOrdersLoading);
        this.error$ = this.store.select(selectOrdersError);
        this.pagination$ = this.store.select(selectOrderPagination);
    }

    ngOnInit() {
        this.initializeFilters();
        this.loadOrders();
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private initializeFilters() {
        combineLatest([
            this.searchControl.valueChanges,
            this.statusFilter.valueChanges,
            this.fromDateFilter.valueChanges,
            this.toDateFilter.valueChanges
        ]).pipe(
            debounceTime(300),
            distinctUntilChanged(),
            takeUntil(this.destroy$)
        ).subscribe(() => {
            this.loadOrders();
        });
    }

    private loadOrders() {
        const filters = {
            searchTerm: this.searchControl.value || undefined,
            status: this.statusFilter.value || undefined,
            fromDate: this.fromDateFilter.value?.toISOString() || undefined,
            toDate: this.toDateFilter.value?.toISOString() || undefined,
            page: this.paginator?.pageIndex ? this.paginator.pageIndex + 1 : 1,
            pageSize: this.paginator?.pageSize || 10,
            sortBy: this.sort?.active,
            sortDirection: this.sort?.direction || undefined
        };

        this.store.dispatch(OrderActions.loadOrders({ params: filters }));
    }
    masterToggle() {
        this.isAllSelected() ?
            this.selection.clear() :
            this.orders$.subscribe(orders => orders.forEach(order => this.selection.select(order)));
    }

    isAllSelected() {
        const numSelected = this.selection.selected.length;
        let numRows = 0;
        this.orders$.pipe(map(orders => orders.length)).subscribe(length => numRows = length);
        return numSelected === numRows;
    }

    checkboxLabel(row?: Order): string {
        if (!row) {
            return `${this.isAllSelected() ? 'select' : 'deselect'} all`;
        }
        return `${this.selection.isSelected(row) ? 'deselect' : 'select'} row ${row.orderNumber}`;
    }

    sortChange(sort: Sort) {
        this.loadOrders();
    }

    updateStatus(orderId: string, newStatus: OrderStatus) {
        this.store.dispatch(OrderActions.updateStatus({ orderId, newStatus }));
    }


    onPageChange(event: PageEvent) {
        this.loadOrders();
    }
}