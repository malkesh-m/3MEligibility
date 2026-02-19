import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getRoleList() {
    return this.http.get<any>(`${this.apiUrl}/securityrole/getall`).pipe(catchError(this.handleError))
  }

  getUnAssignedPermissionsByRoleId(roleId: number) {
    return this.http.get<any>(`${this.apiUrl}/rolepermission/getUnAssignedPermissionsByRoleId?roleId=${roleId}`).pipe(catchError(this.handleError))
  }

  getAssignedPermissionsByRoleId(roleId: number) {
    return this.http.get<any>(`${this.apiUrl}/rolepermission/getAssignedPermissionsByRoleId?roleId=${roleId}`).pipe(catchError(this.handleError))
  }

  addPermission(payload: any) {
    return this.http
      .post<any>(`${this.apiUrl}/rolepermission`, payload)
      .pipe(catchError(this.handleError));
  }

  deletePermission(payload: any) {
    return this.http.delete<any>(`${this.apiUrl}/rolepermission`, { body: payload }).pipe(catchError(this.handleError));
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
