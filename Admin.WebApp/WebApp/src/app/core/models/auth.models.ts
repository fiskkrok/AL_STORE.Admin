// auth.models.ts
export interface User {
  id: string;
  username: string;
  email: string;
  firstName?: string;
  lastName?: string;
  roles: string[];
  permissions: string[];
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface AuthResponse {
  access_token: string;
  id_token: string;
  expires_in: number;
  token_type: string;
  scope: string;
  profile: User;
}

export interface AuthConfig {
  authority: string;
  clientId: string;
  redirectUri: string;
  responseType: string;
  scope: string;
}

// // Constants for your auth configuration
// export const AUTH_CONFIG: AuthConfig = {
//   authority: 'https://localhost:5001',
//   clientId: 'admin-portal',
//   redirectUri: window.location.origin + '/callback',
//   responseType: 'code',
//   scope: 'openid profile email api.full',
// };
