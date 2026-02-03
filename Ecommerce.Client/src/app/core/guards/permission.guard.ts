import { inject } from '@angular/core';
import { Router, CanActivateFn, ActivatedRouteSnapshot } from '@angular/router';
import { map, take } from 'rxjs';
import { PermissionService } from '../../shared/services/permission.service';
import { AccountService } from '../../account/account-service';

/**
 * Permission guard for route protection.
 * 
 * Usage in routes:
 * ```typescript
 * {
 *   path: 'products/create',
 *   component: ProductCreateComponent,
 *   canActivate: [permissionGuard],
 *   data: { 
 *     permission: 'Permissions.Products.Create' 
 *   }
 * }
 * 
 * // Multiple permissions (ALL required by default)
 * {
 *   path: 'admin/settings',
 *   component: SettingsComponent,
 *   canActivate: [permissionGuard],
 *   data: { 
 *     permissions: ['Permissions.Settings.Read', 'Permissions.Settings.Update'],
 *     mode: 'all' // optional, 'all' is default
 *   }
 * }
 * 
 * // Multiple permissions (ANY required)
 * {
 *   path: 'dashboard',
 *   component: DashboardComponent,
 *   canActivate: [permissionGuard],
 *   data: { 
 *     permissions: ['Permissions.Products.Read', 'Permissions.Orders.Read'],
 *     mode: 'any'
 *   }
 * }
 * ```
 */
export const permissionGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
    const permissionService = inject(PermissionService);
    const accountService = inject(AccountService);
    const router = inject(Router);

    // Check if user is logged in first
    const isLoggedIn = accountService.isLoggedIn();
    if (!isLoggedIn) {
        router.navigate(['/login']);
        return false;
    }

    // Get permission requirements from route data
    const requiredPermission = route.data['permission'] as string | undefined;
    const requiredPermissions = route.data['permissions'] as string[] | undefined;
    const mode = (route.data['mode'] as 'all' | 'any') || 'all';

    // If no permissions specified, allow access
    if (!requiredPermission && !requiredPermissions) {
        return true;
    }

    // Single permission check
    if (requiredPermission) {
        return permissionService.hasPermission(requiredPermission).pipe(
            take(1),
            map(hasPermission => {
                if (!hasPermission) {
                    router.navigate(['/unauthorized']);
                    return false;
                }
                return true;
            })
        );
    }

    // Multiple permissions check
    if (requiredPermissions) {
        const observable = mode === 'all'
            ? permissionService.hasAllPermissions(requiredPermissions)
            : permissionService.hasAnyPermission(requiredPermissions);

        return observable.pipe(
            take(1),
            map(hasPermissions => {
                if (!hasPermissions) {
                    router.navigate(['/unauthorized']);
                    return false;
                }
                return true;
            })
        );
    }

    return false;
};
