import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { IUserRoles, ICheckBoxRoleManage } from '../../../shared/modules/roles';
import { RoleService } from '../../../shared/services/role.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-user-roles',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-roles.component.html',
  styleUrl: './user-roles.component.scss',
})
export class UserRolesComponent implements OnInit {
  userId: string | null = null;
  userName: string = '';
  email: string = '';
  roles: ICheckBoxRoleManage[] = [];
  loading = false;
  saving = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private roleService: RoleService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.userId = this.route.snapshot.paramMap.get('id');
    if (this.userId) {
      this.loadUserRoles();
    }
  }

  loadUserRoles(): void {
    if (!this.userId) return;

    this.loading = true;
    this.errorMessage = null;

    this.roleService.getManageUserRoles(this.userId).subscribe({
      next: (data: IUserRoles) => {
        this.userName = data.userName;
        this.email = data.email;
        this.roles = data.roles;
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        if (err.status === 401 || err.status === 403) {
          this.errorMessage = 'You don\'t have permission to access this resource.';
        } else if (err.status === 404) {
          this.errorMessage = 'User not found.';
        } else if (err.status === 0) {
          this.errorMessage = 'Unable to connect to the server. Please check if the API is running.';
        } else {
          this.errorMessage = err.error?.message || 'An unexpected error occurred while loading user roles.';
        }

        this.toastr.error(this.errorMessage ?? "Error to load", 'Load Failed', {
          timeOut: 6000,
          positionClass: 'toast-top-center',
          closeButton: true,
        });
      }
    });
  }

  toggleRole(role: ICheckBoxRoleManage): void {
    role.isSelected = !role.isSelected;
  }

  saveUserRoles(): void {
    if (!this.userId) return;

    this.saving = true;
    this.errorMessage = null;
    this.successMessage = null;

    const userRolesDto: IUserRoles = {
      userId: this.userId,
      userName: this.userName,
      email: this.email,
      roles: this.roles
    };

    this.roleService.updateUserRoles(userRolesDto).subscribe({
      next: () => {
        this.successMessage = 'User roles updated successfully!';
        this.saving = false;
        setTimeout(() => this.successMessage = null, 3000);

        this.toastr.success(this.successMessage, 'Success', {
          timeOut: 3000,
          positionClass: 'toast-top-right',
          progressBar: true,
        });
      },
      error: (err: HttpErrorResponse) => {
        this.saving = false;
        if (err.status === 401 || err.status === 403) {
          this.errorMessage = 'You don\'t have permission to update user roles.';
        } else if (err.status === 404) {
          this.errorMessage = 'User not found.';
        } else {
          this.errorMessage = err.error?.message || 'Failed to update user roles.';
        }

        this.toastr.error(this.errorMessage ?? "Failed to update user roles", 'Save Failed', {
          timeOut: 6000,
          positionClass: 'toast-top-center',
          closeButton: true,
        });
      }
    });
  }

  goBack(): void {
    this.router.navigate(['../../'], { relativeTo: this.route });
  }
}




