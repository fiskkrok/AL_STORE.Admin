import { AuditableEntity } from "./auditableEntity.model";
import { Money } from "./money.model";
import { OrderItem } from "./orderItem.model";
import { Address } from "./address.model";
import { PaymentStatus } from "./payment-status.enum";
import { OrderStatus } from "./order-status.model";

interface Order extends AuditableEntity {
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
