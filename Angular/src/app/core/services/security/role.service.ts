import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class RoleService {

    private apiUrl = environment.apiUrl;

    constructor(private http: HttpClient) { }

    addUsers(payload: any) {
        return this.http
            .post<any>(`${this.apiUrl}/userrole`, payload)
            .pipe(catchError(this.handleError));
    }

    getRoleList() {
        return this.http.get<any>(`${this.apiUrl}/securityrole/getall`).pipe(catchError(this.handleError))
    }

    getAssignedUserbyId(id: number) {
        return this.http.get<any>(`${this.apiUrl}/userrole/${id}`).pipe(catchError(this.handleError))
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

    addRole(payload: any) {
        return this.http
            .post<any>(`${this.apiUrl}/securityrole`, payload)
            .pipe(catchError(this.handleError));
    }

    addAssignedUser(payload: any) {
        return this.http
            .post<any>(`${this.apiUrl}/securityrole`, payload)
            .pipe(catchError(this.handleError));
    }

    updateRoleDetails(payload: any) {
        return this.http
            .put<any>(`${this.apiUrl}/securityrole`, payload)
            .pipe(catchError(this.handleError));
    }

    updateAssignedUser(payload: any) {
        return this.http
            .put<any>(`${this.apiUrl}/securityrole`, payload)
            .pipe(catchError(this.handleError));
    }

    deleteRoleWithId(id: number) {
        return this.http
            .delete<any>(`${this.apiUrl}/securityrole?id=${id}`)
            .pipe(catchError(this.handleError));
    }

    deleteAssignedUser(userId: number | null, roleId: number | null) {
        return this.http.delete<any>(`${this.apiUrl}/userrole/deleteByUserIdAndRoleId?userId=${userId}&roleId=${roleId}`).pipe(catchError(this.handleError))
    }

    getUserRoleCount(userId: number) {
        return this.http.get<any>(`${this.apiUrl}/userrole/count?userId=${userId}`).pipe(catchError(this.handleError))
    }
}
