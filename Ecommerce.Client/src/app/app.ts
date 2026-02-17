import { Component, ApplicationRef, signal, OnInit } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavBar } from "./core/nav-bar/nav-bar";
import { HomeModule } from './home/home-module';
import { CoreModule } from './core/core-module';
import { ShopModule } from './shop/shop-module';
import { RouterOutlet } from '@angular/router';
import { BusyService } from './shared/services/busy-service';
import { SpinnerComponent } from './shared/components/spinner-component/spinner-component';
import { BasketService } from './shared/services/basket-service';
import { AccountService } from './account/account-service';
import { WishlistService } from './wishlist/wishlist-service';
import { ChatbotWidgetComponent } from './shared/components/chatbot-widget/chatbot-widget';
import { take } from 'rxjs';
import { Router } from '@angular/router';
import { PermissionService } from './shared/services/permission.service';
import { Permissions } from './core/constants/Permissions';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [NavBar, HomeModule, CoreModule, ShopModule, RouterOutlet, SpinnerComponent, ChatbotWidgetComponent],
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})
export class App implements OnInit {
  protected readonly title = signal('Tasaqolli');
  readonly loading;

  constructor(
    private busyService: BusyService,
    private appRef: ApplicationRef,
    private basketService: BasketService,
    private wishlistService: WishlistService,
    private accountService: AccountService,
    private permissionService: PermissionService,
    private router: Router
  ) {
    this.loading = toSignal(this.busyService.loading$, { initialValue: true });
  }

  ngOnInit(): void {
    // Initialize app stable state and hide loader
    this.busyService.busy();
    this.appRef.isStable.subscribe(stable => {
      if (stable) setTimeout(() => this.busyService.idle(), 1500);
    });

    // Load current user (refresh handled by interceptor if needed)
    this.accountService.loadCurrentUser();

    const currentUrl = this.router.url;
    const canAutoRedirect = currentUrl === '/' || currentUrl === '/home' || currentUrl.startsWith('/home?');

    if (canAutoRedirect && this.accountService.isLoggedIn()) {
      const dashboardPermissions = [
        Permissions.Account_Read,
        Permissions.Roles_Read,
        Permissions.Roles_Update,
        Permissions.Products_Create,
        Permissions.Products_Update,
        Permissions.Products_Delete,
        Permissions.DeliveryMethods_Create,
        Permissions.DeliveryMethods_Update,
        Permissions.DeliveryMethods_Delete
      ];

      this.permissionService.fetchPermissions().pipe(take(1)).subscribe(perms => {
        const hasAny = dashboardPermissions.some(p => perms.includes(p));
        if (hasAny) {
          this.router.navigateByUrl('/dashboard');
        }
      });
    }

    // Initialize basket
    const basketId = localStorage.getItem('basket_id');
    if (basketId) {
      this.basketService.getBasket(basketId).pipe(take(1)).subscribe({
        next: () => console.log("Basket initialized"),
        error: err => console.error(err)
      });
    }

    // Initialize wishlist
    const wishListId = localStorage.getItem('wishlist_id');
    if (wishListId) {
      this.wishlistService.getWishList(wishListId).pipe(take(1)).subscribe({
        next: () => console.log("Wishlist initialized"),
        error: err => console.error(err)
      });
    }
  }
}
