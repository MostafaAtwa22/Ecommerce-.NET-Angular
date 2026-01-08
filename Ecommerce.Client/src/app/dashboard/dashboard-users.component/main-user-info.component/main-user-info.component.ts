import { Component, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IProfile } from '../../../shared/modules/profile';
import { IPagination, Pagination } from '../../../shared/modules/pagination';
import { HttpErrorResponse } from '@angular/common/http';
import { ProfileService } from '../../../shared/services/profile-service';
import { UserParams } from '../../../shared/modules/UserParams ';
import { RouterLink } from '@angular/router';
import { AccountService } from '../../../account/account-service';

@Component({
  selector: 'app-main-user-info',
  standalone: true,
  imports: [CommonModule, FormsModule, DecimalPipe, RouterLink],
  templateUrl: './main-user-info.component.html',
  styleUrl: './main-user-info.component.scss'
})
export class MainUserInfoComponent implements OnInit {
  users: IProfile[] = [];
  loading = false;
  errorMessage: string | null = null;

  // Statistics
  totalUsers = 0;
  maleCount = 0;
  femaleCount = 0;
  customerCount = 0;
  adminCount = 0;
  superAdminCount = 0;

  // Filter/Search
  userParams = new UserParams();
  showFilters = false;

  // Pagination
  pagination: IPagination<IProfile> = {
    data: [],
    pageIndex: 1,
    pageSize: 10,
    totalData: 0,
  };

  // Add local totalPages property
  totalPages = 0;

  // Sort options
  sortOptions = [
    { value: 'name', name: 'Name: A-Z' },
    { value: 'nameDesc', name: 'Name: Z-A' },
    { value: 'newest', name: 'Newest First' },
    { value: 'oldest', name: 'Oldest First' },
    { value: 'email', name: 'Email: A-Z' }
  ];

  // Role filter options
  roleOptions = [
    { value: '', name: 'All Users' },
    { value: 'Customer', name: 'Customer' },
    { value: 'Admin', name: 'Admin' },
    { value: 'SuperAdmin', name: 'Super Admin' }
  ];

  constructor(
    private profileService: ProfileService,
    private accountService: AccountService
  ) {}

  getUserAvatar(user: IProfile): string {
    if (user.profilePicture) {
      return user.profilePicture;
    }

    // Default avatars based on gender
    if (user.gender?.toLowerCase() === 'male') {
      return 'default-male.png';
    } else if (user.gender?.toLowerCase() === 'female') {
      return 'default-female.png';
    }

    // Default neutral avatar
    return 'default-user.png';
  }

  handleImageError(event: Event): void {
    const imgElement = event.target as HTMLImageElement;
    const userGender = this.users.find(u =>
      `${u.firstName} ${u.lastName}` === imgElement.alt
    )?.gender?.toLowerCase();

    if (userGender === 'male') {
      imgElement.src = 'default-male.png';
    } else if (userGender === 'female') {
      imgElement.src = 'default-female.png';
    } else {
      imgElement.src = 'default-user.png';
    }

    // Add a CSS class for broken images
    imgElement.classList.add('broken-image');
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.errorMessage = null;

    this.profileService.getAllUsers(false).subscribe({
      next: (response) => {
        this.pagination = response;
        this.users = response.data;
        this.totalUsers = response.totalData;

        // Calculate totalPages locally
        this.totalPages = this.calculateTotalPages();

        this.calculateStatistics();
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        if (err.status === 401 || err.status === 403) {
          this.errorMessage = 'You don\'t have permission to access users.';
        } else if (err.status === 0) {
          this.errorMessage = 'Unable to connect to the server. Please check if the API is running.';
        } else {
          this.errorMessage = err.error?.message || 'An unexpected error occurred while loading users.';
        }
      }
    });
  }

  calculateTotalPages(): number {
    if (this.totalUsers <= 0 || this.userParams.pageSize <= 0) {
      return 0;
    }
    return Math.ceil(this.totalUsers / this.userParams.pageSize);
  }

