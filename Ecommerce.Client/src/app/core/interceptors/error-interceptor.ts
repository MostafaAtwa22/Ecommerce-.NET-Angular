import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AccountService } from '../../account/account-service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const accountService = inject(AccountService);

  return next(req).pipe(
    catchError(error => {
      if (error) {
        switch (error.status) {
          case 400:
            if (error.error.errors)
                throw error.errors;
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
            break;
        }
      }

      return throwError(() => error);
    })
  );
};
