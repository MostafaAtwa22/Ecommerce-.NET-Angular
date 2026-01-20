import { inject } from '@angular/core';
import {
  HttpInterceptorFn,
  HttpRequest,
  HttpHandlerFn,
  HttpErrorResponse
} from '@angular/common/http';
import { BehaviorSubject, throwError } from 'rxjs';
import { catchError, filter, switchMap, take } from 'rxjs/operators';
import { AccountService } from '../../account/account-service';

// Shared state for handling token refresh
let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<boolean>(false);

export const jwtInterceptor: HttpInterceptorFn = (
  request: HttpRequest<any>,
  next: HttpHandlerFn
) => {
  const accountService = inject(AccountService);
  const user = accountService.user();

  // Attach access token if available
  let authRequest = request;
  if (user?.token) {
    authRequest = request.clone({
      setHeaders: {
        Authorization: `Bearer ${user.token}`
      },
      withCredentials: true
    });
  } else {
    authRequest = request.clone({ withCredentials: true });
  }

  return next(authRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 errors (expired/invalid access token)
      if (error.status === 401 && !request.url.includes('/refresh-token')) {
        return handle401Error(authRequest, next, accountService);
      }
      return throwError(() => error);
    })
  );
};

// Refresh Token Logic
function handle401Error(
  request: HttpRequest<any>,
  next: HttpHandlerFn,
  accountService: AccountService
) {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(false);

    return accountService.refreshToken().pipe(
      switchMap(() => {
        isRefreshing = false;
        refreshTokenSubject.next(true);

        const user = accountService.user();
        const retryRequest = user?.token
          ? request.clone({
              setHeaders: {
                Authorization: `Bearer ${user.token}`
              },
              withCredentials: true
            })
          : request;

        return next(retryRequest);
      }),
      catchError(err => {
        isRefreshing = false;
        accountService.logout();
        return throwError(() => err);
      })
    );
  }

  // Wait until refresh finishes
  return refreshTokenSubject.pipe(
    filter(done => done === true),
    take(1),
    switchMap(() => {
      const user = accountService.user();
      const retryRequest = user?.token
        ? request.clone({
            setHeaders: {
              Authorization: `Bearer ${user.token}`
            },
            withCredentials: true
          })
        : request;

      return next(retryRequest);
    })
  );
}
