import { Injectable, signal } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { catchError, Observable, throwError, tap } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface MakerChecker {
  makerCheckerId: number;
  makerId: number;
  checkerId: number;
  makerDate: string;
  checkerDate: string;
  tableName: string;
  actionName: string;
  oldValueJson: string;
  newValueJson: string;
  recordId: number;
  status: number;
  lastModifiedDateTime: string;
  statusName: string;
}

@Injectable({
  providedIn: 'root'
})
export class MakerCheckerService {
  private apiUrl = environment.apiUrl;
  makerCheckerList = signal<MakerChecker[]>([]);
  makerCheckerList$ = toObservable(this.makerCheckerList);
  isMakerCheckerEnabled = signal<boolean>(false);
  statusList = signal<{ id: number; name: string }[]>([]);

  constructor(private http: HttpClient) { }
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'MakerChecker'
    });
  }
  getAll(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/makerChecker/getAll`, { headers: this.getHeaders() }).pipe(
      tap(response => {
        if (response.isSuccess && response.data) {
          this.makerCheckerList.set(response.data);
        }
      }),
      catchError(this.handleError)
    );
  }

  getMakerCheckerStatuses(): void {
    this.http.get<{ id: number; name: string }[]>(`${this.apiUrl}/makerChecker/getMakerCheckerStatuses`, { headers: this.getHeaders() }).subscribe({
      next: (response) => this.statusList.set(response),
      error: (error) => console.error('Error fetching statuses:', error)
    });
  }
  updateStatus(makerCheckerId: number, statusName: string, comment: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/makerChecker/statusUpdate?id=${makerCheckerId}&statusName=${statusName}&Comment=${comment}`, { headers: this.getHeaders() }
)
      .pipe();
  }

  getStatusName(statusId: number): string {
    const status = this.statusList().find(s => s.id === statusId);
    return status ? status.name : 'Unknown';
  }

  getById(id: number): Observable<MakerChecker> {
    return this.http.get<MakerChecker>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }

  create(makerChecker: MakerChecker): Observable<any> {
    return this.http.post(`${this.apiUrl}`, makerChecker, { headers: this.getHeaders() });
  }

  update(makerChecker: MakerChecker): Observable<any> {
    return this.http.put(`${this.apiUrl}`, makerChecker, { headers: this.getHeaders() });
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}?id=${id}`);
  }

  getMakerCheckerConfig(): void {
    this.http.get<{ isSuccess: boolean; message: string; data: { isMakerCheckerEnable: boolean } }>(
      `${this.apiUrl}/setting/getbyentityid`, { headers: this.getHeaders() }
    ).subscribe({
      next: (response) => this.isMakerCheckerEnabled.set(response.data.isMakerCheckerEnable),
      error: (error) => console.error('Error fetching Maker-Checker config:', error)
    });
  }

  updateMakerCheckerConfig(isEnabled: boolean): Observable<any> {
    return this.http.put(`${this.apiUrl}/setting`, { isMakerCheckerEnable: isEnabled }, { headers: this.getHeaders() });
  }

  updateChildEntityLimitConfig(maxChildEntityLimit: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/appsetting`, { appSettingId: 1, maximumEntities: maxChildEntityLimit });
  }
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred!';
    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else {
      errorMessage = `${error.error.message}`;
    }
    return throwError(() => new Error(errorMessage));
  }
}
