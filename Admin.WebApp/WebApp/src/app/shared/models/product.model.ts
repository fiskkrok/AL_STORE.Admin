// Base interfaces for common properties
interface AuditableEntity {
    createdAt: string;
    createdBy: UserRef | null;
    lastModifiedAt: string | null;
    lastModifiedBy: UserRef | null;
}

interface UserRef {
    id: string;
    username: string;
    email: string;
}

// Product related interfaces
export interface Category {
    id: string;
    name: string;
    description: string;
    parentId?: string;
    slug: string;
    isActive: boolean;
}

interface SubCategory extends Omit<Category, 'parentId'> {
    parentCategoryId: string;
}

export interface ProductImage {
    id: string;
    url: string;
    fileName: string;
    size: number;
    isPrimary: boolean;
    sortOrder: number;
    alt?: string;
}

export interface ProductVariant {
    id: string;
    sku: string;
    price: Money;
    stock: number;
    attributes: ProductAttribute[];
}

export interface ProductAttribute {
    name: string;
    value: string;
    type: 'color' | 'size' | 'material' | 'style' | string;
}

export interface Money {
    amount: number;
    currency: string;
}

export interface Product extends AuditableEntity {
    id: string;
    name: string;
    slug: string;
    description: string;
    shortDescription?: string;
    sku: string;
    barcode?: string;

    price: Money;
    compareAtPrice?: Money;

    category: Category;
    subCategory?: SubCategory;

    stock: number;
    lowStockThreshold?: number;

    status: ProductStatus;
    visibility: ProductVisibility;

    images: ProductImage[];
    variants?: ProductVariant[];
    attributes?: ProductAttribute[];

    seo?: {
        title?: string;
        description?: string;
        keywords?: string[];
    };

    dimensions?: {
        weight: number;
        width: number;
        height: number;
        length: number;
        unit: 'cm' | 'inch';
    };

    tags: string[];
    isArchived: boolean;
}

export enum ProductStatus {
    Draft = 'draft',
    Active = 'active',
    OutOfStock = 'out_of_stock',
    Discontinued = 'discontinued'
}

export enum ProductVisibility {
    Visible = 'visible',
    Hidden = 'hidden',
    Featured = 'featured'
}

// Order related interfaces
interface OrderItem {
    id: string;
    productId: string;
    productName: string;
    quantity: number;
    price: Money;
    variantId?: string;
}

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

enum OrderStatus {
    Pending = 'pending',
    Confirmed = 'confirmed',
    Processing = 'processing',
    Shipped = 'shipped',
    Delivered = 'delivered',
    Cancelled = 'cancelled',
    Refunded = 'refunded'
}

enum PaymentStatus {
    Pending = 'pending',
    Authorized = 'authorized',
    Paid = 'paid',
    Failed = 'failed',
    Refunded = 'refunded'
}

interface Address {
    firstName: string;
    lastName: string;
    addressLine1: string;
    addressLine2?: string;
    city: string;
    state: string;
    postalCode: string;
    country: string;
    phone?: string;
}

// Customer related interfaces
interface Customer extends AuditableEntity {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    phone?: string;
    addresses: Address[];
    defaultBillingAddressId?: string;
    defaultShippingAddressId?: string;
    status: CustomerStatus;
    notes?: string;
    tags?: string[];
}

enum CustomerStatus {
    Active = 'active',
    Inactive = 'inactive',
    Blocked = 'blocked'
}
