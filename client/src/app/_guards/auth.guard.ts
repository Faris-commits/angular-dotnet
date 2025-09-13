import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthStoreService } from '../_services/AuthStoreService';

export const authGuard: CanActivateFn = (route, state) => {
  const authStore = inject(AuthStoreService);
  const toastr = inject(ToastrService);

  if (authStore.currentUserValue) {
    return true;
  } else {
    toastr.error('You shall not pass!');
    return false;
  }
};