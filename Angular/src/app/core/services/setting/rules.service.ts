import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class RulesService {

  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'Rules'
    });
  }
  getRulesList() {
    return this.http.get<any>(`${this.apiUrl}/erule/GetAll`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  getRuleById(ruleId: any) {
    return this.http.get<any>(`${this.apiUrl}/erule/${ruleId}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  addNewRule(payload: any) {
    return this.http.post<any>(`${this.apiUrl}/erule`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  editNewERuleMaster(payload: any) {
    return this.http.put<any>(`${this.apiUrl}/erule/editerulemaster`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  addNewEruleMaster(payload: any) {
    return this.http.post<any>(`${this.apiUrl}/erule/adderulemaster`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  updateExistingRule(payload: any) {
    return this.http.put<any>(`${this.apiUrl}/erule`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  deleteEruleMaster(id: any) {
    return this.http.delete<any>(`${this.apiUrl}/erule/deleteerulemaster?id=${id}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  deleteSingleRule(id: number) {
    return this.http.delete<any>(`${this.apiUrl}/erule?id=${id}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  deleteMultipleRules(payload: any) {
    console.log("service callimns ", payload)
    return this.http.delete<any>(`${this.apiUrl}/erule/multipledeleteerulemaster`, { body: payload, headers: this.getHeaders() 
}).pipe(catchError(this.handleError))
  }

  //downloadTemplate(): Observable<Blob> {
  //  return this.http
  //    .get(this.apiUrl + '/erule/Download-Template', { responseType: 'blob', headers: this.getHeaders() }).pipe(
  //      catchError(this.handleError)
  //    );
  //}
  downloadTemplate(): Observable<Blob> {
    return this.http
      .get(this.apiUrl + '/erule/download-templateerulemaster', { responseType: 'blob', headers: this.getHeaders() }).pipe(
        catchError(this.handleError)
      );
  }
  
  //importRule(file: File,createdBy:string): Observable<any> { // Return an Observable
  //  const formData = new FormData();
  //  formData.append('file', file);
  //  return this.http.post(this.apiUrl + `/erule/import?createdBy=${createdBy}`, formData, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
  //}
  importRule(file: File): Observable<any> { // Return an Observable
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(this.apiUrl + `/erule/importerulemaster`, formData, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
  }
  
  // exportRules(): Observable<Blob> {
  //   return this.http.get(`${this.apiUrl}/erule/export`, { responseType: 'blob' }).pipe(
  //     catchError(this.handleError)
  //   );
  // }

  exportRules(selectedIds: number[]): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/erule/export`, selectedIds, { 
      responseType: 'blob', headers: this.getHeaders() 
    }).pipe(
      catchError(this.handleError)
    );
  }

  validateRule(ruleId: number, payload: {[parameterId: number]: string}) {
    return this.http.post<any>(`${this.apiUrl}/validator/validateerule?ruleId=${ruleId}`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  validateFormErule(expreesion: string, payload: {[parameterId: number]: string}) {
    return this.http.post<any>(`${this.apiUrl}/validator/validateformerule?expreesion=${expreesion}`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
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
