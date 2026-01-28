// Updated Dashboard Component with API Integration and RxJS Subscription
import { AfterViewInit, Component, Injectable } from '@angular/core';
import { Chart } from 'chart.js/auto';
import { MatDialog } from '@angular/material/dialog';
import { PageEvent } from '@angular/material/paginator';

import { forkJoin } from 'rxjs';
import { DashboardService } from '../../../core/services/setting/dashboard.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiDetailsDialogComponent } from './api-details-dialog/api-details-dialog.component';

export interface EvaluationHistoryFilter {

  searchText?: string;
  decision?: string;
  failureReason?: string;
  fromDate?: string;
  toDate?: string;
  pageNumber?: number;
  pageSize?: number;
}
@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})

@Injectable({
  providedIn: 'root'
})
export class DashboardComponent implements AfterViewInit {
  [x: string]: any;
  //filter: EvaluationHistoryFilter = {};
  filter: EvaluationHistoryFilter = {
    searchText: '',
    decision: '',  // This will make "All Decisions" selected
    failureReason: '',  // This will make "All Reasons" selected
  };
  constructor(private dashboardService: DashboardService, private _snackBar: MatSnackBar, private dialog: MatDialog) { }
  pageNumber = 1;
  pageSize = 10;
  totalRecords = 0;
  years: number[] = [];
  monthlyData: any[] = [];
  selectedYear: number = new Date().getFullYear();
  isLoadingTiles = false;
  apiEvaluationHistory: any[] = [];
  dashboardTiles: any[] = [];
  passFailTrendChart!: Chart;
  failureReasonsChart!: Chart;
  customerSegmentChart!: Chart;
  processingTimeChart!: Chart;
  riskScoreChart!: Chart;
  evaluationHistory:any = [];
 
  //evaluationHistory = [
  //  {
  //    customerId: 'CUST001',
  //    timestamp: '2025-05-06 09:15:00',
  //    outcome: 'Approved - Personal Loan ₹1,00,000',
  //    failureReason: '',
  //    creditScore: 710,
  //    previousApplications: 2
  //  },
  //  {
  //    customerId: 'CUST002',
  //    timestamp: '2025-05-06 09:25:00',
  //    outcome: 'Rejected',
  //    failureReason: 'Low Credit Score',
  //    creditScore: 520,
  //    previousApplications: 4
  //  },
  //  {
  //    customerId: 'CUST003',
  //    timestamp: '2025-05-06 10:00:00',
  //    outcome: 'Approved - Credit Card ₹50,000',
  //    failureReason: '',
  //    creditScore: 690,
  //    previousApplications: 1
  //  }
  //];
  getEvaluationHistory(): void {
    // Include pagination info in the filter
 this.filter.fromDate ? new Date(this.filter.fromDate).toISOString() : undefined,
 this.filter.toDate ? new Date(this.filter.toDate).toISOString() : undefined
 console.log('Fetching Evaluation History with filter:', this.filter);
const request = {
      ...this.filter,
      pageNumber: this.pageNumber ?? 1,
      pageSize: this.pageSize ?? 10
    };

    this.dashboardService.getEvaluationHistory(request).subscribe({
      next: (response) => {
        this.evaluationHistory = response.data; // assign only the data array
        this.totalRecords = response.totalCount; // store total count for pagination
      },
      error: (err) => {
        console.error('Failed to load Evaluation History:', err);
      }
    });
  }

