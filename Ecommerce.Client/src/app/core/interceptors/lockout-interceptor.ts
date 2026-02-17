import { inject } from '@angular/core';
import {
  HttpInterceptorFn,
  HttpErrorResponse,
  HttpRequest,
  HttpHandlerFn
} from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { AccountService } from '../../account/account-service';

// Logs user out immediately if backend indicates account is locked.
// This avoids waiting for token expiry and prevents refresh-token loops.
export const lockoutInterceptor: HttpInterceptorFn = (
  request: HttpRequest<any>,
  next: HttpHandlerFn
) => {
  const accountService = inject(AccountService);

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 403 && typeof error.error?.message === 'string') {
        const msg = error.error.message.toLowerCase();
        if (msg.includes('locked')) {
          accountService.logout();
        }
      }

      return throwError(() => error);
    })
  );
};
