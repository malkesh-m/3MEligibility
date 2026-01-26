import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { map } from 'rxjs/operators';

export const globalDateInterceptor: HttpInterceptorFn = (req, next) => {
  const datePattern = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/;

  const isDateString = (value: any): boolean => {
    if (typeof value !== 'string') return false;
    return datePattern.test(value);
  };

  const convertDates = (obj: any): void => {
    if (!obj || typeof obj !== 'object') return;

    if (Array.isArray(obj)) {
      obj.forEach(item => convertDates(item));
      return;
    }

    Object.keys(obj).forEach(key => {
      const value = obj[key];

      if (isDateString(value)) {
        // Convert UTC string to Date object
        // Ensure the string is treated as UTC by appending 'Z' if not present
        let dateStr = value;
        if (!dateStr.endsWith('Z')) {
          dateStr += 'Z';
        }
        obj[key] = new Date(dateStr);
        console.log(`Converted ${key}: "${value}" → Date object (${obj[key].toString()})`);
      } else if (typeof value === 'object' && value !== null) {
        convertDates(value);
      }
    });
  };

  return next(req).pipe(
    map(event => {
      if (event instanceof HttpResponse && event.body) {
        console.log('Interceptor running on:', req.url);
        convertDates(event.body);
      }
      return event;
    })
  );
};