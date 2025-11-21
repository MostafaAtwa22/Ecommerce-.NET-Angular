import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError, throwError } from 'rxjs';
import { AccountService } from '../../account/account-service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const toastrService = inject(ToastrService);
  const accountService = inject(AccountService);

  return next(req).pipe(
    catchError(error => {
      if (error) {
        switch (error.status) {
          case 400:
            if (error.error.errors)
                throw error.errors;
            else
              toastrService.error(error.error.message || 'Bad Request',
              error.error.StatusCode);
            break;

          case 401:
            // Don't logout on login/register/home requests (these don't require auth)
            const isAuthRequest = req.url.includes('/api/auth/login') ||
                                  req.url.includes('/api/auth/register') ||
                                  req.url.includes('/api/account/login') ||
                                  req.url.includes('/api/account/register') ||
                                  req.url.includes('/api/home');

            if (!isAuthRequest) {
              // Token expired or invalid - logout automatically
              toastrService.warning('Your session has expired. Please login again.', 'Session Expired');
              accountService.logout();
            } else {
              // This is a login/register error, just show the error
              toastrService.error(error.error.message || 'Unauthorized Access',
                error.error.StatusCode);
            }
            break;

          case 404:
            router.navigateByUrl('/not-found');
            break;

          case 500:
            const navExtras: NavigationExtras = {
              state: {error: error.error}
            };
            router.navigateByUrl('/server-error', navExtras);
            break;

          default:
            toastrService.error('An unexpected error occurred', 'Error');
            break;
        }
      }

      return throwError(() => error);
    })
  );
};
