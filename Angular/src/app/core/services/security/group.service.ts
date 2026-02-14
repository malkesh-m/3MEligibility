import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class GroupService {

  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  addUsers(payload: any) {
    return this.http
      .post<any>(`${this.apiUrl}/usergroup`, payload)
      .pipe(catchError(this.handleError));
  }

  getGroupList() {
    return this.http.get<any>(`${this.apiUrl}/securitygroup/getall`).pipe(catchError(this.handleError))
  }

  getAssignedUserbyId(id:number) {
    return this.http.get<any>(`${this.apiUrl}/usergroup/${id}`).pipe(catchError(this.handleError))
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred!';
    console.log("error.error ", error.error)
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
    }
    else {
      // Server-side error
      errorMessage = `${error.error.message}`;
    }
    console.log("errorMessage ===>", errorMessage)
    return throwError(() => new Error(errorMessage));
  }

  addGroup(payload: any) {
    return this.http
      .post<any>(`${this.apiUrl}/securitygroup`, payload)
      .pipe(catchError(this.handleError));
  }

  addAssignedUser(payload: any) {
    return this.http
      .post<any>(`${this.apiUrl}/securitygroup`, payload)
      .pipe(catchError(this.handleError));
  }

  updateUserDetails(payload: any) {
    return this.http
      .put<any>(`${this.apiUrl}/securitygroup`, payload)
      .pipe(catchError(this.handleError));
  }

  updateAssignedUser(payload: any) {
    return this.http
      .put<any>(`${this.apiUrl}/securitygroup`, payload)
      .pipe(catchError(this.handleError));
  }

  deleteGroupWithId(id: number) {
    return this.http
      .delete<any>(`${this.apiUrl}/securitygroup?id=${id}`)
      .pipe(catchError(this.handleError));
  }
  
  deleteAssignedUser(userId: number | null, groupId: number | null) {
    return this.http.delete<any>(`${this.apiUrl}/usergroup/deleteByUserIdAndGroupId?userId=${userId}&groupId=${groupId}`).pipe(catchError(this.handleError))
  }

  getUserGroupCount(userId: number) {
    return this.http.get<any>(`${this.apiUrl}/usergroup/count?userId=${userId}`).pipe(catchError(this.handleError))
  }
}
