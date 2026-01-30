import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ProductsService {

  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'Products'
    });
  }
  /**
   * categories Crud api services
   **/
  getCategoriesList() {
    return this.http.get<any>(`${this.apiUrl}/category/getall`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  addCategories(payload: any) {
    return this.http.post<any>(`${this.apiUrl}/category`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  updateCategoriesDetails(payload: any) {
    return this.http.put<any>(`${this.apiUrl}/category`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  deleteCategoryWithId(id: number) {
    return this.http.delete<any>(`${this.apiUrl}/category?id=${id}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  deleteMultipleCategories(listAry: any) {
    return this.http.delete<any>(`${this.apiUrl}/category/multipledelete`, { body: listAry, headers: this.getHeaders() 
}).pipe(catchError(this.handleError))
  }

  // exportCategories(): Observable<Blob> {
  //   return this.http.get(`${this.apiUrl}/category/export`, { responseType: 'blob' }).pipe(
  //     catchError(this.handleError)
  //   );
  // }

  exportCategories(selectedIds: number[]): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/category/export`, selectedIds, { 
      responseType: 'blob', headers: this.getHeaders() 
    }).pipe(
      catchError(this.handleError)
    );
  }

  DownloadCategoryTemplate(): Observable<Blob> {
    return this.http
      .get(this.apiUrl + '/category/Download-Template', { responseType: 'blob', headers: this.getHeaders()  }).pipe(
        catchError(this.handleError)
      );
  }

  importCategory(file: File): Observable<any> { // Return an Observable
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(this.apiUrl + `/category/import`, formData, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
  }

  
  /**
   * Info Crud service Api 
   */

  getInfoList() {
    return this.http.get<any>(`${this.apiUrl}/product/getAll`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  getInfoListName() {
    return this.http.get<any>(`${this.apiUrl}/product/getallname`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  addCategoriesInfo(payload: any) {
    return this.http.post<any>(`${this.apiUrl}/product`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  updateCategoriesInfo(payload: any) {
    return this.http.put<any>(`${this.apiUrl}/product`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  deleteCategoryInfoWithId(id: number) {
    return this.http.delete<any>(`${this.apiUrl}/product?id=${id}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  deleteMultipleInfoItems(listAry: any) {
    return this.http.delete<any>(`${this.apiUrl}/product/multipleDelete`, { body: listAry,headers: this.getHeaders() 
}).pipe(catchError(this.handleError))
  }

  // exportInfoList(): Observable<Blob> {
  //   return this.http.get(`${this.apiUrl}/product/export`, { responseType: 'blob' }).pipe(
  //     catchError(this.handleError)
  //   );
  // }

  exportInfoList(selectedIds: number[]): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/product/export`, selectedIds, { 
      responseType: 'blob', headers: this.getHeaders() 
    }).pipe(
      catchError(this.handleError)
    );
  }

  DownloadInfoTemplate(): Observable<Blob> {
    return this.http
      .get(this.apiUrl + '/product/Download-Template', { responseType: 'blob',headers: this.getHeaders() }).pipe(
        catchError(this.handleError)
      );
  }

  importInfo(file: File,createdBy:string): Observable<any> { // Return an Observable
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(this.apiUrl + `/product/import`, formData, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
  }

  
  /**
   * Details Crud Service Api
   */

  getDetailsList() {
    return this.http.get<any>(`${this.apiUrl}/productParam/GetAll`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  addProductDetails(payload: any) {
    return this.http.post<any>(`${this.apiUrl}/productParam`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  updateProductDetails(payload: any) {
    return this.http.put<any>(`${this.apiUrl}/productParam`, payload, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  deleteProductDetailWithId(productId: number, paramId: number) {
    return this.http.delete<any>(`${this.apiUrl}/productParam?productId=${productId}&parameterId=${paramId}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  // exportDetails(): Observable<Blob> {
  //   return this.http.get(`${this.apiUrl}/productParam/export`, { responseType: 'blob' }).pipe(
  //     catchError(this.handleError)
  //   );
  // }

  exportDetails(selectedIds: number[]): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/productParam/export`, selectedIds, { 
      responseType: 'blob',headers: this.getHeaders() 
    }).pipe(
      catchError(this.handleError)
    );
  }

  // deleteMultipleDetails(listAry: any) {
  //   return this.http.delete<any>(`${this.apiUrl}/productParam/MultiPleDelete`, { body: { productids: listAry.productIds, parameterids: listAry.parameterIds } }).pipe(catchError(this.handleError))
  // }

  deleteMultipleDetails(listAry: any) {
    const queryParams = listAry.productIds.map((id: any) => `productids=${id}`, { headers: this.getHeaders() }).join('&') + '&' +
      listAry.parameterIds.map((id: any) => `parameterids=${id}`).join('&');

    return this.http.delete<any>(`${this.apiUrl}/productParam/MultiPleDelete?${queryParams}`)
      .pipe(catchError(this.handleError));
  }

  DownloadDetailsTemplate(): Observable<Blob> {
    return this.http
      .get(this.apiUrl + '/productParam/Download-Template', { responseType: 'blob', headers: this.getHeaders() }).pipe(
        catchError(this.handleError)
      );
  }

  importDetails(file: File,createdBy:string): Observable<any> { // Return an Observable
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(this.apiUrl + `/productParam/import?createdBy=${createdBy}`, formData, { headers: this.getHeaders() }).pipe(catchError(this.handleError));
  }
  
  getParameterList() {
    return this.http.get<any>(`${this.apiUrl}/parameter/GetAll`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  getPatameterByProductId(productId: number) {
    return this.http.get<any>(`${this.apiUrl}/parameter/getParametersByProductId?productId=${productId}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  getParameterValueByParamterId(parameterId: number) {
    return this.http.get<any>(`${this.apiUrl}/factors/getValueByParameterId?parameterId=${parameterId}`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }

  
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred!';
    console.log("error.error ", error.error)
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `${error.error.message}`;
    }
    else {
      // Server-side error
      errorMessage = `${error.error.message}`;
    }
    console.log("errorMessage ===>", errorMessage)
    return throwError(() => new Error(errorMessage));
  }
}
