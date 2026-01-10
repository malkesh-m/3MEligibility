import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, catchError, Observable, tap, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { RolesService } from '../setting/role.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<any | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient,private rolesService:RolesService) {
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
        if (user?.groupRoles) {
          this.rolesService.setRoles(Object.values(user.groupRoles));
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
    const currentUser = this.currentUserSubject.value.user;


    return this.http.post<any>(
      `${this.apiUrl}/user/Logout/${currentUser.userId}`,
      {},  
      { headers: this.getHeaders() }
    )

      .pipe(
        tap(() => {
          localStorage.removeItem('token');
          localStorage.removeItem('currentUser');
          this.currentUserSubject.next(null);
        }),
        catchError(this.handleError)
      );
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }
}
