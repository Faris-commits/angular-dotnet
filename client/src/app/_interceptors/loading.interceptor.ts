import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { LoadingService } from '../_services/loading.service';
import { finalize } from 'rxjs';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);

  const isPollingRequest = req.url.includes('/photos');

  if (!isPollingRequest) {
    loadingService.setLoading(true);
  }

  return next(req).pipe(
    finalize(() => {
      if (!isPollingRequest) {
        loadingService.setLoading(false);
      }
    })
  );
};
