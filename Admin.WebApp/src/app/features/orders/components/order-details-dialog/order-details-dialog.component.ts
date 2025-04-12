import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Store } from '@ngrx/store';
import { OrderActions } from 'src/app/store/order/order.actions';
import { StatusBadgeComponent } from 'src/app/shared/components/badges/status-badge.component';
import { AddPaymentDialogComponent } from '../add-payment-dialog/add-payment-dialog.component';
import { Order } from 'src/app/shared/models/order.model';

@Component({
    selector: 'app-order-details-dialog',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatButtonModule,
        MatTabsModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        StatusBadgeComponent,
    ],
    template: `
        <div class="order-details-dialog">
            <div class="dialog-header">
                <h2 mat-dialog-title>Order #{{order.orderNumber}}</h2>
                <app-status-badge [status]="order.status"></app-status-badge>
            </div>
            
            <mat-dialog-content>
                <mat-tab-group>
                    <!-- Order Details Tab -->
                    <mat-tab label="Details">
                        <div class="tab-content">
                            <div class="info-section">
                                <div class="section-header">
                                    <h3>Order Information</h3>
                                    <button mat-button color="primary" (click)="updateStatus()">
                                        Update Status
                                    </button>
                                </div>
                                <div class="info-grid">
                                    <div class="info-item">
                                        <label>Created</label>
                                        <span>{{order.createdAt | date:'medium'}}</span>
                                    </div>
                                    <div class="info-item">
                                        <label>Last Modified</label>
                                        <span>{{order.lastModifiedAt | date:'medium'}}</span>
                                    </div>
                                    <div class="info-item">
                                        <label>Created By</label>
                                        <span>{{order.createdBy?.username || 'Customer'}}</span>
                                    </div>
                                </div>
                            </div>

                            <div class="info-section">
                                <div class="section-header">
                                    <h3>Items</h3>
                                    <button mat-button color="primary" (click)="editItems()">
                                        Edit Items
                                    </button>
                                </div>
                                <table class="items-table">
                                    <thead>
                                        <tr>
                                            <th>SKU</th>
                                            <th>Product</th>
                                            <th>Quantity</th>
                                            <th>Unit Price</th>
                                            <th>Total</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        @for (item of order.items; track item.id) {
                                            <tr>
                                                <td>{{item.sku}}</td>
                                                <td>{{item.productName}}</td>
                                                <td>{{item.quantity}}</td>
                                                <td>{{item.price.amount | currency:item.price.currency}}</td>
                                                <td>{{item.quantity * item.price.amount | currency:item.price.currency}}</td>
                                            </tr>
                                        }
                                    </tbody>
                                    <tfoot>
                                        <tr>
                                            <td colspan="4">Subtotal</td>
                                            <td>{{order.subtotal.amount | currency:order.subtotal.currency}}</td>
                                        </tr>
                                        <tr>
                                            <td colspan="4">Shipping</td>
                                            <td>{{order.shipping.amount | currency:order.shipping.currency}}</td>
                                        </tr>
                                        <tr>
                                            <td colspan="4">Tax</td>
                                            <td>{{order.tax.amount | currency:order.tax.currency}}</td>
                                        </tr>
                                        <tr class="total-row">
                                            <td colspan="4">Total</td>
                                            <td>{{order.total.amount | currency:order.total.currency}}</td>
                                        </tr>
                                    </tfoot>
                                </table>
                            </div>
                        </div>
                    </mat-tab>

                    <!-- Payment Tab -->
                    <mat-tab label="Payment">
                        <div class="tab-content">
                            <div class="info-section">
                                <div class="section-header">
                                    <h3>Payment Information</h3>
                                    <button mat-button color="primary" 
                                            [disabled]="order.paymentStatus === 'paid'"
                                            (click)="addPayment()">
                                        Add Payment
                                    </button>
                                </div>
                                <div class="info-grid">
                                    <div class="info-item">
                                        <label>Status</label>
                                        <app-status-badge [status]="order.paymentStatus"></app-status-badge>
                                    </div>
                                    <div class="info-item">
                                        <label>Method</label>
                                        <span>{{order.paymentMethod}}</span>
                                    </div>
                                    <div class="info-item">
                                        <label>Transaction ID</label>
                                        <span>{{order.payment?.transactionId || 'N/A'}}</span>
                                    </div>
                                </div>

                                @if (order.payment) {
                                    <div class="payment-history">
                                        <h4>Payment History</h4>
                                        <table class="history-table">
                                            <thead>
                                                <tr>
                                                    <th>Date</th>
                                                    <th>Amount</th>
                                                    <th>Status</th>
                                                    <th>Reference</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <tr>
                                                    <td>{{order.payment.date | date:'short'}}</td>
                                                    <td>{{order.payment.amount | currency:order.payment.currency}}</td>
                                                    <td>
                                                        <app-status-badge [status]="order.payment.status">
                                                        </app-status-badge>
                                                    </td>
                                                    <td>{{order.payment.reference}}</td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                }
                            </div>
                        </div>
                    </mat-tab>

                    <!-- Shipping Tab -->
                    <mat-tab label="Shipping">
                        <div class="tab-content">
                            <div class="info-section">
                                <div class="section-header">
                                    <h3>Shipping Information</h3>
                                    <button mat-button color="primary" 
                                            [disabled]="!canUpdateShipping()"
                                            (click)="updateShipping()">
                                        Update Shipping
                                    </button>
                                </div>

                                <div class="addresses-grid">
                                    <div class="address-box">
                                        <h4>Shipping Address</h4>
                                        <div class="address-content">
                                            <p>{{order.shippingAddress.firstName}} {{order.shippingAddress.lastName}}</p>
                                            <p>{{order.shippingAddress.addressLine1}}</p>
                                            <p>{{order.shippingAddress?.addressLine2}}</p>
                                            <p>{{order.shippingAddress.city}}, {{order.shippingAddress.state}} {{order.shippingAddress.postalCode}}</p>
                                            <p>{{order.shippingAddress.country}}</p>
                                            @if (order.shippingAddress.phone) {
                                                <p>{{order.shippingAddress.phone}}</p>
                                            }
                                        </div>
                                    </div>

                                    @if (order.shippingInfo) {
                                        <div class="shipping-details">
                                            <h4>Tracking Information</h4>
                                            <div class="info-grid">
                                                <div class="info-item">
                                                    <label>Carrier</label>
                                                    <span>{{order.shippingInfo.carrier}}</span>
                                                </div>
                                                <div class="info-item">
                                                    <label>Tracking Number</label>
                                                    <span>{{order.shippingInfo.trackingNumber}}</span>
                                                </div>
                                                <div class="info-item">
                                                    <label>Estimated Delivery</label>
                                                    <span>{{order.shippingInfo.estimatedDeliveryDate | date}}</span>
                                                </div>
                                            </div>
                                        </div>
                                    }
                                </div>
                            </div>
                        </div>
                    </mat-tab>

                    <!-- Admin Notes Tab -->
                    <mat-tab label="Notes">
                        <div class="tab-content">
                            <div class="info-section">
                                <div class="section-header">
                                    <h3>Admin Notes</h3>
                                    <button mat-button color="primary" (click)="addNote()">
                                        Add Note
                                    </button>
                                </div>

                                @if (order.notes?.length) {
                                    <div class="notes-list">
                                        @for (note of order.notes; track note.id) {
                                            <div class="note-item">
                                                <div class="note-header">
                                                    <span class="note-author">{{note.createdBy.username}}</span>
                                                    <span class="note-date">{{note.createdAt | date:'short'}}</span>
                                                </div>
                                                <p class="note-content">{{note.content}}</p>
                                            </div>
                                        }
                                    </div>
                                } @else {
                                    <p class="no-notes">No notes added yet</p>
                                }
                            </div>
                        </div>
                    </mat-tab>
                </mat-tab-group>
            </mat-dialog-content>

            <mat-dialog-actions align="end">
                <button mat-button (click)="close()">Close</button>
                @if (order.status !== 'cancelled') {
                    <button mat-button color="warn" (click)="cancelOrder()">
                        Cancel Order
                    </button>
                }
            </mat-dialog-actions>
        </div>
    `,
    styles: [`
        .dialog-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 1rem;
        }

        .section-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 1rem;
        }

        .tab-content {
            padding: 1rem;
        }

        .info-section {
            margin-bottom: 2rem;

            h3 {
                margin: 0;
                color: var(--text-primary);
            }
        }

        .info-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 1rem;
            margin-top: 1rem;
        }

        .info-item {
            label {
                display: block;
                font-size: 0.875rem;
                color: var(--text-secondary);
                margin-bottom: 0.25rem;
            }
        }

        .items-table, .history-table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 1rem;

            th, td {
                padding: 0.75rem;
                text-align: left;
                border-bottom: 1px solid var(--border);
            }

            th {
                font-weight: 500;
                color: var(--text-secondary);
            }

            .total-row {
                font-weight: 500;
                td {
                    border-top: 2px solid var(--border);
                }
            }
        }

        .addresses-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 1rem;
            margin-top: 1rem;
        }

        .address-box {
            padding: 1rem;
            background-color: var(--bg-secondary);
            border-radius: 8px;
            border: 1px solid var(--border);

            h4 {
                margin: 0 0 0.5rem;
                color: var(--text-primary);
            }

            p {
                margin: 0.25rem 0;
                color: var(--text-secondary);
            }
        }

        .notes-list {
            margin-top: 1rem;

            .note-item {
                padding: 1rem;
                background-color: var(--bg-secondary);
                border-radius: 8px;
                border: 1px solid var(--border);
                margin-bottom: 1rem;

                .note-header {
                    display: flex;
                    justify-content: space-between;
                    margin-bottom: 0.5rem;
                    font-size: 0.875rem;
                    color: var(--text-secondary);
                }

                .note-content {
                    margin: 0;
                    color: var(--text-primary);
                }
            }
        }

        .no-notes {
            color: var(--text-secondary);
            font-style: italic;
            margin: 1rem 0;
        }
    `]
})
export class OrderDetailsDialogComponent {
    dialog: any;
    dialogService: any;
    constructor(
        public dialogRef: MatDialogRef<OrderDetailsDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public order: Order,
        private readonly store: Store,
        private readonly fb: FormBuilder
    ) { }

