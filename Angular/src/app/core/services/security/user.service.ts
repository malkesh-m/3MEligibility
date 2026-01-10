import {
  HttpClient,
  HttpErrorResponse,
  HttpParams,
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';


@Injectable({
  providedIn: 'root',
})
export class UserService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getUsersList() {
    return this.http
      .get<any>(`${this.apiUrl}/user/getall`)
      .pipe(catchError(this.handleError));
  }

  addUsers(payload: any) {
    let params = new HttpParams()
      .set('UserName', payload.UserName)
      .set('LoginId', payload.LoginId)
      .set('UserPassword', payload.UserPassword)
      .set('Email', payload.Email)
      .set('Phone', payload.Phone)
      .set('StatusId', payload.StatusId)

    return this.http
      .post<any>(`${this.apiUrl}/user`, null, { params })
      .pipe(catchError(this.handleError));
  }

  updateUserDetails(payload: any) {
    let params = new HttpParams()
      .set('UserId',payload.userId)
      .set('UserName', payload.userName)
      .set('LoginId', payload.loginId)
      .set('UserPassword', payload.userPassword)
      .set('Email', payload.email)
      .set('Phone', payload.phone)
      .set('StatusId', payload.statusId)
      .set('EntityId', payload.entityId)

    return this.http
      .put<any>(`${this.apiUrl}/user`, null, { params })
      .pipe(catchError(this.handleError));
  }

  deleteUserWithId(id: number) {
    return this.http
      .delete<any>(`${this.apiUrl}/user?id=${id}`)
      .pipe(catchError(this.handleError));
  }

  deActiveMultipleUsers(ids: number[]) {
    return this.http.request<any>('delete', `${this.apiUrl}/user/deactivateuser`, {
      body: ids
    }).pipe(
        catchError(this.handleError)
    );
  }
  reActivateUser(model: any) {
    return this.http.post<any>(`${this.apiUrl}/user/reactivateuser`, model).pipe(
      catchError(this.handleError)
    );
  }


  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred!';
    console.log('error.error ', error.error);
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side error
      errorMessage = `${error.error.message}`;
    }
    console.log('errorMessage ===>', errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}

