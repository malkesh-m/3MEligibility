import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserprogileService {

  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  updateUsers(payload: any) {
    let params = new HttpParams()
      .set('UserId', payload.userId)
      .set('Phone', payload.phone);
      
    if (payload.entityId) {
      params = params.set('EntityId', payload.entityId);
    }
    return this.http
      .put<any>(`${this.apiUrl}/user`, null, { params })
      .pipe(catchError(this.handleError));
  }

  getUserProfileById(groupId: number) {
    return this.http.get<any>(`${this.apiUrl}/user/${groupId}`).pipe(catchError(this.handleError))
  }

  changePassword(payload:any){
    return this.http
      .post<any>(`${this.apiUrl}/user/ChangePassword`, payload)
      .pipe(catchError(this.handleError));
  }

  getUserGroup(){
    return this.http
    .get<any>(`${this.apiUrl}/usergroup`)
    .pipe(catchError(this.handleError));
  }

  getGroupById(groupId: number){
    return this.http
    .get<any>(`${this.apiUrl}/usergroup/${groupId}`)
    .pipe(catchError(this.handleError));
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
