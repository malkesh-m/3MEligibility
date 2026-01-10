import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getGroupList() {
    return this.http.get<any>(`${this.apiUrl}/securitygroup/getall`).pipe(catchError(this.handleError))
  }

  getUnAssignedRolesByGroupId(groupId:number) {
    return this.http.get<any>(`${this.apiUrl}/grouprole/getUnAssignedRolesByGroupId?groupId=${groupId}`).pipe(catchError(this.handleError))
  }

  getAssignedRolesByGroupId(groupId:number) {
    return this.http.get<any>(`${this.apiUrl}/grouprole/getAssignedRolesByGroupId?groupId=${groupId}`).pipe(catchError(this.handleError))
  }

  addRole(payload: any) {
    return this.http
      .post<any>(`${this.apiUrl}/grouprole`, payload)
      .pipe(catchError(this.handleError));
  }

  deleteRole(payload: any) {
    return this.http.delete<any>(`${this.apiUrl}/grouprole`, { body: payload }).pipe(catchError(this.handleError));
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred!';
    console.log("error.error ", error.error)
    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    }
    else {
      errorMessage = `${error.error.message}`;
    }
    console.log("errorMessage ===>", errorMessage)
    return throwError(() => new Error(errorMessage));
  }
}
