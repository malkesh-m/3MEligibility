import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class FactorsService {
  private apiUrl = environment.apiUrl;
  constructor(private http: HttpClient) { }
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'Factor'
    });
  }
  getFactorsList() {
    return this.http.get<any>(`${this.apiUrl}/factors/getAll`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  getFactorsByParameterId(paramId: number) {
    return this.http.get<any>(`${this.apiUrl}/factors/getFactorsByParameterId?parameterid=${paramId}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  getParametersList() {
    return this.http.get<any>(`${this.apiUrl}/parameter/getAll`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  getConditionsList() {
    return this.http.get<any>(`${this.apiUrl}/condition/getAll`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  saveFactor(factor: any) {
    return this.http.post<any>(`${this.apiUrl}/factors`, factor, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  updateFactor(factor: any) {
    return this.http.put<any>(`${this.apiUrl}/factors`, factor, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  deleteFactor(id: number) {
    return this.http.delete<any>(`${this.apiUrl}/factors?id=${id}`, { headers: this.getHeaders() }).pipe(
      catchError(this.handleError)
    );
  }

  deleteMultipleFactors(ids: number[]): Observable<any> {
    return this.http.request<any>('delete', `${this.apiUrl}/factors/multipleDelete`, {
      body: ids,  headers: this.getHeaders() 
    }).pipe(
      catchError(this.handleError)
    );
  }

  // exportFactors(): Observable<Blob> {
  //   return this.http.get(`${this.apiUrl}/factors/export`, { responseType: 'blob' }).pipe(
  //     catchError(this.handleError)
  //   );
  // }

  exportFactors(selectedIds: number[]): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/factors/export`, selectedIds, { 
      responseType: 'blob',   headers: this.getHeaders() 
    }).pipe(
      catchError(this.handleError)
    );
  }

  fetchAllLists(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/managedList/getAll`, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
  }

  downloadTemplate(): Observable<Blob> {
    return this.http
      .get(this.apiUrl + '/factors/Download-Template', { responseType: 'blob',  headers: this.getHeaders() }).pipe(
        catchError(this.handleError)
      );
  }
  
  importFactor(file: File,createdBy:string): Observable<any> { // Return an Observable
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(this.apiUrl + `/factors/import`, formData, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
  }
  
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred!';
    console.log("error.error ", error.error)
    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else {
      errorMessage = `${error.error.message}`;
    }
    return throwError(() => new Error(errorMessage));
  }
}
