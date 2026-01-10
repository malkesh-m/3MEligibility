import { Injectable } from '@angular/core';
import {
    HttpInterceptor,
    HttpRequest,
    HttpHandler,
    HttpEvent,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(private router: Router) { }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const token = localStorage.getItem('token');
        if (token) {
            const clonedRequest = req.clone({
                headers: req.headers.set('Authorization', `Bearer ${token}`),
            });
            return next.handle(clonedRequest).pipe(
                catchError((error) => {
                    console.log("error guard ", error)
                    if (error.status === 401) {
                        this.handleUnauthorized();
                    }
                    return throwError(error);
                })
            );
        }

        return next.handle(req);
    }

    private isTokenExpired(token: string): boolean {
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            const expiry = payload.exp * 1000;
            return Date.now() > expiry;
        } catch (e) {
            return true;
        }
    }

    private handleUnauthorized(): void {
        localStorage.removeItem('token');
        this.router.navigate(['/auth']);
    }
}
