import { Money } from "./money.model";
import { ProductAttribute } from "./product-attribute.model";

export interface ProductVariant {
    id: string;
    sku: string;
    price: Money;
    stock: number;
    attributes: ProductAttribute[];
}
