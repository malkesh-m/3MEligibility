import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LogerrorService } from '../services/log/logerror.service';

@Injectable()
export class GlobalErrorInterceptor implements HttpInterceptor {

  constructor(
    private logService: LogerrorService,
    private snackBar: MatSnackBar
  ) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {

    if (req.url.includes('/api/log')) {
      return next.handle(req);
    }

    const componentName = req.headers.get('X-Component') || 'Unknown';

    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {

        this.logService.captureLog({
          component: componentName,
          path: req.url,
          request: JSON.stringify(req.body || {}),
          message: error.message,
          status: error.status,
          stack: error.error?.stack || '',
          userAgent: navigator.userAgent,
          error: error.error
        }).subscribe({
          error: err => console.error('Failed to send error log:', err)
        });

        if (error.status === 429) {
          this.snackBar.open(
            'Too many requests, please wait...',
            'Close',
            { duration: 5000 }
          );
        }

        return throwError(() => error);
      })
    );
  }
}