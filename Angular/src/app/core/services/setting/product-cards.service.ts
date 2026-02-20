import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ProductCardsService {

  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'ProductCard'
    });
  }
  getProductCardsList() {
    return this.http.get<any>(`${this.apiUrl}/pcard/getAll`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  addProductCard(payload: any) {
    return this.http.post<any>(`${this.apiUrl}/pcard`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  updateProductCard(payload: any) {
    return this.http.put<any>(`${this.apiUrl}/pcard`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  deleteProductCard(id: number) {
    return this.http.delete<any>(`${this.apiUrl}/pcard?id=${id}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  deleteMultipleProductCard(payload: any) {
    console.log("service callimns ", payload)
    return this.http.delete<any>(`${this.apiUrl}/pcard/multipledelete`, { body: payload, headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  // exportPCards(): Observable<Blob> {
  //   return this.http.get(`${this.apiUrl}/pcard/export`, { responseType: 'blob' }).pipe(
  //     catchError(this.handleError)
  //   );
  // }

  exportPCards(selectedIds: number[], searchTerm?: string): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/pcard/export`, { selectedIds, searchTerm }, {
      responseType: 'blob', headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  downloadTemplate(): Observable<Blob> {
    return this.http
      .get(this.apiUrl + '/pcard/Download-Template', { responseType: 'blob', headers: this.getHeaders() }).pipe(
        catchError(this.handleError)
      );
  }

  importPCard(file: File): Observable<any> { // Return an Observable
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(this.apiUrl + `/pcard/import`, formData, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
  }

  validatePCard(pcardId: number, payload: { [parameterId: number]: string }) {
    return this.http.post<any>(`${this.apiUrl}/validator/validatepcard?pCardId=${pcardId}`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  validateFormPCard(expreesion: string, payload: { [parameterId: number]: string }) {
    return this.http.post<any>(`${this.apiUrl}/validator/validateformpcard?expreesion=${expreesion}`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
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
