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
import { isTokenExpired } from './shared/utils/token-utils';
import { WishlistService } from './wishlist/wishlist-service';
import { take } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [NavBar, HomeModule, CoreModule, ShopModule, RouterOutlet, SpinnerComponent],
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
    private accountService: AccountService
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
