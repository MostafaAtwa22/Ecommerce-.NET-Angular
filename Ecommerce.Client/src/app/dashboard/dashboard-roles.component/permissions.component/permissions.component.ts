import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { IRolePermissions, IPermissionCheckbox } from '../../../shared/modules/roles';
import { RoleService } from '../../../shared/services/role.service';

interface ModulePermissions {
  module: string;
  read: IPermissionCheckbox | null;
  create: IPermissionCheckbox | null;
  update: IPermissionCheckbox | null;
  delete: IPermissionCheckbox | null;
}

@Component({
  selector: 'app-permissions',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './permissions.component.html',
  styleUrl: './permissions.component.scss',
})
export class PermissionsComponent implements OnInit {
  roleId: string | null = null;
  roleName: string = '';
  permissions: IPermissionCheckbox[] = [];
  modulePermissions: ModulePermissions[] = [];
  loading = false;
  saving = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private roleService: RoleService
  ) {}

  ngOnInit(): void {
    this.roleId = this.route.snapshot.paramMap.get('id');
    if (this.roleId) {
      this.loadPermissions();
    }
  }

  loadPermissions(): void {
    if (!this.roleId) return;

    this.loading = true;
    this.errorMessage = null;

    this.roleService.getManagePermissions(this.roleId).subscribe({
      next: (data: IRolePermissions) => {
        this.roleName = data.roleName;
        this.permissions = data.permissions;
        this.groupPermissionsByModule();
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        if (err.status === 401 || err.status === 403) {
          this.errorMessage = 'You don\'t have permission to access this resource.';
        } else if (err.status === 404) {
          this.errorMessage = 'Role not found.';
        } else if (err.status === 0) {
          this.errorMessage = 'Unable to connect to the server. Please check if the API is running.';
        } else {
          this.errorMessage = err.error?.message || 'An unexpected error occurred while loading permissions.';
        }
      }
    });
  }

  groupPermissionsByModule(): void {
    const moduleMap = new Map<string, ModulePermissions>();

    this.permissions.forEach(permission => {
      if (!moduleMap.has(permission.module)) {
        moduleMap.set(permission.module, {
          module: permission.module,
          read: null,
          create: null,
          update: null,
          delete: null
        });
      }

      const modulePerms = moduleMap.get(permission.module)!;
      const action = permission.action.toLowerCase();

      if (action === 'read') {
        modulePerms.read = permission;
      } else if (action === 'create') {
        modulePerms.create = permission;
      } else if (action === 'update') {
        modulePerms.update = permission;
      } else if (action === 'delete') {
        modulePerms.delete = permission;
      }
    });

    this.modulePermissions = Array.from(moduleMap.values()).sort((a, b) =>
      a.module.localeCompare(b.module)
    );
  }

  togglePermission(permission: IPermissionCheckbox | null): void {
    if (permission) {
      permission.isSelected = !permission.isSelected;
    }
  }

  savePermissions(): void {
    if (!this.roleId) return;

    this.saving = true;
    this.errorMessage = null;
    this.successMessage = null;

    this.roleService.updateRolePermissions(this.roleId, this.permissions).subscribe({
      next: () => {
        this.successMessage = 'Permissions updated successfully!';
        this.saving = false;
        setTimeout(() => this.successMessage = null, 3000);
      },
      error: (err: HttpErrorResponse) => {
        this.saving = false;
        if (err.status === 401 || err.status === 403) {
          this.errorMessage = 'You don\'t have permission to update role permissions.';
        } else if (err.status === 404) {
          this.errorMessage = 'Role not found.';
        } else {
          this.errorMessage = err.error?.message || 'Failed to update permissions.';
        }
      }
    });
  }

  goBack(): void {
    this.router.navigate(['../../'], { relativeTo: this.route });
  }
}
