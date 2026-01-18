import { HttpInterceptorFn } from '@angular/common/http';
import { isTokenExpired } from '../../shared/utils/token-utils';
import { inject } from '@angular/core';
import { AccountService } from '../../account/account-service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token');

  if (
    req.url.includes('/api/auth/login') ||
    req.url.includes('/api/auth/register') ||
    req.url.includes('/api/home')
  ) {
    return next(req);
  }

  if (token) {
    // Check if token is expired before adding it to the request
    if (isTokenExpired(token)) {
      const accountService = inject(AccountService);
      accountService.logout();
      return next(req);
    }

    const isFormData = req.body instanceof FormData;
    const headers: Record<string, string> = {
      Authorization: `Bearer ${token}`
    };

    // For non-multipart requests, ensure Content-Type is set to application/json if the caller hasn’t specified it.
    if (!isFormData && !req.headers.has('Content-Type')) {
      headers['Content-Type'] = 'application/json';
    }

    req = req.clone({ setHeaders: headers });
  }

  return next(req);
};