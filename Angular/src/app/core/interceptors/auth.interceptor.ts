import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent
} from '@angular/common/http';
import { Observable, from, switchMap } from 'rxjs';
import { OidcAuthService } from '../services/auth/oidc-auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  constructor(private oidcAuthService: OidcAuthService) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {

    return from(this.oidcAuthService.getAccessToken()).pipe(
      switchMap(token => {
        if (token) {
          req = req.clone({
            setHeaders: {
              Authorization: `Bearer ${token}`
            }
          });
        }
        return next.handle(req);
      })
    );
  }
}