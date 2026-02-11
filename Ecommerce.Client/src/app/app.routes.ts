import { Routes } from '@angular/router';
import { HomeComponent } from './home/home-component';
import { NotFoundComponent } from './core/not-found-component/not-found-component';
import { BasketComponent } from './basket/basket-component';
import { authGuard } from './core/guards/auth-guard';
import { registerGuard } from './core/guards/register-guard';
import { OrderDetailsComponent } from './orders/order-details-component/order-details-component';
import { OrdersComponent } from './orders/orders-component';
import { WishlistComponent } from './wishlist/wishlist.component';
import { guestGuard } from './core/guards/guest-guard';
export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', component: HomeComponent },
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
    path: 'dashboard',
    loadComponent: () =>
      import('./dashboard/dashboard.component').then(m => m.DashboardComponent),
    children: [
      {
        path: '', loadComponent: () =>
          import('./dashboard/dashboard-main.component/dashboard-main.component').then(m => m.DashboardMainComponent),
      },
      {
        path: 'products', loadComponent: () =>
          import('./dashboard/dashboard-products.component/dashboard-products.component').then(m => m.DashboardProductsComponent),
      },
      {
        path: 'orders', loadComponent: () =>
          import('./dashboard/dashboard-orders.component/dashboard-orders.component').then(m => m.DashboardOrdersComponent),
      },
      {
        path: 'chat', loadComponent: () =>
          import('./dashboard/dashboard-chat.component/dashboard-chat.component').then(m => m.DashboardChatComponent),
      },
      {
        path: 'delivery', loadComponent: () =>
          import('./dashboard/dashboard-delivery-method.component/dashboard-delivery-method.component').then(m => m.DashboardDeliveryMethodComponent),
      },
      {
        path: 'users', loadComponent: () =>
          import('./dashboard/dashboard-users.component/dashboard-users.component').then(m => m.DashboardUsersComponent),
        children: [
          {
            path: '', loadComponent: () =>
              import('./dashboard/dashboard-users.component/main-user-info.component/main-user-info.component').then(m => m.MainUserInfoComponent),
          },
          {
            path: 'roles/:id', loadComponent: () =>
              import('./dashboard/dashboard-users.component/user-roles.component/user-roles.component').then(m => m.UserRolesComponent),
          }
        ]
      },
      {
        path: 'roles', loadComponent: () =>
          import('./dashboard/dashboard-roles.component/dashboard-roles.component').then(m => m.DashboardRolesComponent),
        children: [
          {
            path: '', loadComponent: () =>
              import('./dashboard/dashboard-roles.component/main-role-info.component/main-role-info.component').then(m => m.MainRoleInfoComponent),
          },
          {
            path: 'permissions/:id', loadComponent: () =>
              import('./dashboard/dashboard-roles.component/permissions.component/permissions.component').then(m => m.PermissionsComponent),
          }
        ]
      },
    ]
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
    path: 'signin-google',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./account/google-callback.component/google-callback.component')
        .then(s => s.GoogleCallbackComponent)
  },
  {
    path: 'forgetpassword',
    loadComponent: () =>
      import('./account/forget-password.component/forget-password.component')
        .then(s => s.ForgetPasswordComponent)
  },
  {
    path: 'email-verification',
    loadComponent: () =>
      import('./account/verify-email.component/verify-email.component')
        .then(s => s.VerifyEmailComponent),
    canActivate: [guestGuard],
  },
  {
    path: 'resetpassword',
    loadComponent: () =>
      import('./account/reset-password.component/reset-password.component')
        .then(s => s.ResetPasswordComponent)
  },
  {
    path: 'check-inbox',
    loadComponent: () =>
      import('./account/check-inbox.component/check-inbox.component')
        .then(s => s.CheckInboxComponent)
  },
  {
    path: 'verify-2fa',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./account/verfiy2-fa.component/verfiy2-fa.component')
        .then(s => s.Verfiy2FAComponent)
  },
  {
    path: 'login',
    canActivate: [guestGuard],
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
  { path: '**', component: NotFoundComponent }
];
