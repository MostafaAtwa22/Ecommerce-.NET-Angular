import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../../account/account-service';

export const guestGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const router = inject(Router);

  if (accountService.isLoggedIn()) {
    return router.createUrlTree(['/home']);
  }

  return true;
};
