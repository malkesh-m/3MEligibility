import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuditService {
  private apiUrl = environment.apiUrl;
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'AuditLog'
    });
  }
  constructor(private http: HttpClient) { }
  getAuditLog(pageIndex: number, pageSize:number) {
    return this.http.get<any>(`${this.apiUrl}/audit/getall?pageIndex=${pageIndex}&pageSize=${pageSize}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred!';
    console.log("error.error ", error.error)
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side error
      errorMessage = `${error.error.message}`;
    }
    return throwError(() => new Error(errorMessage));
  }
}
