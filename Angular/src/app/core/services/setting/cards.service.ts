import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export class CardsService {
  private apiUrl = environment.apiUrl;
  constructor(private http: HttpClient) {
  
   }
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'Cards'  
    });
  }
   getCardsList(){
     return this.http.get<any>(`${this.apiUrl}/ecard/getAll`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
   }
   updateCard(payload:any): Observable<any>{
    const url = `${this.apiUrl}/ecard`;
     return this.http.put(url, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
   }
   addCard(payload:any): Observable<any>{
    const url = `${this.apiUrl}/ecard`;
     return this.http.post(url, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
   }
   deleteCard(ecardId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/ecard`, {
      params: { id: ecardId.toString() },
      headers: this.getHeaders()

    }).pipe(catchError(this.handleError));
  }
  deleteMultipleCards(cardIds: number[]): Observable<any> {
    return this.http.delete(`${this.apiUrl}/ecard/multipleDelete`, {
      body: cardIds, headers: this.getHeaders()
 // Send the parameter IDs as the request body
    });

  }

  downloadTemplate(): Observable<Blob> {
    return this.http
      .get(this.apiUrl + '/ecard/Download-Template', { responseType: 'blob', headers: this.getHeaders() },).pipe(
        catchError(this.handleError)
      );
  }

  importCard(file: File,createdBy:string): Observable<any> { // Return an Observable
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(this.apiUrl + `/ecard/import`, formData, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
  }

  

  // exportCards(): Observable<Blob> {
  //   return this.http.get(`${this.apiUrl}/ecard/export`, { responseType: 'blob' }).pipe(
  //     catchError(this.handleError)
  //   );
  // }

  exportCards(selectedIds: number[]): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/ecard/export`, selectedIds, { 
      responseType: 'blob', headers: this.getHeaders() 
    }).pipe(
      catchError(this.handleError)
    );
  }
  
  validateCard(cardId: number, payload: {[parameterId: number]: string}) {
    return this.http.post<any>(`${this.apiUrl}/validator/validateecard?eCardId=${cardId}`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  validateFormECard(expreesion: string, payload: {[parameterId: number]: string}) {
    return this.http.post<any>(`${this.apiUrl}/validator/validateformecard?expreesion=${expreesion}`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
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
