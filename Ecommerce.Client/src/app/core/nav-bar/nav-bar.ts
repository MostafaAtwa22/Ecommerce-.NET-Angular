// Add this to your NavBar component temporarily for debugging

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

@Component({
  selector: 'app-nav-bar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, AsyncPipe, CommonModule],
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

  constructor() {
    // Debug effect to track user state changes
    effect(() => {
      const user = this.currentUser();
      console.log('Current User State:', user);
      console.log('Is Logged In:', this.isLoggedIn());
      console.log('LocalStorage token:', localStorage.getItem('token'));
      console.log('LocalStorage user:', localStorage.getItem('user'));
    });
  }

  ngOnInit(): void {
    this.basket$ = this.basketService.basket$;
    this.wishList$ = this.wishListService.wishList$;
    // Check initial state
    console.log('NavBar initialized');
    console.log('User on init:', this.currentUser());
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
    if (!user) return 'default-avatar.png';
    if (user.profilePicture) return user.profilePicture;

    return user.gender === 'Male' ? 'default-male.png' : 'default-female.png';
  }

  setDefaultAvatar(event: Event, gender?: 'Male' | 'Female') {
    const img = event.target as HTMLImageElement;
    img.src = gender === 'Male' ? 'default-male.png' : 'default-female.png';
  }

  isAdmin(): boolean {
    const user = this.currentUser();
    return user?.roles?.some(role => 
      role.toLowerCase() === 'admin' || role.toLowerCase() === 'superadmin'
    ) || false;
  }

  isSuperAdmin(): boolean {
    const user = this.currentUser();
    return user?.roles?.some(role => role.toLowerCase() === 'superadmin') || false;
  }

  isCustomer(): boolean {
    const user = this.currentUser();
    if (!user?.roles || user.roles.length === 0) return true;
    return user.roles.every(role => role.toLowerCase() === 'customer');
  }

  logout() {
    console.log('Logout called');
    this.accountService.logout();
  }
}
