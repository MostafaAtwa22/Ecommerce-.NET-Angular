import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../../account/account-service';

export const adminChatGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const router = inject(Router);

  if (!accountService.isLoggedIn()) {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }

  const user = accountService.user();
  const roles = user?.roles ?? [];

  const allowed = roles.some(r => {
    const role = (r || '').toLowerCase();
    return role === 'admin' || role === 'superadmin' || role.includes('admin');
  });

  if (allowed) {
    return true;
  }

  return router.createUrlTree(['/unauthorized']);
};
