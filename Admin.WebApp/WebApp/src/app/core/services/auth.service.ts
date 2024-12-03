// auth.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, from, map, tap } from 'rxjs';
import { User } from '../models/auth.models';
import { Router } from '@angular/router';
import { UserManager, User as OidcUser, UserManagerSettings } from 'oidc-client-ts';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly userManager: UserManager;
  private readonly currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

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
      includeIdTokenInSilentRenew: true
    };

    this.userManager = new UserManager(settings);
    this.initializeAuth();
  }

  ngOnInit(): void {
    this.initializeAuth();
  }

  private async initializeAuth() {
    try {
      const user = await this.userManager.getUser();
      if (user?.access_token) {
        this.processUserLogin(user);
      }
    } catch (error) {
      console.error('Error during auth initialization:', error);
    }
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
    // console.log('Setting currentUser:', user); // Debug log
    this.currentUserSubject.next(user);
  }

  login(): Promise<void> {
    return this.userManager.signinRedirect();
  }

  async completeAuthentication(): Promise<void> {
    try {
      const user = await this.userManager.signinRedirectCallback();
      this.processUserLogin(user);
      this.router.navigate(['/']);
    } catch (error) {
      console.error('Error completing authentication:', error);
      this.router.navigate(['/login']);
    }
  }

  logout(): Promise<void> {
    this.currentUserSubject.next(null);
    return this.userManager.signoutRedirect();
  }

  isAuthenticated(): Observable<boolean> {
    return from(this.userManager.getUser()).pipe(
      map(user => !!user && !user.expired)
    );
  }

  getAccessToken(): Observable<string | null> {
    return from(this.userManager.getUser()).pipe(
      // tap(user => console.log('User in getAccessToken:', user)), // Debug log
      map(user => {
        // console.log('Access token:', user?.access_token); // Debug log
        return user?.access_token ?? null;
      })
    );
  }

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

  getCurrentUserName(): string {
    const user = this.currentUserSubject.value;
    return user ? `${user.firstName} ${user.lastName}` : '';

  }
}