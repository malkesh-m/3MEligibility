import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ExceptionManagementService {
private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'ExceptionManagement'
    });
  }
  getExceptionList() {
    return this.http.get<any>(`${this.apiUrl}/exceptionmanagement/getall`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  addException(payload: any) {
    return this.http.post<any>(`${this.apiUrl}/exceptionmanagement`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  getExceptionParameterList() {
    return this.http.get<any>(`${this.apiUrl}/exceptionparameter/getall`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  addExceptionParameter(payload: any) {
    return this.http.post<any>(`${this.apiUrl}/exceptionparameter`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  
  getExceptionById(Id: number) {
    return this.http.get<any>(`${this.apiUrl}/exceptionmanagement/${Id}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  
  updateException(payload: any) {
    return this.http.put<any>(`${this.apiUrl}/exceptionmanagement`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  deleteException(Id: any) {
    return this.http.delete<any>(`${this.apiUrl}/exceptionmanagement?id=${Id}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
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
}
