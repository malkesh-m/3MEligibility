import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class LogerrorService {

  private apiUrl = environment.apiUrl;
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'Log'
    });
  }
  constructor(private http: HttpClient) { }

  captureLog(payload: any) {
    return this.http.post(`${this.apiUrl}/log/frontend-error`, payload); 
  }
}
