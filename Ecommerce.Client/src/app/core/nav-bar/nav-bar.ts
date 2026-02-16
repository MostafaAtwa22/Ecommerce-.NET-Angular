import { Component, HostListener, OnInit, inject, computed, effect } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { BasketService } from '../../shared/services/basket-service';
import { Observable } from 'rxjs';
import { IBasket } from '../../shared/modules/basket';
import { AsyncPipe, CommonModule } from '@angular/common';
import { AccountService } from '../../account/account-service';
import { IAccountUser } from '../../shared/modules/accountUser';
import { WishlistService } from '../../wishlist/wishlist-service';
import { IWishList } from '../../shared/modules/wishlist';
import { getDefaultAvatarByGender, resolveUserAvatar } from '../../shared/utils/avatar-utils';
import { HasPermissionDirective } from '../../shared/directives/has-permission.directive';

interface RoleInfo {
  displayName: string;
  icon: string;
  badgeClass: string;
  priority: number;
}

@Component({
  selector: 'app-nav-bar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, AsyncPipe, CommonModule, HasPermissionDirective],
  templateUrl: './nav-bar.html',
  styleUrls: ['./nav-bar.scss'],
})
export class NavBar implements OnInit {
  private basketService = inject(BasketService);
  private accountService = inject(AccountService);
  private wishListService = inject(WishlistService);

  wishList$!: Observable<IWishList | null>;
  basket$!: Observable<IBasket | null>;

  isLoggedIn = computed(() => this.accountService.isLoggedIn());
  currentUser = computed(() => this.accountService.user());
  sections = ['home', 'shop', 'reviews', 'contact'];
  activeSection: string = '';

  // Role configuration
  private roleConfig: Record<string, RoleInfo> = {
    'superadmin': {
      displayName: 'Super Admin',
      icon: 'fa-crown',
      badgeClass: 'badge-superadmin',
      priority: 100
    },
    'admin': {
      displayName: 'Admin',
      icon: 'fa-user-shield',
      badgeClass: 'badge-admin',
      priority: 90
    },
    'moderator': {
      displayName: 'Moderator',
      icon: 'fa-user-check',
      badgeClass: 'badge-moderator',
      priority: 80
    },
    'editor': {
      displayName: 'Editor',
      icon: 'fa-edit',
      badgeClass: 'badge-editor',
      priority: 70
    },
    'customer': {
      displayName: 'Customer',
      icon: 'fa-user',
      badgeClass: 'badge-customer',
      priority: 10
    },
    'vip': {
      displayName: 'VIP',
      icon: 'fa-star',
      badgeClass: 'badge-vip',
      priority: 60
    },
    'premium': {
      displayName: 'Premium',
      icon: 'fa-gem',
      badgeClass: 'badge-premium',
      priority: 50
    }
  };

  constructor() {
    // Debug effect to track user state changes
    effect(() => {
      const user = this.currentUser();
      console.log('Current User State:', user);
      console.log('Roles:', user?.roles);
      console.log('Is Logged In:', this.isLoggedIn());
    });
  }

  ngOnInit(): void {
    this.basket$ = this.basketService.basket$;
    this.wishList$ = this.wishListService.wishList$;
  }

  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.sections.forEach(sectionId => {
      const section = document.getElementById(sectionId);
      const link = document.getElementById(`${sectionId}Link`);

      if (section && link) {
        const rect = section.getBoundingClientRect();
        const inView = rect.top < window.innerHeight / 2 && rect.bottom > 100;

        if (inView) {
          link.classList.add('active');
          this.activeSection = sectionId;
        } else {
          link.classList.remove('active');
        }
      }
    });
  }

  getAvatarUrl(user: IAccountUser | null): string {
    if (!user) {
      return getDefaultAvatarByGender();
    }

    return resolveUserAvatar(user.profilePicture, user.gender);
  }

  setDefaultAvatar(event: Event, gender?: 'Male' | 'Female') {
    const img = event.target as HTMLImageElement;
    img.src = getDefaultAvatarByGender(gender);
  }

  // Role check methods
  hasRole(roleName: string): boolean {
    const user = this.currentUser();
    if (!user?.roles) return false;

    const roleLower = roleName.toLowerCase();
    return user.roles.some(role =>
      role.toLowerCase() === roleLower
    );
  }

  hasAdminRole(): boolean {
    const user = this.currentUser();
    if (!user?.roles) return false;

    return user.roles.some(role => {
      const roleLower = role.toLowerCase();
      return roleLower === 'admin' ||
             roleLower === 'superadmin' ||
             roleLower === 'moderator' ||
             roleLower.includes('admin');
    });
  }

  hasSuperAdminRole(): boolean {
    return this.hasRole('superadmin');
  }

  hasCustomerRole(): boolean {
    return this.hasRole('customer');
  }

  hasEditorRole(): boolean {
    return this.hasRole('editor');
  }

  hasModeratorRole(): boolean {
    return this.hasRole('moderator');
  }

  // Get primary role (highest priority)
  getPrimaryRole(): string | null {
    const user = this.currentUser();
    if (!user?.roles || user.roles.length === 0) return null;

    let highestPriority = -1;
    let primaryRole: string | null = null;

    user.roles.forEach(role => {
      const roleLower = role.toLowerCase();
      const roleConfig = this.roleConfig[roleLower];

      if (roleConfig && roleConfig.priority > highestPriority) {
        highestPriority = roleConfig.priority;
        primaryRole = role;
      }
    });

    return primaryRole || user.roles[0];
  }

  // Get all roles sorted by priority
  getSortedRoles(): string[] {
    const user = this.currentUser();
    if (!user?.roles) return [];

    return [...user.roles].sort((a, b) => {
      const aConfig = this.roleConfig[a.toLowerCase()];
      const bConfig = this.roleConfig[b.toLowerCase()];

      const aPriority = aConfig?.priority || 0;
      const bPriority = bConfig?.priority || 0;

      return bPriority - aPriority; // Descending order
    });
  }

  // Role display helpers
  getRoleDisplayName(role: string): string {
    const roleConfig = this.roleConfig[role.toLowerCase()];
    return roleConfig?.displayName || role;
  }

  getRoleIcon(role: string): string {
    const roleConfig = this.roleConfig[role.toLowerCase()];
    return roleConfig?.icon || 'fa-user';
  }

  getRoleBadgeClass(role: string): string {
    const roleConfig = this.roleConfig[role.toLowerCase()];
    return roleConfig?.badgeClass || 'badge-default';
  }

  // Check if user has any admin role (for showing/hiding cart/wishlist)
  hasAnyAdminRole(): boolean {
    const adminRoles = ['superadmin', 'admin', 'moderator'];
    const user = this.currentUser();

    if (!user?.roles) return false;

    return user.roles.some(role =>
      adminRoles.includes(role.toLowerCase())
    );
  }

  logout() {
    console.log('Logout called');
    this.accountService.logout();
  }
}
