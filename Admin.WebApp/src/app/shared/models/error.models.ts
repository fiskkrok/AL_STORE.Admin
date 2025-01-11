// src/app/core/models/error.models.ts
export interface ApiError {
    id: string;
    code: string;
    message: string;
    details?: string;
    timestamp: Date;
    severity: 'error' | 'warning' | 'info';
    field?: string;  // For validation errors
    correlationId?: string;  // For tracking error chains
}

export type ValidationError = {
    field: string;
    message: string;
    code: string;
}