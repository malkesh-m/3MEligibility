import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { from, switchMap } from 'rxjs';
import { OidcAuthService } from '../services/auth/oidc-auth.service';

export const AuthInterceptor: HttpInterceptorFn = (req, next) => {
  const oidcAuthService = inject(OidcAuthService);

  // Get token from OIDC service
  return from(oidcAuthService.getAccessToken()).pipe(
    switchMap(token => {
      if (token) {
        // Clone request and add authorization header
        const clonedReq = req.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`
          }
        });
        return next(clonedReq);
      }

      // No token, proceed without auth header
      return next(req)
    })
  );
};