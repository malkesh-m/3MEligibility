import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { EvaluationHistoryFilter } from '../../../features/setting/dashboard/dashboard.component';

@Injectable({
    providedIn: 'root'
})
export class DashboardService {

    private apiUrl = environment.apiUrl;

    constructor(private http: HttpClient) { }
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'Dashboard'
    });
  }
    /**
     * Dashboard Crud api services
     **/


  getMonthlySummary(year: number) {
    return this.http.get<any[]>(`${this.apiUrl}/dashboard/monthly-summary?year=${year}`);
  }


    getFailureReasonsSummary() {
      return this.http.get<any>(`${this.apiUrl}/dashboard/failure-reasons`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
    }
  getEvaluationHistoryById(id: number){
    return this.http.get < any > (`${this.apiUrl}/evalutionhistory/${id}`);
  }

  getEvaluationHistory(filter: EvaluationHistoryFilter) {
    return this.http.post<any>(`${this.apiUrl}/dashboard/evaluation-history`, filter, {
      headers: this.getHeaders()
    }).pipe(catchError(this.handleError));
  }
    getProcessingTimeDistribution() {
      return this.http.get<any>(`${this.apiUrl}/dashboard/processing-time-distribution`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
    }

    getCustomersEvaluated() {
      return this.http.get<any>(`${this.apiUrl}/dashboard/customers-evaluated`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
    }

    getApprovalRate() {
      return this.http.get<any>(`${this.apiUrl}/dashboard/approval-rate`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
    }

    getRejectionRate() {
      return this.http.get<any>(`${this.apiUrl}/dashboard/rejection-rate`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
    }

    getTopFailureReason() {
      return this.http.get(`${this.apiUrl}/dashboard/top-failure-reason`, { headers: this.getHeaders(), responseType: 'text' }).pipe(catchError(this.handleError))
    }

    getAvgApprovedScore() {
      return this.http.get<any>(`${this.apiUrl}/dashboard/avg-approved-score`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
    }
  getAvgTimeProcessing() {
    return this.http.get<any>(`${this.apiUrl}/dashboard/avg-processing-time`, { headers: this.getHeaders() }).pipe(catchError(this.handleError))
  }
  getApiEvaluationHistory(EvaluationHistoryId: number) {
    return this.http.get<any>(
      `${this.apiUrl}/dashboard/apievaluationhistory?evaluationHistoryId=${EvaluationHistoryId}`,
      { headers: this.getHeaders() }
    );
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
