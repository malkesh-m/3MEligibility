import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ListsService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'ManagedList'
    });
  }
  fetchAllLists(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/managedlist/getall`, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
  }

  updateLists(payload: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/managedlist`, payload,).pipe(catchError(this.handleError));
  }

  addLists(payload: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/managedlist`, payload).pipe(catchError(this.handleError));
  }

  deleteParameter(listId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/managedlist`, {
      params: { id: listId.toString() }
    }).pipe(catchError(this.handleError));
  }


  deleteMultipleList(listId: any): Observable<any> {
    const url = `${this.apiUrl}/managedlist/multipledelete`;
    return this.http.request('DELETE', url, {
      body: listId,
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteMultipleListItem(listId: any): Observable<any> {
    const url = `${this.apiUrl}/listitem/multipledelete`;
    return this.http.request('DELETE', url, {
      body: listId,
    }).pipe(
      catchError(this.handleError)
    );
  }



  fetchListItems(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/listitem/getall`).pipe(catchError(this.handleError));
  }


  deleteListItem(itemId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/listitem`, {
      params: { id: itemId.toString() }
    }).pipe(catchError(this.handleError));
  }

  addListsItem(payload: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/listitem`, payload).pipe(catchError(this.handleError));
  }

  updateListsItem(payload: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/listitem`, payload).pipe(catchError(this.handleError));
  }

  downloadListTemplate(): Observable<Blob> {
    return this.http
      .get(this.apiUrl + '/managedlist/Download-Template', { responseType: 'blob' }).pipe(
        catchError(this.handleError)
      );
  }

  downloadItemTemplate(): Observable<Blob> {
    return this.http
      .get(this.apiUrl + '/listitem/Download-Template', { responseType: 'blob' }).pipe(
        catchError(this.handleError)
      );
  }

  importList(file: File,createdBy:string): Observable<any> { // Return an Observable
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(this.apiUrl + `/managedlist/import`, formData).pipe(catchError(this.handleError));
  }

  // ExportListIteam(): Observable<Blob> {
  //   return this.http.get(`${this.apiUrl}/listItem/export`, { responseType: 'blob' }).pipe(
  //     catchError(this.handleError)
  //   );
  // }

  ExportListIteam(selectedIds: number[]): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/listitem/export`, selectedIds, { 
      responseType: 'blob' 
    }).pipe(
      catchError(this.handleError)
    );
  }

  // ExportLists(): Observable<Blob> {
  //   return this.http.get(`${this.apiUrl}/managedList/export`, { responseType: 'blob' }).pipe(
  //     catchError(this.handleError)
  //   );
  // }

  ExportLists(selectedIds: number[]): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/managedlist/export`, selectedIds, { 
      responseType: 'blob' 
    }).pipe(
      catchError(this.handleError)
    );
  }

  importListItem(file: File,createdBy:string): Observable<any> { // Return an Observable
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(this.apiUrl + `/listitem/import`, formData).pipe(catchError(this.handleError));
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
