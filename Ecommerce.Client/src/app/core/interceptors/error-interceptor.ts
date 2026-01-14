import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError, throwError, switchMap, BehaviorSubject, filter, take } from 'rxjs';
import { AccountService } from '../../account/account-service';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<any>(null);

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const toastrService = inject(ToastrService);
  const accountService = inject(AccountService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error) {
        switch (error.status) {
          case 400:
            // Skip error toast for revoke-token failures (expected when no cookie)
            if (req.url.includes('/revoke-token')) {
              break;
            }
            if (error.error.errors)
                throw error.error.errors;
            else
              toastrService.error(error.error.message || 'Bad Request',
              error.error.StatusCode);
            break;

          case 401:
            // Check if this is a refresh-token or revoke-token request
            const isRefreshRequest = req.url.includes('/refresh-token');
            const isRevokeRequest = req.url.includes('/revoke-token');
            
            // Don't try to refresh on login/register/home requests
            const isAuthRequest = req.url.includes('/api/auth/login') ||
                                  req.url.includes('/api/auth/register') ||
                                  req.url.includes('/api/account/login') ||
                                  req.url.includes('/api/account/register') ||
                                  req.url.includes('/api/account/googlelogin') ||
                                  req.url.includes('/api/home');

            // If this is a refresh or revoke request that failed, logout immediately
            if (isRefreshRequest || isRevokeRequest) {
              toastrService.warning('Your session has expired. Please login again.', 'Session Expired');
              accountService.logout();
              return throwError(() => error);
            }

            if (!isAuthRequest) {
              // Try to refresh the token
              if (!isRefreshing) {
                isRefreshing = true;
                refreshTokenSubject.next(null);

                return accountService.refreshToken().pipe(
                  switchMap((user) => {
                    isRefreshing = false;
                    refreshTokenSubject.next(user.token);
                    
                    // Retry the failed request with new token
                    const clonedReq = req.clone({
                      setHeaders: {
                        Authorization: `Bearer ${user.token}`
                      }
                    });
                    return next(clonedReq);
                  }),
                  catchError((refreshError) => {
                    isRefreshing = false;
                    toastrService.warning('Your session has expired. Please login again.', 'Session Expired');
                    accountService.logout();
                    return throwError(() => refreshError);
                  })
                );
              } else {
                // Wait for the token to be refreshed
                return refreshTokenSubject.pipe(
                  filter(token => token !== null),
                  take(1),
                  switchMap(token => {
                    const clonedReq = req.clone({
                      setHeaders: {
                        Authorization: `Bearer ${token}`
                      }
                    });
                    return next(clonedReq);
                  })
                );
              }
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
