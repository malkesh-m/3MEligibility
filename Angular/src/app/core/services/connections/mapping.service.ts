import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MappingService {

  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'Mapping'
    });
  }
    getAllMappingFunctions() {
      return this.http.get<any>(`${this.apiUrl}/mappingFunction/getAll`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
    }

    getMappingTreeList() {
      return this.http.get<any>(`${this.apiUrl}/parametersMap/getAll`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
    }

    getAllParameters() {
      return this.http.get<any>(`${this.apiUrl}/parameter/getAll`,{ headers: this.getHeaders() }).pipe(catchError(this.handleError))
    }

    addMap(payload: any) {
      return this.http
        .post<any>(`${this.apiUrl}/parametersMap`, payload, { headers: this.getHeaders() })
        .pipe(catchError(this.handleError));
    }

    updateMap(payload: any) {
      return this.http
        .put<any>(`${this.apiUrl}/parametersMap`, payload, { headers: this.getHeaders() })
        .pipe(catchError(this.handleError));
    }
  
    deleteMap(payload:any) {
      return this.http
        .delete<any>(`${this.apiUrl}/parametersMap?apiId=${payload.apiId}&nodeId=${payload.nodeId}&parameterId=${payload.parameterId}`, { headers: this.getHeaders() })
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
