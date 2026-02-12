import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable, Injector } from '@angular/core';
import { BehaviorSubject, Observable, throwError, from, of } from 'rxjs';
import { catchError, tap, switchMap } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { OidcAuthService } from './oidc-auth.service';
import { PermissionsService } from '../setting/permission.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<any | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private injector: Injector,
    private PermissionsService: PermissionsService
  ) {
    const savedUser = localStorage.getItem('currentUser');
    this.currentUserSubject = new BehaviorSubject<any | null>(savedUser ? JSON.parse(savedUser) : null);
    this.currentUser$ = this.currentUserSubject.asObservable();
  }
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'Auth'
    });
  }
  login(credentials: { loginId: string; password: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/user/login`, credentials, { headers: this.getHeaders() }).pipe(
      tap((user) => {
        console.log(user);
        console.log("user");

        localStorage.setItem('token', user.token);
        localStorage.setItem('currentUser', JSON.stringify(user));
        this.currentUserSubject.next(user);
        if (user?.groupPermissions) {
          this.PermissionsService.setPermissions(Object.values(user.groupPermissions));
        }
      }),
      catchError(this.handleError)
    );
  }

  forget(payload: any) {
    return this.http
      .post<any>(`${this.apiUrl}/user/ForgotPassword`, payload, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }

  resetPassword(payload: any) {
    return this.http
      .post<any>(`${this.apiUrl}/user/ResetPassword`, payload, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }
  changePassword(payload: any) {
    return this.http.post(`${this.apiUrl}/user/changepassword`, payload).pipe(catchError(this.handleError));;
  }
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred';

    if (error.status === 429) {
      errorMessage = 'Too many requests, please wait...';
    }
    else if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    }
    else if (error.error?.message) {
      // Backend error with message
      errorMessage = error.error.message;
    }
    else {
      // Fallback
      errorMessage = error.message;
    }

    return throwError(() => errorMessage);
  }
  logout(): Observable<any> {
    const currentUser = this.currentUserSubject.value?.user; // Safe access
    const userId = currentUser ? currentUser.userId : 'unknown';

    // 1. Call Backend Logout API
    return this.http.post<any>(
      `${this.apiUrl}/user/Logout/${userId}`,
      {},
      { headers: this.getHeaders() }
    ).pipe(
      catchError(err => {
        return of(null);
      }),
      tap(() => {
        localStorage.removeItem('token');
        localStorage.removeItem('currentUser');
        localStorage.removeItem('keycloak_profile');
        this.currentUserSubject.next(null);
      }),
      switchMap(() => {
        try {
          const oidcAuthService = this.injector.get(OidcAuthService);
          return from(oidcAuthService.logout()).pipe(
            catchError(err => {
              console.error('OIDC logout failed explicitly:', err);
              return of(null);
            })
          );
        } catch (e) {
          console.error('Could not get OidcAuthService:', e);
          return of(null);
        }
      })
    );
  }
loadUserPermissions() {
  return this.http.get<any>(`${this.apiUrl}/user/me`).pipe(
    tap(res => this.PermissionsService.setPermissions(res.permissions || []))
  );
}

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }
}


