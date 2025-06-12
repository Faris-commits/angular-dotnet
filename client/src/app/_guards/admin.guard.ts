import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AuthStoreService } from '../_services/AuthStoreService';
import { ToastrService } from 'ngx-toastr';

export const adminGuard: CanActivateFn = (route, state) => {
  const authStore = inject(AuthStoreService);
  const toastr = inject(ToastrService);

  const roles = authStore.currentUserValue?.roles ?? [];
  if (roles.includes('Admin') || roles.includes('Moderator')) {
    return true;
  } else {
    toastr.error('You cannot enter this area');
    return false;
  }
};