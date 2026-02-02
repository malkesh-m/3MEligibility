import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { catchError, map, Observable, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Parameter } from '../../../features/setting/parameters/parameters.component';

@Injectable({
  providedIn: 'root'
})
export class ParameterService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'Parameter'
    });
  }
  getParameters(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/parameter/getAll`, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
  }

  updateParameter(parameter: Parameter): Observable<any> {
    const url = `${this.apiUrl}/Parameter`;
    return this.http.put(url, parameter, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
  }
  addParameter(parameter: Parameter): Observable<any> {
    const url = `${this.apiUrl}/parameter`;
    return this.http.post(url, parameter, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
  }

  deleteParameter(parameterId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/parameter`, {
      params: { id: parameterId.toString() }, headers: this.getHeaders()
    }).pipe(catchError(this.handleError));
  }

  deleteMultipleParameters(ids: number[]): Observable<any> {
    const url = `${this.apiUrl}/parameter/multipledelete`;
    return this.http.request('DELETE', url, {
      body: ids, headers: this.getHeaders() // Pass the array of IDs in the body
    }).pipe(
      catchError(this.handleError)
    );
  }


  downloadTemplate(): Observable<Blob> {
    return this.http
      .get(this.apiUrl + '/parameter/Download-Template', { responseType: 'blob', headers: this.getHeaders() }).pipe(
        catchError(this.handleError)
      );
  }

  importParameter(file: File, identifier: number, createdBy: string): Observable<any> { // Return an Observable
    const formData = new FormData();
    formData.append('file', file);
    if (identifier === 1) {
      return this.http.post(this.apiUrl + `/parameter/importcustomer?createdBy=${createdBy}`, formData, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
    } else {
      return this.http.post(this.apiUrl + `/parameter/importproduct`, formData, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
    }
  }

  getConditions(): Observable<{ conditionId: number; conditionValue: string }[]> {
    return this.http.get<any>(`${this.apiUrl}/condition/getall`, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
  }

  getDataTypes(): Observable<{ dataTypeId: number; dataTypeName: string }[]> {
    return this.http.get<{ isSuccess: boolean; message: string; data: { dataTypeId: number; dataTypeName: string }[] }>(
      `${this.apiUrl}/dataType/GetAll`, { headers: this.getHeaders() }
    ).pipe(
      map(response => response.data),
      catchError(this.handleError)
    );
  }


  // exportParameters(identifier: number): Observable<Blob> {
  //   return this.http.get(`${this.apiUrl}/parameter/export?Identifier=${identifier}`, { responseType: 'blob' }).pipe(
  //     catchError(this.handleError)
  //   );
  // }
  exportParameters(identifier: number, selectedIds: number[]): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/parameter/export?Identifier=${identifier}`, selectedIds, {
      responseType: 'blob', headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  getSystemParameters(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/parameter/system-parameters`, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
  }

  getParameterBindings(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/parameterbinding`, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
  }

  saveParameterBinding(binding: { systemParameterId: string, mappedParameterId: number | null }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/parameterbinding`, binding, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred';

    if (error.status === 429) {
      errorMessage = 'Too many requests, please wait...';
    }
    else if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    }
    else if (error.error?.message) {
      // Backend error with message
      errorMessage = error.error.message;
    }
    else {
      // Fallback
      errorMessage = error.message;
    }

    return throwError(() => errorMessage);
  }
}
