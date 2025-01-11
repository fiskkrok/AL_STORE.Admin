// src/app/core/models/auth.models.ts

export interface AuthState {
    user: User | null;
    accessToken: string | null;
    isAuthenticated: boolean;
    loading: boolean;
    error: string | null;
    message?: string;
}

export interface User {
    id: string;
    username: string;
    email: string;
    firstName?: string;
    lastName?: string;
    roles: string[];
    permissions: string[];
}

export interface AuthTokens {
    accessToken: string;
    expiresIn: number;
    tokenType: string;
    scope: string;
    profile: User;
}

export interface TokenResponse {
    access_token: string;
    id_token: string;
    expires_in: number;
    token_type: string;
    scope: string;
    profile: User;
}

// Constants for auth configuration
export const AUTH_STORAGE_KEYS = {
    ACCESS_TOKEN: 'auth.access_token',
    USER: 'auth.user',
    EXPIRES_AT: 'auth.expires_at'
} as const;
// Common auth error types
export type AuthError =
    | 'auth/invalid-credentials'
    | 'auth/session-expired'
    | 'auth/network-error'
    | 'auth/unknown-error';

export const AUTH_ERROR_MESSAGES: Record<AuthError, string> = {
    'auth/invalid-credentials': 'Invalid username or password',
    'auth/session-expired': 'Your session has expired. Please login again.',
    'auth/network-error': 'Unable to connect to authentication service',
    'auth/unknown-error': 'An unknown error occurred'
};


// // Constants for your auth configuration
// export const AUTH_CONFIG: AuthConfig = {
//   authority: 'https://localhost:5001',
//   clientId: 'admin-portal',
//   redirectUri: window.location.origin + '/callback',
//   responseType: 'code',
//   scope: 'openid profile email api.full',
// };