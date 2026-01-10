import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EntityService {

  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'Entity'
    });
  }
  /**
   * entities Crud api services
   **/
  getEntitiesList() {
    return this.http.get<any>(`${this.apiUrl}/entity/getall`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  getCurrencyList() {
    return this.http.get<any>(`${this.apiUrl}/currency/getall`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  addEntities(payload: any) {
    return this.http.post<any>(`${this.apiUrl}/entity`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  updateEntitiesDetails(payload: any) {
    return this.http.put<any>(`${this.apiUrl}/entity`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  deleteEntityWithId(id: number) {
    return this.http.delete<any>(`${this.apiUrl}/entity?id=${id}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  deleteMultipleEntities(ids: number[]): Observable<any> {
    return this.http.request<any>('delete', `${this.apiUrl}/entity/multipledelete`, {
      body: ids, headers: this.getHeaders() 
    }).pipe(
      catchError(this.handleError)
    );
  }

  getCountriesList() {
    return this.http.get<any>(`${this.apiUrl}/country/getall`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  getCitiesList() {
    return this.http.get<any>(`${this.apiUrl}/city/getall`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  // exportEntities(): Observable<Blob> {
  //   return this.http.get(`${this.apiUrl}/entity/export`, { responseType: 'blob' }).pipe(
  //     catchError(this.handleError)
  //   );
  // }

  exportEntities(selectedIds: number[]): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/entity/export`, selectedIds, { 
      responseType: 'blob', headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  downloadTemplate(): Observable<Blob> {
    return this.http
      .get(this.apiUrl + '/entity/Download-Template', { responseType: 'blob', headers: this.getHeaders() }).pipe(
        catchError(this.handleError)
      );
  }

  importEntities(file: File,createdBy:string): Observable<any> { // Return an Observable
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(this.apiUrl + `/entity/import`, formData, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
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
