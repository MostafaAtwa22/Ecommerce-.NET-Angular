import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AccountService } from '../account/account-service';

@Component({
  selector: 'app-dashboard.component',
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
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
    return user?.roles?.some(role => role.toLowerCase() === 'superadmin') || false;
  }

  logout() {
    console.log('Logout called');
    this.accountService.logout();
  }
}

