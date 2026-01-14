import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { OidcAuthService } from '../../services/auth/oidc-auth.service';

export const authGuard: CanActivateFn = () => {
  const oidcAuthService = inject(OidcAuthService);
  const router = inject(Router);

  if (oidcAuthService.isAuthenticated()) {
    return true;
  }

  // Redirect to login page
  router.navigate(['/auth/login']);
  return false;
};