    canUpdateShipping(): boolean {
        return ['confirmed', 'processing'].includes(this.order.status);
    }

    updateStatus() {
        // Implement status update dialog
    }

    editItems() {
        // Implement items edit dialog
    }

    addPayment() {
        // Open payment dialog to add new payment
        const dialogRef = this.dialog.open(AddPaymentDialogComponent, {
            data: {
                orderId: this.order.id,
                amount: this.order.total.amount,
                currency: this.order.total.currency
            }
        });

        dialogRef.afterClosed().subscribe((payment: any) => {
            if (payment) {
                this.store.dispatch(OrderActions.addPayment({
                    orderId: this.order.id,
                    payment
                }));
            }
        });
    }

    updateShipping() {
        const dialogRef = this.dialog.open(UpdateShippingDialogComponent, {
            data: {
                orderId: this.order.id,
                currentShipping: this.order.shippingInfo
            }
        });

        dialogRef.afterClosed().subscribe(shipping => {
            if (shipping) {
                this.store.dispatch(OrderActions.updateShipping({
                    orderId: this.order.id,
                    shipping
                }));
            }
        });
    }

    addNote() {
        const dialogRef = this.dialog.open(AddNoteDialogComponent);

        dialogRef.afterClosed().subscribe(note => {
            if (note) {
                this.store.dispatch(OrderActions.addNote({
                    orderId: this.order.id,
                    note
                }));
            }
        });
    }

    cancelOrder() {
        this.dialogService.confirm(
            'Are you sure you want to cancel this order?',
            'Cancel Order'
        ).then((confirmed: any) => {
            if (confirmed) {
                this.store.dispatch(OrderActions.updateStatus({
                    orderId: this.order.id,
                    newStatus: OrderStatus.Cancelled
                }));
                this.close();
            }
        });
    }

    close() {
        this.dialogRef.close();
    }
}