  ngAfterViewInit() {
    this.fetchTileData();
    this.loadPassFailTrend();   // <-- This loads data and calls chart with 3 params

    this.createFailureReasonsChart();
    this.createCustomerSegmentChart();
    this.createProcessingTimeChart();
    this.createRiskScoreChart();
    this.getEvaluationHistory();
  //  this.fetchApiEvaluationHistory();
  }
  onPageChange(event: PageEvent) {
    this.pageNumber = event.pageIndex + 1; // paginator index is 0-based
    this.pageSize = event.pageSize;
    this.getEvaluationHistory();
  }
  applyFilters() {

    this.getEvaluationHistory();
  }
  fetchTileData(): void {
    this.isLoadingTiles = true;
    forkJoin({
      customersEvaluated: this.dashboardService.getCustomersEvaluated(),
      approvalRate: this.dashboardService.getApprovalRate(),
      rejectionRate: this.dashboardService.getRejectionRate(),
       avgProcessingTime: this.dashboardService.getAvgTimeProcessing(),
     // processingTime: this.dashboardService.getProcessingTimeDistribution(),
      topFailureReason: this.dashboardService.getTopFailureReason(),
      //avgApprovedScore: this.dashboardService.getAvgApprovedScore(),
      //failureReasonsSummary: this.dashboardService.getFailureReasonsSummary()
    }).subscribe({
      next: (res) => {
        console.log('Dashboard tile data:', res.topFailureReason);
        this.dashboardTiles = [
          { name: 'Customers Evaluated', count: res.customersEvaluated ?? 0, icon: 'person_search', footer: 'Today / Month / Year' },
          { name: 'Eligibility Rate', count: (res.approvalRate ?? 0) + '%', icon: 'check_circle', footer: 'Eligibility Rate' },
          { name: 'Non-Eligibility Rate', count: (res.rejectionRate ?? 0) + '%', icon: 'cancel', footer: 'Ineligibility Rate' },
          { name: 'Avg. Processing Time', count: (res.avgProcessingTime??0)+' s', icon: 'timer', footer: 'Evaluation Duration' },
          { name: 'Top Failure Reasons', count: res.topFailureReason, icon: 'error', footer: 'Most Frequent', class: 'small-text wide-tile'},
          { name: 'Re-evaluations Triggered', count: 52, icon: 'autorenew', footer: 'Auto Triggers' },
          //{ name: 'Avg. Approved Score', count: res.avgApprovedScore ?? 'N/A', icon: 'insights', footer: 'MOZN Score Avg' },

          //{ name: 'Avg. Processing Time', count: (res.processingTime ?? 0) + 's', icon: 'timer', footer: 'Evaluation Duration' },
          //{ name: 'Top Failure Reasons', count: res.topFailureReason ?? 'N/A', icon: 'error', footer: 'Most Frequent' },
          //{ name: 'Re-evaluations Triggered', count: res.failureReasonsSummary?.reEvaluations ?? 0, icon: 'autorenew', footer: 'Auto Triggers' }
        ];
      },
      error: (err) => {
        console.error('Dashboard tile API error:', err);
      },
      complete: () => {
        this.isLoadingTiles = false;
      }
    });
  }

  passFailTrend: any[] = [];

  ngOnInit() {
    this.loadPassFailTrend();
    this.generateYears();

  }
  generateYears() {
    const current = new Date().getFullYear();
    for (let y = current; y >= current - 5; y--) {
      this.years.push(y);
    }
  }
  onYearChange(event: any) {
    this.selectedYear = event.target.value;
    this.createPassFailTrendChart();
  }
  loadPassFailTrend() {
    const year = this.selectedYear;

    this.dashboardService.getMonthlySummary(year).subscribe({
      next: (response) => {

        const labels = response.map(m => m.month);
        const passData = response.map(m => m.passed);
        const failData = response.map(m => m.failed);

        this.createPassFailTrendChart();
      },
      error: (err) => {
        console.error("Error loading monthly summary:", err);
      }
    });
  }

  createPassFailTrendChart() {
    const canvas = document.getElementById('passFailTrendChart') as HTMLCanvasElement;

    if (this.passFailTrendChart) this.passFailTrendChart.destroy();

    this.dashboardService.getMonthlySummary(this.selectedYear).subscribe({
      next: (data) => {

        const labels = data.map(x => x.month);
        const passData = data.map(x => x.approvedCount);
        const failData = data.map(x => x.rejectedCount);

        this.passFailTrendChart = new Chart(canvas, {
          type: 'line',
          data: {
            labels: labels,
            datasets: [
              { label: '✅ Pass', data: passData, borderColor: 'green', fill: false },
              { label: '❌ Fail', data: failData, borderColor: 'red', fill: false }
            ]
          },
          options: { responsive: true }
        });
      },
      error: (err) => {
        console.error("Error loading monthly summary:", err);
      }
    });
  }
  createFailureReasonsChart() {
    const canvas = document.getElementById('failureReasonsChart') as HTMLCanvasElement;

    // Destroy previous chart if exists
    if (this.failureReasonsChart) this.failureReasonsChart.destroy();

    this.dashboardService.getFailureReasonsSummary().subscribe({
      next: (response) => {

        // Map response to chart labels and data
        const labels = response.map((r: { reason: any; }) => {
          switch (r.reason) {
            case 'Score': return 'Credit Score';
            case 'Customer Salary': return 'Income';
            case 'Multiple Defaults': return 'Multiple Defaults';
            default: return r.reason;
          }
        });

        const data = response.map((r: { count: any; }) => r.count);

        const backgroundColors = labels.map((label: any) => {
          switch (label) {
            case 'Low Credit Score': return '#e74c3c';
            case 'Insufficient Income': return '#f39c12';
            case 'Multiple Defaults': return '#8e44ad';
            default: return '#3498db';
          }
        });

        // Create the chart
        this.failureReasonsChart = new Chart(canvas, {
          type: 'bar',
          data: {
            labels: labels,
            datasets: [{
              label: 'Rejection ',
              data: data,
              backgroundColor: backgroundColors
            }]
          },
          options: { responsive: true }
        });
      },
      error: (err) => {
        console.error('Failed to load Failure Reasons Data:', err);
      }
    });
  }


