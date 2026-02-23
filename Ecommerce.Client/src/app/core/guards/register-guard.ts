import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../../account/account-service';

export const registerGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const router = inject(Router);

  const user = accountService.user();

  if (!accountService.isLoggedIn())
    return true;

  if (user?.roles?.includes('SuperAdmin'))
    return true;

  return router.createUrlTree(['/home']);
};
