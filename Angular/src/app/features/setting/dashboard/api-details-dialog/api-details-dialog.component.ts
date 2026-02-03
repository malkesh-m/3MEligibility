import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { DashboardService } from '../../../../core/services/setting/dashboard.service';
export interface ApiEvaluationHistory {
  id: number;
  evaluationHistoryId: number;
  nodeApiId: number;
  apiName: string;
  apiRequest: string;
  apiResponse: string;
  evaluationTimeStamp: string;
  formattedRequest?: string;
  formattedResponse?: string;
}
@Component({
  selector: 'app-api-details-dialog',
  standalone: false,

  templateUrl: './api-details-dialog.component.html',
  styleUrl: './api-details-dialog.component.scss'
})
export class ApiDetailsDialogComponent {
  apiDetails: ApiEvaluationHistory[] = [];
  loading = true;
   BreData: any;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: { evaluationHistoryId: number },
    private dashboardService: DashboardService
  ) { }
  ngOnInit(): void {
    this.loadApiDetails();
    this.getBreApi()
  }
  getBreApi() {
    this.dashboardService.getEvaluationHistoryById(this.data.evaluationHistoryId).subscribe({
      next: (res) => {
        this.BreData = res.data;
        console.log('BRE API Response:', res);

      },
    });

  }

  loadApiDetails(): void {
    this.dashboardService.getApiEvaluationHistory(this.data.evaluationHistoryId)
      .subscribe({
        next: (res) => {
          const list: ApiEvaluationHistory[] = Array.isArray(res.data) ? res.data : [];

          // Format JSON strings for display
          this.apiDetails = list.map((x: ApiEvaluationHistory) => ({
            ...x,
            formattedRequest: this.formatJson(x.apiRequest),
            formattedResponse: this.formatJson(x.apiResponse)
          }));
          

          this.loading = false;
        },
        error: (err) => {
          console.error("Error fetching API details: ", err);
          this.loading = false;
        }
      });
  }

  // Pretty JSON formatter
  formatJson(jsonString: string): string {
    try {
      return JSON.stringify(JSON.parse(jsonString), null, 4);
    } catch {
      return jsonString; // fallback if it's not valid JSON
    }
  }
}
