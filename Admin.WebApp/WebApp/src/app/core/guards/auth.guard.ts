// auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { map, tap } from 'rxjs';

export const authGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);

    return authService.isAuthenticated().pipe(
        tap(isAuthenticated => {
            if (!isAuthenticated) {
                authService.login();
            }
        }),
        map(isAuthenticated => isAuthenticated)
    );
};

// Optional: Permission-based guard
export const permissionGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const requiredPermission = route.data['permission'];

    if (!requiredPermission) {
        return true;
    }

    return authService.hasPermission(requiredPermission);
};