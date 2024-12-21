import { Address } from "./address.model";
import { AuditableEntity } from "./auditableEntity.model";

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
