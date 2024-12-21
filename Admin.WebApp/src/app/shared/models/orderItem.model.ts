import { Money } from "./money.model";

// Order related interfaces
export interface OrderItem {
    id: string;
    productId: string;
    productName: string;
    quantity: number;
    price: Money;
    variantId?: string;
}
