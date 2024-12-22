import { UserRef } from "./user.model";

// Base interfaces for common properties
export interface AuditableEntity {
    createdAt: string;
    createdBy: UserRef | null;
    lastModifiedAt: string | null;
    lastModifiedBy: UserRef | null;
}
