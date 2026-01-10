import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse,
  HttpResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { LogerrorService } from '../services/Log/logerror.service';

@Injectable()
export class GlobalErrorInterceptor implements HttpInterceptor {

  constructor(private logService: LogerrorService, private snackBar: MatSnackBar) { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (req.url.includes('/api/log')) {
      return next.handle(req);
    }
    const componentName = req.headers.get('X-Component') || 'Unknown';

    return next.handle(req).pipe(
   
      catchError((error: HttpErrorResponse) => {
     
        console.log(error.status)
        console.log("error")
        this.logService.captureLog({
          component: componentName||"GlobalError",
          path: req.url,
          request: JSON.stringify(req.body || {}),
          message: error.message,
          status: error.status,
          stack: error.error?.stack || '',
          userAgent: navigator.userAgent,
          error: error.error
        }).subscribe({
          error: (err: any) => console.error('Failed to send error log:', err)
        });
        if (error.status === 429) {
          this.snackBar.open('Too many requests, please wait...', 'Close', {
            duration: 5000,
          });
        }

        return throwError(() => error);
      })
    );
  }
}
