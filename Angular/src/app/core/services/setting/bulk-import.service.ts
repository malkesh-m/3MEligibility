import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BulkImportService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'BulkImport'
    });
  }
  downloadTemplate(selectedList: string): Observable<Blob> {
    const formData = new FormData();
    formData.append('selectedList', selectedList);
    return this.http
      .post(this.apiUrl + '/bulkimport/download-template', formData, { responseType: 'blob', headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }

  downloadFile(id: string): Observable<Blob> {
    const formData = new FormData();
    formData.append('id', id);
    return this.http
      .post(this.apiUrl + '/bulkimport/download', formData, { responseType: 'blob', headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }

  fetchBulkImportHistory() {
    return this.http.get<any>(`${this.apiUrl}/bulkimport/getall`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  bulkImport(file: File): Observable<any> { // Return an Observable
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(this.apiUrl + `/bulkimport/import`, formData, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
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
