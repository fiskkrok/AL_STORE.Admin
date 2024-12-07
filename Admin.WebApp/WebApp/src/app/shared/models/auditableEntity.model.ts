import { UserRef } from "./user-ref.model";

// Base interfaces for common properties
export interface AuditableEntity {
    createdAt: string;
    createdBy: UserRef | null;
    lastModifiedAt: string | null;
    lastModifiedBy: UserRef | null;
}