  calculateStatistics(): void {
    this.maleCount = 0;
    this.femaleCount = 0;
    this.customerCount = 0;
    this.adminCount = 0;
    this.superAdminCount = 0;

    this.users.forEach(user => {
      // Count gender
      if (user.gender?.toLowerCase() === 'male') {
        this.maleCount++;
      } else if (user.gender?.toLowerCase() === 'female') {
        this.femaleCount++;
      }

      // Count roles
      if (user.roles) {
        user.roles.forEach(role => {
          const roleLower = role.toLowerCase();
          if (roleLower.includes('superadmin')) {
            this.superAdminCount++;
          } else if (roleLower.includes('admin')) {
            this.adminCount++;
          } else if (roleLower.includes('customer') || roleLower.includes('user')) {
            this.customerCount++;
          }
        });
      }
    });
  }

  // Add a helper method for Math.min in template
  getMaxDisplayNumber(): number {
    return Math.min(this.pagination.pageIndex * this.userParams.pageSize, this.totalUsers);
  }

  onSearch(): void {
    this.userParams.pageIndex = 1;
    this.profileService.setUserParams(this.userParams);
    this.loadUsers();
  }

  resetSearch(): void {
    this.userParams.search = '';
    this.userParams.role = '';
    this.userParams.pageIndex = 1;
    this.profileService.resetUserParams();
    this.loadUsers();
  }

  onSortSelected(sort: string): void {
    this.userParams.sort = sort;
    this.userParams.pageIndex = 1;
    this.profileService.setUserParams(this.userParams);
    this.loadUsers();
  }

  onRoleSelected(role: string): void {
    this.userParams.role = role;
    this.userParams.pageIndex = 1;
    this.profileService.setUserParams(this.userParams);
    this.loadUsers();
  }

  onPageChanged(pageIndex: number): void {
    this.userParams.pageIndex = pageIndex;
    this.profileService.setUserParams(this.userParams);
    this.loadUsers();
  }

  toggleFilters(): void {
    this.showFilters = !this.showFilters;
  }

  resetFilters(): void {
    this.resetSearch();
    this.showFilters = false;
  }

  getInitials(firstName: string, lastName: string): string {
    return `${firstName?.charAt(0) || ''}${lastName?.charAt(0) || ''}`.toUpperCase();
  }

  getRoleBadgeClass(role: string): string {
    const roleLower = role.toLowerCase();
    if (roleLower.includes('superadmin')) return 'badge-superadmin';
    if (roleLower.includes('admin')) return 'badge-admin';
    if (roleLower.includes('customer') || roleLower.includes('user')) return 'badge-customer';
    return 'badge-default';
  }

  getGenderIcon(gender: string): string {
    return gender?.toLowerCase() === 'male' ? 'fa-mars' :
           gender?.toLowerCase() === 'female' ? 'fa-venus' : 'fa-genderless';
  }

  getGenderColor(gender: string): string {
    return gender?.toLowerCase() === 'male' ? 'text-primary' :
           gender?.toLowerCase() === 'female' ? 'text-pink' : 'text-muted';
  }

  isSuperAdmin(): boolean {
    const user = this.accountService.user();
    return user?.roles?.some(role => role.toLowerCase() === 'superadmin') || false;
  }

  lockUser(userId: string): void {
    // TODO: Implement lock user API call when endpoint is available
    // this.profileService.lockUser(userId).subscribe({
    //   next: () => {
    //     this.loadUsers();
    //   },
    //   error: (err) => {
    //     console.error('Failed to lock user:', err);
    //   }
    // });
    console.log('Lock user:', userId);
    alert('Lock user functionality - API endpoint needs to be implemented');
  }

  unlockUser(userId: string): void {
    // TODO: Implement unlock user API call when endpoint is available
    // this.profileService.unlockUser(userId).subscribe({
    //   next: () => {
    //     this.loadUsers();
    //   },
    //   error: (err) => {
    //     console.error('Failed to unlock user:', err);
    //   }
    // });
    console.log('Unlock user:', userId);
    alert('Unlock user functionality - API endpoint needs to be implemented');
  }
}

