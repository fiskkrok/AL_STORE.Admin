import { AuditableEntity } from "./auditableEntity.model";
import { Money, Address } from "./value-object.model";


// Admin.WebApp/src/app/shared/models/order.model.ts
export interface Order {
    id: string;
    orderNumber: string;
    customerId: string;
    status: OrderStatus; // Enum mapping to backend string
    subtotal: number;
    shippingCost: number;
    tax: number;
    total: number;
    currency: string;
    shippingAddress: Address;
    billingAddress: Address;
    notes?: string;
    cancelledAt?: string; // ISO date string
    cancellationReason?: string;
    items: OrderItem[];
    payment?: Payment;
    paymentStatus: PaymentStatus; // Enum mapping to backend string
    paymentMethod: string;
    shippingInfo?: ShippingInfo;
    createdAt: string; // ISO date string
    createdBy?: string;
    lastModifiedAt?: string; // ISO date string
    lastModifiedBy?: string;
}
export interface OrderItem {
    id: string;
    productId: string;
    productName: string;
    sku: string;
    variantId?: string;
    quantity: number;
    unitPrice: number;
    currency: string;
    total: number;
}
export enum OrderStatus {
    Pending = 'pending',
    Confirmed = 'confirmed',
    Processing = 'processing',
    Shipped = 'shipped',
    Delivered = 'delivered',
    Cancelled = 'cancelled'
}

export enum PaymentStatus {
    Pending = 'pending',
    Authorized = 'authorized',
    Paid = 'paid',
    Failed = 'failed',
    Refunded = 'refunded'
}

export interface Payment {
    transactionId: string;
    method: string;
    amount: number;
    currency: string;
    status: PaymentStatus;
    processedAt: string; // ISO date string
}
export interface ShippingInfo {
    carrier: string;
    trackingNumber: string;
    estimatedDeliveryDate: string; // ISO date string
    actualDeliveryDate?: string; // ISO date string
}

