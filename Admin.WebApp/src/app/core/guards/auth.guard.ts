// src/app/core/guards/auth.guards.ts
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import {
    CanActivateFn,
    CanMatchFn,
    Route,
    UrlSegment
} from '@angular/router';
import { Observable, map, tap } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { environment } from 'src/environments/environment';

// Basic auth guard
export const authGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    return authService.isAuthenticated$.pipe(
        tap(isAuthenticated => {
            if (!isAuthenticated) {
                // Store attempted URL for redirect after login
                const returnUrl = state.url;
                router.navigate(['/login'], {
                    queryParams: { returnUrl }
                });
            }
        })
    );
};

// Permission-based guard
export const permissionGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const requiredPermission = route.data['permission'];

    if (!requiredPermission) {
        return true;
    }

    return authService.hasPermission(requiredPermission).pipe(
        tap(hasPermission => {
            if (!hasPermission) {
                router.navigate(['/unauthorized']);
            }
        })
    );
};

// Role-based guard
export const roleGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const requiredRole = route.data['role'];

    if (!requiredRole) {
        return true;
    }

    return authService.hasRole(requiredRole).pipe(
        tap(hasRole => {
            if (!hasRole) {
                router.navigate(['/unauthorized']);
            }
        })
    );
};

// Combined guard for routes that need both authentication and permissions
export const authenticatedWithPermissionGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const requiredPermission = route.data['permission'];

    return new Observable<boolean>(observer => {
        authService.isAuthenticated$.pipe(
            tap(isAuthenticated => {
                if (!isAuthenticated) {
                    router.navigate(['/login'], {
                        queryParams: { returnUrl: state.url }
                    });
                    observer.next(false);
                    observer.complete();
                }
            }),
            map(isAuthenticated => {
                if (!isAuthenticated) return false;
                if (!requiredPermission) return true;

                return authService.hasPermission(requiredPermission).pipe(
                    tap(hasPermission => {
                        if (!hasPermission) {
                            router.navigate(['/unauthorized']);
                        }
                    })
                );
            })
        ).subscribe(result => {
            observer.next(result as boolean);
            observer.complete();
        });
    });
};

// Route matcher guard for feature flags or complex conditions
export const routeMatchGuard: CanMatchFn = (route: Route, segments: UrlSegment[]) => {
    const authService = inject(AuthService);

    // Example of complex route matching logic
    return new Observable<boolean>(observer => {
        // Check authentication
        authService.isAuthenticated$.pipe(
            map(isAuthenticated => {
                if (!isAuthenticated) return false;

                // Check feature flag in route data
                const featureFlag = route.data?.['featureFlag'] as keyof typeof environment.features;
                if (featureFlag && !environment.features[featureFlag]) {
                    return false;
                }

                // Check required permissions
                const permissions = route.data?.['permissions'] as string[];
                if (permissions?.length) {
                    return permissions.every(permission =>
                        authService.hasPermission(permission)
                    );
                }

                return true;
            })
        ).subscribe(result => {
            observer.next(result);
            observer.complete();
        });
    });
};