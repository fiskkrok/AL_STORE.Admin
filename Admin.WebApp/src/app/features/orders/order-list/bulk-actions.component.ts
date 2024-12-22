import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SelectionModel } from '@angular/cdk/collections';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Store } from '@ngrx/store';
import * as XLSX from 'xlsx';
import { Order } from 'src/app/shared/models/orders/order.model';
import { BulkActionsService } from '../../../core/services/bulk-actions.service';

@Component({
    selector: 'app-bulk-actions',
    standalone: true,
    imports: [
        CommonModule,
        MatButtonModule,
        MatMenuModule,
        MatTooltipModule,
        MatCheckboxModule
    ],
    template: `
        <div class="bulk-actions" [class.visible]="selection.hasValue()">
            <div class="selected-info">
                <span class="count">{{ selection.selected.length }} orders selected</span>
                <button mat-button (click)="clearSelection()">Clear</button>
            </div>

            <div class="actions">
                <!-- Status Update -->
                <button mat-raised-button [matMenuTriggerFor]="statusMenu" color="primary">
                    Update Status
                </button>
                <mat-menu #statusMenu="matMenu">
                    @for (status of orderStatuses; track status) {
                        <button mat-menu-item (click)="updateStatus(status)">
                            {{ status }}
                        </button>
                    }
                </mat-menu>

                <!-- Export -->
                <button mat-raised-button [matMenuTriggerFor]="exportMenu">
                    Export
                </button>
                <mat-menu #exportMenu="matMenu">
                    <button mat-menu-item (click)="exportToCsv()">
                        Export to CSV
                    </button>
                    <button mat-menu-item (click)="exportToExcel()">
                        Export to Excel
                    </button>
                </mat-menu>

                <!-- Print -->
                <button mat-raised-button [matMenuTriggerFor]="printMenu">
                    Print
                </button>
                <mat-menu #printMenu="matMenu">
                    <button mat-menu-item (click)="printInvoices()">
                        Print Invoices
                    </button>
                    <button mat-menu-item (click)="printShippingLabels()">
                        Print Shipping Labels
                    </button>
                    <button mat-menu-item (click)="printPackingSlips()">
                        Print Packing Slips
                    </button>
                </mat-menu>
            </div>
        </div>
    `,
    styles: [`
        .bulk-actions {
            position: fixed;
            bottom: 0;
            left: 0;
            right: 0;
            background-color: var(--bg-secondary);
            padding: 1rem;
            display: flex;
            justify-content: space-between;
            align-items: center;
            border-top: 1px solid var(--border);
            transform: translateY(100%);
            transition: transform 0.3s ease;
            z-index: 1000;

            &.visible {
                transform: translateY(0);
            }
        }

        .selected-info {
            display: flex;
            align-items: center;
            gap: 1rem;

            .count {
                font-weight: 500;
                color: var(--text-primary);
            }
        }

        .actions {
            display: flex;
            gap: 1rem;
        }
    `]
})
export class BulkActionsComponent {
    @Input() selection: SelectionModel<Order>;
    orderStatuses = Object.values(OrderStatus);

    constructor(
        private readonly bulkActionsService: BulkActionsService,
        private readonly store: Store
    ) { }

    clearSelection() {
        this.selection.clear();
    }

    async updateStatus(newStatus: OrderStatus) {
        const confirmed = await this.dialogService.confirm(
            `Are you sure you want to update ${this.selection.selected.length} orders to ${newStatus}?`,
            'Update Status'
        );

        if (confirmed) {
            const orderIds = this.selection.selected.map(order => order.id);
            this.bulkActionsService.updateOrderStatus(orderIds, newStatus).subscribe({
                next: () => {
                    this.snackBar.open('Orders updated successfully', 'Close', {
                        duration: 3000
                    });
                    this.clearSelection();
                },
                error: (error) => {
                    this.errorService.addError({
                        message: 'Failed to update orders: ' + error.message,
                        code: error.code || 'UPDATE_ERROR',
                        severity: 'error'
                    });
                }
            });
        }
    }

    exportToCsv() {
        const data = this.prepareExportData();
        const csv = Papa.unparse(data);
        const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        saveAs(blob, `orders_export_${new Date().toISOString()}.csv`);
    }

    exportToExcel() {
        const data = this.prepareExportData();
        const ws = XLSX.utils.json_to_sheet(data);
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, 'Orders');
        XLSX.writeFile(wb, `orders_export_${new Date().toISOString()}.xlsx`);
    }

    private prepareExportData(): any[] {
        return this.selection.selected.map(order => ({
            'Order Number': order.orderNumber,
            'Date': new Date(order.createdAt).toLocaleDateString(),
            'Status': order.status,
            'Customer': `${order.shippingAddress.firstName} ${order.shippingAddress.lastName}`,
            'Email': order.customerEmail,
            'Total': `${order.total.amount} ${order.total.currency}`,
            'Items': order.items.length,
            'Shipping Method': order.shippingInfo?.carrier || 'N/A',
            'Tracking Number': order.shippingInfo?.trackingNumber || 'N/A'
        }));
    }

    async printInvoices() {
        this.bulkActionsService.generateInvoices(this.selection.selected).subscribe({
            next: (pdfBlob) => {
                window.open(URL.createObjectURL(pdfBlob));
            },
            error: (error) => {
                this.errorService.addError({
                    message: 'Failed to generate invoices: ' + error.message,
                    code: error.code || 'PRINT_ERROR',
                    severity: 'error'
                });
            }
        });
    }

    async printShippingLabels() {
        this.bulkActionsService.generateShippingLabels(this.selection.selected).subscribe({
            next: (pdfBlob) => {
                window.open(URL.createObjectURL(pdfBlob));
            },
            error: (error) => {
                this.errorService.addError({
                    message: 'Failed to generate shipping labels: ' + error.message,
                    code: error.code || 'PRINT_ERROR',
                    severity: 'error'
                });
            }
        });
    }

    async printPackingSlips() {
        this.bulkActionsService.generatePackingSlips(this.selection.selected).subscribe({
            next: (pdfBlob) => {
                window.open(URL.createObjectURL(pdfBlob));
            },
            error: (error) => {
                this.errorService.addError({
                    message: 'Failed to generate packing slips: ' + error.message,
                    code: error.code || 'PRINT_ERROR',
                    severity: 'error'
                });
            }
        });
    }
}