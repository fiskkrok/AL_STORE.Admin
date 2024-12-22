import { AuditableEntity } from "./auditableEntity.model";
import { Address } from "./valueobjects/value-object.model";

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
    // status: CustomerStatus;
    notes?: string;
    tags?: string[];
}

enum CustomerStatus {
    Active = 'active',
    Inactive = 'inactive',
    Blocked = 'blocked'
}