import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { catchError } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class UserActivityService {
  private apiUrl = environment.apiUrl;
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'UserActivity'
    });
  }
  constructor(private http: HttpClient, private router: Router) { }
  logAction(actionType: string, actionName: string, componentName: string) {
    const payload = {
      actionType,      
      actionName,      
      componentName,   
      pageUrl: this.router.url,
    };
    this.http.post(`${this.apiUrl}/log/loguseractivity`, payload).subscribe();
  }
}
