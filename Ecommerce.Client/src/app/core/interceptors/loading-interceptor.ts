import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { delay, finalize } from 'rxjs';
import { BusyService } from '../../shared/services/busy-service';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const busyService = inject(BusyService);
  if (req.method === 'POST' && req.url.includes('orders'))
    return next(req);

  if (req.method === 'DELETE')
    return next(req);

  if(!req.url.includes('emailexists'))
    return next(req);

  busyService.busy();

  return next(req).pipe(
    delay(1500),
    finalize(() => {
      busyService.idle();
    })
  );
};