  createCustomerSegmentChart() {
    const canvas = document.getElementById('customerSegmentChart') as HTMLCanvasElement;
    if (this.customerSegmentChart) this.customerSegmentChart.destroy();

    this.customerSegmentChart = new Chart(canvas, {
      type: 'bar',
      data: {
        labels: ['Beginner', 'Intermediate', 'Expert'],
        datasets: [
          { label: '✅ Pass', data: [80, 90, 120], backgroundColor: 'green' },
          { label: '❌ Fail', data: [20, 30, 10], backgroundColor: 'red' }
        ]
      },
      options: { responsive: true }
    });
  }

  // createProcessingTimeChart() {
  //   const canvas = document.getElementById('processingTimeChart') as HTMLCanvasElement;
  //   if (this.processingTimeChart) this.processingTimeChart.destroy();

  //   this.processingTimeChart = new Chart(canvas, {
  //     type: 'bar',
  //     data: {
  //       labels: ['0-2s', '2-5s', '5-10s', '10-20s', '20s+'],
  //       datasets: [{
  //         label: 'Evaluations',
  //         data: [60, 100, 200, 150, 40],
  //         backgroundColor: '#3498db'
  //       }]
  //     },
  //     options: { responsive: true }
  //   });
  // }

  createProcessingTimeChart(): void {
    this.dashboardService.getProcessingTimeDistribution().subscribe({
      next: (response) => {
        const labels = response.map((item: any) => item.range);
        const data = response.map((item: any) => item.count);

        const canvas = document.getElementById('processingTimeChart') as HTMLCanvasElement;
        if (this.processingTimeChart) this.processingTimeChart.destroy();

        this.processingTimeChart = new Chart(canvas, {
          type: 'bar',
          data: {
            labels: labels,
            datasets: [{
              label: 'Evaluations',
              data: data,
              backgroundColor: '#3498db'
            }]
          },
          options: { responsive: true }
        });
      },
      error: (err) => {
        console.error('Failed to load Processing Time Distribution:', err);
      }
    });
  }
  fetchApiEvaluationHistory(): void {
    this.dashboardService.getApiEvaluationHistory(1).subscribe({
      next: (response) => {
        this.apiEvaluationHistory =
          Array.isArray(response.data) ? response.data : [];
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });      },
      error: (err) => {
        console.error('Failed to load API Evaluation History:', err);
      }
    });
  }
  createRiskScoreChart() {
    const canvas = document.getElementById('riskScoreChart') as HTMLCanvasElement;
    if (this.riskScoreChart) this.riskScoreChart.destroy();

    this.riskScoreChart = new Chart(canvas, {
      type: 'scatter',
      data: {
        datasets: [
          {
            label: '✅ Pass',
            data: [
              { x: 101, y: 750 }, { x: 102, y: 680 }, { x: 103, y: 700 }
            ],
            backgroundColor: 'green'
          },
          {
            label: '❌ Fail',
            data: [
              { x: 201, y: 450 }, { x: 202, y: 480 }, { x: 203, y: 460 }
            ],
            backgroundColor: 'red'
          }
        ]
      },
      options: { responsive: true }
    });
  }

  updateCharts() {
    this.loadPassFailTrend();
    this.createFailureReasonsChart();
    this.createCustomerSegmentChart();
    this.createProcessingTimeChart();
    this.createRiskScoreChart();
  }
  openApiDetails(evaluationHistoryId: number): void {
    const dialogRef = this.dialog.open(ApiDetailsDialogComponent, {
      width: '1500px',
      maxWidth: '110vw',
      height: '150vh',
      panelClass: 'compact-dialog', // Add this
      data: { evaluationHistoryId }
    });
  }

  //drillDown(customerId: string) {
  //  alert(`Opening full evaluation history for: ${customerId}`);
  //}
}
