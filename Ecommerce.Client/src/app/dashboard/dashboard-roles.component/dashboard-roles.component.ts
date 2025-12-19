import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

import { IRole } from '../../shared/modules/roles';
import { RoleService } from '../../shared/services/role.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-dashboard-roles',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
  ],
  templateUrl: './dashboard-roles.component.html',
  styleUrl: './dashboard-roles.component.scss'
})
export class DashboardRolesComponent implements OnInit {

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
    private fb: FormBuilder
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

    // Count roles by name (case-insensitive)
    this.roles.forEach(role => {
      const roleName = role.name.toLowerCase();
      
      if (roleName.includes('superadmin') || roleName.includes('super-admin')) {
        this.superAdminCount++;
      } else if (roleName.includes('admin')) {
        this.adminCount++;
      } else if (roleName.includes('customer') || roleName.includes('user')) {
        this.customerCount++;
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
        this.successMessage = 'Role created successfully!';
        this.loadRoles();
        setTimeout(() => this.successMessage = null, 3000);
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        if (err.status === 401 || err.status === 403) {
          this.errorMessage = 'You don\'t have permission to create roles.';
        } else if (err.status === 409) {
          this.errorMessage = 'A role with this name already exists.';
        } else {
          this.errorMessage = err.error?.message || 'Failed to create role.';
        }
      }
    });
  }

  deleteRole(id: string): void {
    if (!confirm('Are you sure you want to delete this role? This action cannot be undone.')) {
      return;
    }

    this.errorMessage = null;
    this.successMessage = null;
    
    this.rolesService.deleteRole(id).subscribe({
      next: () => {
        this.successMessage = 'Role deleted successfully!';
        this.loadRoles();
        setTimeout(() => this.successMessage = null, 3000);
      },
      error: (err: HttpErrorResponse) => {
        if (err.status === 401 || err.status === 403) {
          this.errorMessage = 'You don\'t have permission to delete roles.';
        } else if (err.status === 400) {
          this.errorMessage = 'Cannot delete role: It may have users assigned to it.';
        } else {
          this.errorMessage = err.error?.message || 'Failed to delete role.';
        }
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