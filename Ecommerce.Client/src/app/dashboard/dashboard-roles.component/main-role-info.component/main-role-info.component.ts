import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

import { IRole } from '../../../shared/modules/roles';
import { RoleService } from '../../../shared/services/role.service';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import Swal from 'sweetalert2';
import { ToastrService } from 'ngx-toastr';
import { HasPermissionDirective } from '../../../shared/directives/has-permission.directive';

@Component({
  selector: 'app-main-role-info',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    HasPermissionDirective
  ],
  templateUrl: './main-role-info.component.html',
  styleUrl: './main-role-info.component.scss'
})
export class MainRoleInfoComponent implements OnInit {

  roles: IRole[] = [];
  roleForm!: FormGroup;
  loading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  // Statistics
  superAdminCount = 0;
  adminCount = 0;
  customerCount = 0;

  constructor(
    private rolesService: RoleService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadRoles();
  }

  initForm(): void {
    this.roleForm = this.fb.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(50),
          Validators.pattern(/^[A-Za-z\s]+$/) // Allow spaces in role names
        ]
      ]
    });
  }

  loadRoles(): void {
    this.loading = true;
    this.errorMessage = null;
    this.rolesService.getAllRoles().subscribe({
      next: (roles: IRole[]) => {
        this.roles = roles;
        this.calculateStatistics();
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        if (err.status === 401 || err.status === 403) {
          this.errorMessage = 'You don\'t have permission to access this resource. SuperAdmin role is required.';
        } else if (err.status === 0) {
          this.errorMessage = 'Unable to connect to the server. Please check if the API is running.';
        } else {
          this.errorMessage = err.error?.message || 'An unexpected error occurred while loading roles.';
        }
      }
    });
  }

  calculateStatistics(): void {
    // Reset counts
    this.superAdminCount = 0;
    this.adminCount = 0;
    this.customerCount = 0;

    // Count users in each role
    this.roles.forEach(role => {
      const roleName = role.name.toLowerCase();
      const count = role.userCount || 0;

      if (roleName.includes('superadmin') || roleName.includes('super-admin')) {
        this.superAdminCount += count;
      } else if (roleName.includes('admin')) {
        this.adminCount += count;
      } else if (roleName.includes('customer') || roleName.includes('user')) {
        this.customerCount += count;
      }
    });
  }

  getRoleCount(roleName: string): number {
    switch(roleName.toLowerCase()) {
      case 'superadmin':
        return this.superAdminCount;
      case 'admin':
        return this.adminCount;
      case 'customer':
        return this.customerCount;
      default:
        return 0;
    }
  }

  getRoleInitial(roleName: string): string {
    // Get the first character of the role name
    return roleName ? roleName.charAt(0).toUpperCase() : '?';
  }

  getRoleBadgeClass(roleName: string): string {
    const name = roleName.toLowerCase();

    if (name.includes('superadmin') || name.includes('super-admin')) {
      return 'bg-superadmin';
    } else if (name.includes('admin')) {
      return 'bg-admin';
    } else if (name.includes('customer') || name.includes('user')) {
      return 'bg-customer';
    } else {
      return 'bg-default';
    }
  }

  getRoleDisplayName(roleName: string): string {
    // Format role name for display (e.g., "superadmin" -> "Super Admin")
    if (!roleName) return '';

    // Handle common role name patterns
    if (roleName.toLowerCase().includes('superadmin')) {
      return 'Super Admin';
    } else if (roleName.toLowerCase().includes('admin')) {
      return 'Admin';
    } else if (roleName.toLowerCase().includes('customer')) {
      return 'Customer';
    } else if (roleName.toLowerCase().includes('user')) {
      return 'User';
    }

    // Capitalize first letter of each word
    return roleName.split(' ').map(word =>
      word.charAt(0).toUpperCase() + word.slice(1).toLowerCase()
    ).join(' ');
  }

  addRole(): void {
    if (this.roleForm.invalid) return;

    this.errorMessage = null;
    this.successMessage = null;
    this.loading = true;

    this.rolesService.createRole(this.roleForm.value).subscribe({
      next: () => {
        this.roleForm.reset();
        this.toastr.success('Role created successfully!', 'Success', {
          timeOut: 3000,
          positionClass: 'toast-top-right'
        });
        this.loadRoles();
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        let errorMsg = 'Failed to create role.';
        if (err.status === 401 || err.status === 403) {
          errorMsg = 'You don\'t have permission to create roles.';
        } else if (err.status === 409) {
          errorMsg = 'A role with this name already exists.';
        } else {
          errorMsg = err.error?.message || errorMsg;
        }
        this.toastr.error(errorMsg, 'Error', {
          timeOut: 5000,
          positionClass: 'toast-top-right'
        });
      }
    });
  }

  deleteRole(id: string): void {
    const role = this.roles.find(r => r.id === id);
    const roleName = role?.name || 'this role';

    Swal.fire({
      title: 'Are you sure?',
      text: `Do you want to delete ${roleName}? This action cannot be undone.`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#5624d0',
      cancelButtonColor: '#6a6f73',
      confirmButtonText: 'Yes, delete it!',
      cancelButtonText: 'Cancel'
    }).then((result) => {
      if (result.isConfirmed) {
        this.errorMessage = null;
        this.successMessage = null;

        this.rolesService.deleteRole(id).subscribe({
          next: () => {
            this.toastr.success('Role deleted successfully!', 'Success', {
              timeOut: 3000,
              positionClass: 'toast-top-right'
            });
            this.loadRoles();
          },
          error: (err: HttpErrorResponse) => {
            let errorMsg = 'Failed to delete role.';
            if (err.status === 401 || err.status === 403) {
              errorMsg = 'You don\'t have permission to delete roles.';
            } else if (err.status === 400) {
              errorMsg = 'Cannot delete role: It may have users assigned to it.';
            } else {
              errorMsg = err.error?.message || errorMsg;
            }
            this.toastr.error(errorMsg, 'Error', {
              timeOut: 5000,
              positionClass: 'toast-top-right'
            });
          }
        });
      }
    });
  }

  // Convenience getter for form control
  get name() {
    return this.roleForm.get('name');
  }

  // Format date for display (if your role model has createdAt)
  formatDate(dateString?: string): string {
    if (!dateString) return 'N/A';

    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      });
    } catch {
      return 'Invalid Date';
    }
  }
}

