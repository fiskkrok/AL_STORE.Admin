import { AuditableEntity } from "../auditableEntity.model";
import { Address, Money } from "../valueobjects/value-object.model";

export interface Order extends AuditableEntity {
    payment: any;
    id: string;
    orderNumber: string;
    customerId: string;
    status: OrderStatus;
    items: OrderItem[];
    subtotal: Money;
    tax: Money;
    shipping: Money;
    total: Money;
    shippingAddress: Address;
    billingAddress: Address;
    paymentStatus: PaymentStatus;
    paymentMethod: string;
    notes?: string;
}

export enum OrderStatus {
    Pending = 'pending',
    Confirmed = 'confirmed',
    Processing = 'processing',
    Shipped = 'shipped',
    Delivered = 'delivered',
    Cancelled = 'cancelled',
    Refunded = 'refunded'
}

export interface OrderItem {
    sku: any;
    id: string;
    productId: string;
    productName: string;
    quantity: number;
    price: Money;
    variantId?: string;
}

export enum PaymentStatus {
    Pending = 'pending',
    Authorized = 'authorized',
    Paid = 'paid',
    Failed = 'failed',
    Refunded = 'refunded'
}

