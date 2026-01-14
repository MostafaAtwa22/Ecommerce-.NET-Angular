import { HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AccountService } from '../../account/account-service';
import { catchError, switchMap, throwError, BehaviorSubject, filter, take, Observable } from 'rxjs';
import { isTokenExpired } from '../../shared/utils/token-utils';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

const handleTokenRefresh = (
  req: HttpRequest<any>,
  next: HttpHandlerFn,
  accountService: AccountService
): Observable<any> => {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    return accountService.refreshToken().pipe(
      switchMap((user) => {
        isRefreshing = false;
        refreshTokenSubject.next(user.token);
        const retryReq = req.clone({
          setHeaders: { Authorization: `Bearer ${user.token}` },
        });
        return next(retryReq);
      }),
      catchError((err) => {
        isRefreshing = false;
        accountService.logout();
        return throwError(() => err);
      })
    );
  } else {
    return refreshTokenSubject.pipe(
      filter((t) => t != null),
      take(1),
      switchMap((newToken) => {
        const retryReq = req.clone({
          setHeaders: { Authorization: `Bearer ${newToken}` },
        });
        return next(retryReq);
      })
    );
  }
};

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token');
  const accountService = inject(AccountService);

  // Skip auth headers for login/register/home/refresh-token
  if (
    req.url.includes('/login') ||
    req.url.includes('/register') ||
    req.url.includes('/home') ||
    req.url.includes('/refresh-token')
  ) {
    return next(req);
  }

  // Proactive check: If token is expired, refresh before sending
  if (token && isTokenExpired(token)) {
      return handleTokenRefresh(req, next, accountService);
  }

  // Normal request with valid (or assumed valid) token
  if (token) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }

  return next(req).pipe(
    catchError((error) => {
      // Fallback: If token was valid client-side but server rejected it (e.g. revoked)
      if (error.status === 401) {
        return handleTokenRefresh(req, next, accountService);
      }
      return throwError(() => error);
    })
  );
};
