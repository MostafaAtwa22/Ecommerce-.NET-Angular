import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { BusyService } from '../../shared/services/busy-service';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const busyService = inject(BusyService);
  if(!req.url.includes('emailexists'))
    busyService.busy();

  return next(req).pipe(
    finalize(() => {
      busyService.idle();
    })
  );
};
