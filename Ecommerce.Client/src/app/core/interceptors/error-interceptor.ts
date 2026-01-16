import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError, throwError, switchMap, BehaviorSubject, filter, take } from 'rxjs';
import { AccountService } from '../../account/account-service';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const toastrService = inject(ToastrService);
  const accountService = inject(AccountService);

  console.log('Intercepting request to:', req.url);
  console.log('Has token:', !!localStorage.getItem('token'));
  console.log('Cookies available:', !!document.cookie);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      console.log('Error intercepted:', error.status, error.url);

      if (error) {
        switch (error.status) {
          case 400:
            // Skip error toast for revoke-token failures (expected when no cookie)
            if (req.url.includes('/revoke-token')) {
              break;
            }
            if (error.error.errors) {
              throw error.error.errors;
            } else {
              toastrService.error(error.error.message || 'Bad Request', error.error.StatusCode);
            }
            break;

          case 401:
            console.log('🔴 401 Unauthorized for:', req.url);

            // Don't try to refresh on these endpoints
            const skipRefreshEndpoints = [
              '/login',
              '/register',
              '/googlelogin',
              '/email-verification',
              '/forgetpassword',
              '/resetpassword',
              '/home'
            ];

            const shouldSkipRefresh = skipRefreshEndpoints.some(endpoint =>
              req.url.includes(endpoint)
            );

            if (shouldSkipRefresh) {
              console.log('Skipping refresh for public endpoint:', req.url);
              toastrService.error(error.error.message || 'Unauthorized', 'Error');
              return throwError(() => error);
            }

            // If this is already a refresh request that failed, logout
            if (req.url.includes('/refresh-token')) {
              console.log('❌ Refresh token request failed - refresh token likely expired or invalid');
              console.log('Error from refresh endpoint:', error.error?.message);
              isRefreshing = false;
              refreshTokenSubject.next(null);
              toastrService.warning('Session expired. Please login again.', 'Session Expired');
              accountService.clearUserDataOnly();
              return throwError(() => error);
            }

            // Try to refresh token
            if (!isRefreshing) {
              console.log('🔄 Attempting token refresh...');
              isRefreshing = true;
              refreshTokenSubject.next(null);

              return accountService.refreshToken().pipe(
                switchMap((user) => {
                  console.log('✅ Token refresh successful, retrying original request');
                  isRefreshing = false;
                  refreshTokenSubject.next(user.token);

                  // Retry the original request with new token
                  const clonedReq = req.clone({
                    setHeaders: { Authorization: `Bearer ${user.token}` },
                    withCredentials: true
                  });
                  return next(clonedReq);
                }),
                catchError((refreshError) => {
                  console.error('❌ Token refresh failed:', refreshError.status, refreshError.error?.message);
                  isRefreshing = false;
                  refreshTokenSubject.next(null);

                  // If refresh failed with 401, it means refresh token is invalid
                  if (refreshError.status === 401) {
                    console.error('Refresh token is invalid - clearing session');
                    toastrService.warning('Session expired. Please login again.', 'Session Expired');
                    accountService.clearUserDataOnly();
                  } else {
                    toastrService.error('Failed to refresh session', 'Error');
                  }

                  return throwError(() => refreshError);
                })
              );
            } else {
              console.log('⏳ Refresh already in progress, waiting...');
              // Wait for ongoing refresh to complete
              return refreshTokenSubject.pipe(
                filter(token => token !== null),
                take(1),
                switchMap(token => {
                  console.log('🔄 Using refreshed token for queued request');
                  const clonedReq = req.clone({
                    setHeaders: { Authorization: `Bearer ${token}` },
                    withCredentials: true
                  });
                  return next(clonedReq);
                })
              );
            }

          case 404:
            router.navigateByUrl('/not-found');
            break;

          case 500:
            const navExtras: NavigationExtras = {
              state: { error: error.error }
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
