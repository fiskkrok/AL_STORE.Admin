// src/app/core/services/auth.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, from, of, throwError } from 'rxjs';
import { catchError, map, tap, switchMap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { User, AuthState, AuthError, AUTH_ERROR_MESSAGES } from '../../shared/models/auth.models';
import { UserManager, User as OidcUser, UserManagerSettings } from 'oidc-client-ts';
import { environment } from '../../../environments/environment';
import { ErrorService } from './error.service';
import { LoadingService } from './loading.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly errorService = inject(ErrorService);
  private readonly loadingService = inject(LoadingService);

  private readonly userManager: UserManager;
  private readonly authStateSubject = new BehaviorSubject<AuthState>({
    user: null,
    accessToken: null,
    isAuthenticated: false,
    loading: false,
    error: null
  });

  readonly authState$ = this.authStateSubject.asObservable();
  readonly currentUser$ = this.authState$.pipe(map(state => state.user));
  readonly isAuthenticated$ = this.authState$.pipe(map(state => state.isAuthenticated));

  private refreshTokenTimeout?: number;

  constructor() {
    const settings: UserManagerSettings = {
      authority: environment.auth.authority,
      client_id: environment.auth.clientId,
      redirect_uri: environment.auth.redirectUri,
      response_type: environment.auth.responseType,
      scope: environment.auth.scope,
      filterProtocolClaims: true,
      loadUserInfo: true,
      automaticSilentRenew: true,
      includeIdTokenInSilentRenew: true,
      monitorSession: true
    };

    this.userManager = new UserManager(settings);
    this.init();

    this.setupEventHandlers();
  }

  private init() {
    this.initializeAuth().catch(error => this.handleAuthError(error));
  }

  private async initializeAuth() {
    this.setLoading(true);
    try {
      const user = await this.userManager.getUser();
      if (user?.access_token && user.expires_in !== undefined) {
        this.processUserLogin(user);
        this.startTokenRefreshTimer(user.expires_in);
      }
    } catch (error) {
      this.handleAuthError(error as Error);
    } finally {
      this.setLoading(false);
    }
  }

  private setupEventHandlers() {
    // Handle silent token renewal errors
    this.userManager.events.addSilentRenewError(error => {
      console.error('Silent renew error:', error);
      this.handleAuthError(error);
    });

    // Handle user session expiration
    this.userManager.events.addUserSignedOut(() => {
      this.handleSignOut();
    });
  }

  private processUserLogin(oidcUser: OidcUser) {
    const user: User = {
      id: oidcUser.profile.sub,
      username: oidcUser.profile.preferred_username!,
      email: oidcUser.profile.email!,
      firstName: oidcUser.profile.given_name,
      lastName: oidcUser.profile.family_name,
      roles: (oidcUser.profile['role'] as string[]) || [],
      permissions: (oidcUser.profile['permissions'] as string[]) || []
    };

    this.authStateSubject.next({
      user,
      accessToken: oidcUser.access_token,
      isAuthenticated: true,
      loading: false,
      error: null
    });
  }

  login(): Promise<void> {
    return this.userManager.signinRedirect();
  }

  async completeAuthentication(): Promise<void> {
    this.setLoading(true);
    try {
      const user = await this.userManager.signinRedirectCallback();
      this.processUserLogin(user);
      if (user.expires_in !== undefined) {
        this.startTokenRefreshTimer(user.expires_in);
      }
      await this.router.navigate(['/']);
    } catch (error) {
      this.handleAuthError(error as Error);
      await this.router.navigate(['/login']);
    } finally {
      this.setLoading(false);
    }
  }

  async logout(): Promise<void> {
    this.stopTokenRefreshTimer();
    this.clearAuthState();
    await this.userManager.signoutRedirect();
  }

  private async handleSignOut() {
    this.clearAuthState();
    await this.router.navigate(['/login']);
  }

  private startTokenRefreshTimer(expiresIn: number) {
    this.stopTokenRefreshTimer();

    // Refresh 1 minute before expiration
    const timeout = (expiresIn - 60) * 1000;
    this.refreshTokenTimeout = window.setTimeout(() => {
      this.userManager.signinSilent()
        .then(user => {
          if (user) {
            this.processUserLogin(user);
            if (user.expires_in !== undefined) {
              this.startTokenRefreshTimer(user.expires_in);
            }
          }
        })
        .catch(error => this.handleAuthError(error));
    }, timeout);
  }

  private stopTokenRefreshTimer() {
    if (this.refreshTokenTimeout) {
      window.clearTimeout(this.refreshTokenTimeout);
    }
  }

  // Token refresh handling
  refreshToken(): Observable<boolean> {
    return from(this.userManager.signinSilent()).pipe(
      map(user => {
        if (user) {
          this.processUserLogin(user);
          if (user.expires_in !== undefined) {
            this.startTokenRefreshTimer(user.expires_in);
          }
          return true;
        }
        return false;
      }),
      catchError(error => {
        this.handleAuthError(error);
        return of(false);
      })
    );
  }

  private clearAuthState() {
    this.authStateSubject.next({
      user: null,
      accessToken: null,
      isAuthenticated: false,
      loading: false,
      error: null,
      message: 'You have been signed out'
    });
  }

  private setLoading(loading: boolean, message?: string) {
    const currentState = this.authStateSubject.value;
    this.authStateSubject.next({
      ...currentState,
      loading,
      message
    });

    // Also update global loading state
    if (loading) {
      this.loadingService.show(message);
    } else {
      this.loadingService.hide();
    }
  }

  private handleAuthError(error: Error) {
    console.error('Auth error:', error);

    let errorCode: AuthError = 'auth/unknown-error';
    if (error.message.includes('network')) {
      errorCode = 'auth/network-error';
    } else if (error.message.includes('expired')) {
      errorCode = 'auth/session-expired';
    }

    const errorMessage = AUTH_ERROR_MESSAGES[errorCode];

    this.errorService.addError({
      code: errorCode,
      message: errorMessage,
      severity: 'error'
    });

    this.authStateSubject.next({
      ...this.authStateSubject.value,
      error: errorMessage
    });
  }

  // Permission checking methods
  hasPermission(permission: string): Observable<boolean> {
    return this.currentUser$.pipe(
      map(user => user?.permissions?.includes(permission) ?? false)
    );
  }

  hasRole(role: string): Observable<boolean> {
    return this.currentUser$.pipe(
      map(user => user?.roles?.includes(role) ?? false)
    );
  }

  // Helper methods
  getAccessToken(): Observable<string | null> {
    return from(this.userManager.getUser()).pipe(
      map(user => user?.access_token ?? null)
    );
  }

  getCurrentUserName(): string {
    const user = this.authStateSubject.value.user;
    return user ? `${user.firstName} ${user.lastName}`.trim() : '';
  }
}