import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../../account/account-service';
import { inject } from '@angular/core';
import { isTokenExpired } from '../../shared/utils/token-utils';

export const authGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const router = inject(Router);

  const token = localStorage.getItem('token');

  // Check if token exists and is not expired
  if (token && isTokenExpired(token)) {
    accountService.logout();
    router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }

  if (accountService.isLoggedIn()) {
    return true;
  } else {
    router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }
};
