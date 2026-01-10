import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { catchError, Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class IntegrationService {

  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'Integration'
    });
  }
  saveApiParameterMapping(mapping: any) {
    return this.http.post<any>(`${this.apiUrl}/apiparametermaps`, mapping);
  }
  getApiParameterMapping() {
    return this.http.get<any>(`${this.apiUrl}/apiparametermaps`);
  }
  getNodeList() {
    return this.http.get<any>(`${this.apiUrl}/node/getAll`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  getAllApiList() {
    return this.http.get<any>(`${this.apiUrl}/apidetail/getallendpoints`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  getAllApiParameters(apiId: number) {
    return this.http.get<any>(`${this.apiUrl}/apiparameters/getById?id=${apiId}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  
  getNodeById(id:number) {
    return this.http.get<any>(`${this.apiUrl}/node/${id}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  getApiParamsByApiId(id: number) {
    return this.http.get<any>(`${this.apiUrl}/apiparametermaps/getbyapi/${id}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  getApiParameters() {
    return this.http.get<any>(`${this.apiUrl}/apiparameters`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  deleteApiParameter(id: number) {
    return this.http
      .delete<any>(`${this.apiUrl}/apiparameters/${id}`, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }
  updateApiParameter(model: any) {
    return this.http.put<any>(`${this.apiUrl}/apiparameters`, model, { headers: this.getHeaders() })
    .pipe(catchError(this.handleError));
}

  addNode(payload: any) {
    return this.http
      .post<any>(`${this.apiUrl}/node`, payload, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }

  updateNode(payload: any) {
    return this.http
      .put<any>(`${this.apiUrl}/node`, payload, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }

  deleteNode(id: number) {
    return this.http
      .delete<any>(`${this.apiUrl}/node?id=${id}`, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }

  deleteMapping(id: number) {
    return this.http
      .delete<any>(`${this.apiUrl}/apiparametermaps?id=${id}`, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }

  deleteMultipleNodes(payload: any) {
    return this.http.delete<any>(`${this.apiUrl}/node/MultiPleDelete`, { body: payload, headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }


  getNodeAPIList() {
    return this.http.get<any>(`${this.apiUrl}/nodeApi/getAll`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  getNodeAPIById(id:number) {
    return this.http.get<any>(`${this.apiUrl}/nodeApi/${id}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  getSoapApi(id:number | null) {
    return this.http.get<any>(`${this.apiUrl}/execute/getsoapapis?nodeId=${id}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  addNodeAPI(payload: any) {
    return this.http
      .post<any>(`${this.apiUrl}/nodeApi`, payload, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }

  updateNodeAPI(payload: any) {
    return this.http
      .put<any>(`${this.apiUrl}/nodeApi`, payload, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }

  deleteNodeAPI(id: number) {
    return this.http
      .delete<any>(`${this.apiUrl}/nodeApi?id=${id}`, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }

  deleteMultipleAPI(payload: any) {
    return this.http.delete<any>(`${this.apiUrl}/nodeApi/multipleDelete`, { body: payload, headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  deleteMultipleDetails(payload: any) {
    return this.http.delete<any>(`${this.apiUrl}/apidetail/multipleDelete`, { body: payload, headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  deleteMultipleApiParams(payload: any) {
    return this.http.delete<any>(`${this.apiUrl}/apiparameters/multipleDelete`, { body: payload, headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  getAPIDetailsList() {
    return this.http.get<any>(`${this.apiUrl}/apidetail/getAll`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  getAPIDetailsById(id:number) {
    return this.http.get<any>(`${this.apiUrl}/apidetail/${id}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  addNodeAPIDetails(payload: any) {
    return this.http
      .post<any>(`${this.apiUrl}/apidetail`, payload, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }

  updateNodeAPIDetails(payload: any) {
    return this.http
      .put<any>(`${this.apiUrl}/apidetail`, payload, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }

  deleteNodeDetails(id: number) {
    return this.http
      .delete<any>(`${this.apiUrl}/apidetail?id=${id}`, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }
  addAPIParams(payload: any) {
    return this.http
      .post<any>(`${this.apiUrl}/apiparameters`, payload, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }
  GetAPIParams(payload: any) {
    return this.http
      .post<any>(`${this.apiUrl}/apiparameters`, payload, { headers: this.getHeaders() })
      .pipe(catchError(this.handleError));
  }
  updateApiParameterMapping(model: any) {
    return this.http.put<any>(`${this.apiUrl}/apiparametermaps`, model);
  }
  callApi(request: any): Observable<any> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    return this.http.post(`${this.apiUrl}/eligibility/callexternalapi`, request, { headers });
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
