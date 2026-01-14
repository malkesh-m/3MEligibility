import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';
import { LogerrorService } from '../services/log/logerror.service';

export const GlobalErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const logService = inject(LogerrorService);
  const snackBar = inject(MatSnackBar);

  // Skip logging for log API calls to avoid infinite loops
  if (req.url.includes('/api/log')) {
    return next(req);
  }

  const componentName = req.headers.get('X-Component') || 'Unknown';

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      console.log(error.status);
      console.log("error");

      logService.captureLog({
        component: componentName || "GlobalError",
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
        snackBar.open('Too many requests, please wait...', 'Close', {
          duration: 5000,
        });
      }

      return throwError(() => error);
    })
  );
};
