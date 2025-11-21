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
    // Check token expiration on app initialization
    const token = localStorage.getItem('token');
    if (token && isTokenExpired(token)) {
      console.warn('Token expired on app initialization, logging out...');
      this.accountService.logout();
    } else {
      // Load current user if token is valid
      this.accountService.loadCurrentUser();
    }

    const basketId = localStorage.getItem('basket_id');
    if (basketId) {
      this.basketService.getBasket(basketId).subscribe(() => {
        console.log("Bakset Init");
      }, err => console.log(err));
    }
  }
}
