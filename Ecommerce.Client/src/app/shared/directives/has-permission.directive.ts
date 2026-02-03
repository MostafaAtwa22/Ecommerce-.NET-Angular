import {
    Directive,
    Input,
    TemplateRef,
    ViewContainerRef,
    OnInit,
    OnDestroy
} from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { PermissionService } from '../services/permission.service';

/**
 * Structural directive to show/hide elements based on user permissions.
 * 
 * Usage:
 * ```html
 * <!-- Single permission -->
 * <button *hasPermission="'Permissions.Products.Create'">Add Product</button>
 * 
 * <!-- Multiple permissions (ALL required) -->
 * <button *hasPermission="['Permissions.Products.Create', 'Permissions.Products.Update']; mode: 'all'">
 *   Edit Product
 * </button>
 * 
 * <!-- Multiple permissions (ANY required) -->
 * <div *hasPermission="['Permissions.Products.Read', 'Permissions.Orders.Read']; mode: 'any'">
 *   Dashboard Content
 * </div>
 * 
 * <!-- Show element when user does NOT have permission -->
 * <div *hasPermission="'Permissions.Products.Delete'; else: true">
 *   You don't have delete permission
 * </div>
 * ```
 */
@Directive({
    selector: '[hasPermission]',
    standalone: true
})
export class HasPermissionDirective implements OnInit, OnDestroy {
    private destroy$ = new Subject<void>();
    private permissions: string[] = [];
    private mode: 'all' | 'any' = 'all';
    private else: boolean = false;

    @Input() set hasPermission(value: string | string[]) {
        this.permissions = Array.isArray(value) ? value : [value];
        this.updateView();
    }

    @Input() set hasPermissionMode(value: 'all' | 'any') {
        this.mode = value;
        this.updateView();
    }

    @Input() set hasPermissionElse(value: boolean) {
        this.else = value;
        this.updateView();
    }

    constructor(
        private templateRef: TemplateRef<any>,
        private viewContainer: ViewContainerRef,
        private permissionService: PermissionService
    ) { }

    ngOnInit(): void {
        this.permissionService.permissions$
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => {
                this.updateView();
            });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private updateView(): void {
        const hasRequiredPermissions = this.checkPermissions();
        const shouldShow = this.else ? !hasRequiredPermissions : hasRequiredPermissions;

        if (shouldShow) {
            this.viewContainer.createEmbeddedView(this.templateRef);
        } else {
            this.viewContainer.clear();
        }
    }

    private checkPermissions(): boolean {
        if (this.permissions.length === 0) {
            return false;
        }

        const userPermissions = this.permissionService.getCurrentPermissions();

        if (this.mode === 'all') {
            return this.permissions.every(p => userPermissions.includes(p));
        } else {
            return this.permissions.some(p => userPermissions.includes(p));
        }
    }
}
