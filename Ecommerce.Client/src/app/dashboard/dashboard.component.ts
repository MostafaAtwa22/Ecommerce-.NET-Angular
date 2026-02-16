import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AccountService } from '../account/account-service';
import { HasPermissionDirective } from '../shared/directives/has-permission.directive';

@Component({
  selector: 'app-dashboard.component',
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet, HasPermissionDirective],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  isSidebarActive = false;
  private accountService = inject(AccountService);

  toggleSidebar() {
    this.isSidebarActive = !this.isSidebarActive;
  }

  isSuperAdmin(): boolean {
    const user = this.accountService.user();
    return user?.roles?.some(role => role.toLowerCase() === 'superadmin' || role.toLowerCase() === 'admin' ) || false;
  }

  isAdminOrSuperAdmin(): boolean {
    const user = this.accountService.user();
    return user?.roles?.some(role => {
      const r = role.toLowerCase();
      return r === 'superadmin' || r === 'admin' || r.includes('admin');
    }) || false;
  }

  logout() {
    console.log('Logout called');
    this.accountService.logout();
  }
}

