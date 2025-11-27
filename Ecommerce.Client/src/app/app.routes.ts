import { Routes } from '@angular/router';
import { HomeComponent } from './home/home-component';
import { NotFoundComponent } from './core/not-found-component/not-found-component';
import { BasketComponent } from './basket/basket-component';
import { authGuard } from './core/guards/auth-guard';
import { loginGuard } from './core/guards/login-guard';
import { registerGuard } from './core/guards/register-guard';
import { OrderDetailsComponent } from './orders/order-details-component/order-details-component';
import { OrdersComponent } from './orders/orders-component';
import { WishlistComponent } from './wishlist/wishlist.component';
export const routes: Routes = [
  {path: '', redirectTo:'home', pathMatch:'full'},
  {path: 'home', component: HomeComponent},
  {
    path: 'checkout',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./checkout/checkout-component').then(m => m.CheckoutComponent)
  },
  {
    path: 'success',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./checkout/checkout-success-component/checkout-success-component')
        .then(m => m.CheckoutSuccessComponent)
  },
  {
    path: 'shop',
    loadComponent: () =>
      import('./shop/shop-component').then(m => m.ShopComponent)
  },
  {
    path: 'shop/:id',
    loadComponent: () =>
      import('./shop/product-details-component/product-details-component')
      .then(s => s.ProductDetailsComponent)
  },
  {
    path: 'basket',
    loadComponent: () =>
      import('./basket/basket-component')
      .then(s => BasketComponent)
  },
  {
    path: 'wishlist',
    loadComponent: () =>
      import('./wishlist/wishlist.component')
      .then(s => WishlistComponent)
  },
  {
    path: 'orders',
    loadComponent: () =>
      import('./orders/orders-component')
      .then(s => OrdersComponent)
  },
  {
    path: 'orders/:id',
    loadComponent: () =>
      import('./orders/order-details-component/order-details-component')
      .then(s => OrderDetailsComponent)
  },
  {
    path: 'server-error',
    loadComponent: () =>
      import('./core/server-error-component/server-error-component')
      .then(s => s.ServerErrorComponent)
  },
  {
    path: 'register',
    canActivate: [registerGuard],
    loadComponent: () =>
      import('./account/register-component/register-component')
      .then(s => s.RegisterComponent)
  },
  {
    path: 'login',
    canActivate: [loginGuard],
    loadComponent: () =>
      import('./account/login-component/login-component')
      .then(s => s.LoginComponent)
  },
  {
    path: 'profile',
    loadComponent: () =>
      import('./profile/profile.component')
      .then(p => p.ProfileComponent)
  },
  {path: '**', component: NotFoundComponent}
];
