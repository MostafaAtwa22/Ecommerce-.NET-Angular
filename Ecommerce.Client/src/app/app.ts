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

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [NavBar, HomeModule, CoreModule, ShopModule, RouterOutlet, SpinnerComponent],
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})
export class App implements OnInit{
  protected readonly title = signal('Tasaqolli');
  readonly loading;

  constructor(private busyService: BusyService,
    private appRef: ApplicationRef,
    private basketService: BasketService,
    private wishlistService: WishlistService,
    private accountService: AccountService) {
    this.loading = toSignal(this.busyService.loading$, { initialValue: true });

    this.busyService.busy();

    this.appRef.isStable.subscribe((stable) => {
      if (stable) {
        setTimeout(() => this.busyService.idle(), 1500);
      }
    });
  }

  ngOnInit(): void {
    // Debug: Check if refresh token cookie exists
    this.accountService.checkRefreshTokenCookie();

    // Load current user first - this handles token refresh if needed
    this.accountService.loadCurrentUser();

    // Check if we have a valid user before loading basket/wishlist
    setTimeout(() => {
      if (this.accountService.isLoggedIn()) {
        const basketId = localStorage.getItem('basket_id');
        if (basketId) {
          this.basketService.getBasket(basketId).subscribe(
            () => console.log("Basket Init"),
            err => console.log("Basket load error:", err)
          );
        }

        const wishListId = localStorage.getItem('wishlist_id');
        if(wishListId) {
          this.wishlistService.getWishList(wishListId)
            .subscribe(
              () => console.log('Wish list init'),
              err => console.error('Wishlist load error:', err)
            );
        }
      } else {
        console.log('User not logged in, skipping basket/wishlist load');
      }
    }, 1000);
  }
}
