import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { catchError, Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class ProductCapService {
  private apiUrl = environment.apiUrl;
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'ProductCap'
    });
  }
  constructor(private http: HttpClient) { }
  getProductList(productId:number) {
    return this.http.get<any>(`${this.apiUrl}/productcap/getbyproductid/${productId}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  deleteProductCap(Id: number) {
    return this.http.delete<any>(`${this.apiUrl}/productcap/${Id}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  updateProductCap(element: any) {
    return this.http.put<any>(`${this.apiUrl}/productcap`, element, { headers: this.getHeaders() } ).pipe(catchError(this.handleError))
  }
  addProductCap(element: any) {
    return this.http.post<any>(`${this.apiUrl}/productcap`, element, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
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
