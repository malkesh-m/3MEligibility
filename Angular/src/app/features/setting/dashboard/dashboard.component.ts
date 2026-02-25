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
  evaluationHistory: any = [];
  userName: string = 'Admin'; // Fallback

  getGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good Morning';
    if (hour < 17) return 'Good Afternoon';
    return 'Good Evening';
  }

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
    const fromDateIso = this.filter.fromDate ? new Date(this.filter.fromDate).toISOString() : undefined;
    const toDateIso = this.filter.toDate ? new Date(this.filter.toDate).toISOString() : undefined;

    console.log('Fetching Evaluation History with filter:', this.filter);
    const request = {
      ...this.filter,
      fromDate: fromDateIso,
      toDate: toDateIso,
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
    this.pageNumber = 1; // Reset to first page on filter change
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
          { name: 'Avg. Processing Time', count: (res.avgProcessingTime ?? 0) + ' s', icon: 'timer', footer: 'Evaluation Duration' },
          { name: 'Top Failure Reasons', count: res.topFailureReason, icon: 'error', footer: 'Most Frequent', class: 'small-text wide-tile' },
          //{ name: 'Avg. Approved ScorTriggerede', count: res.avgApprovedScore ?? 'N/A', icon: 'insights', footer: 'MOZN Score Avg' },

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
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    if (this.passFailTrendChart) this.passFailTrendChart.destroy();

    // Create Gradients
    const passGradient = ctx.createLinearGradient(0, 0, 0, 400);
    passGradient.addColorStop(0, 'rgba(16, 185, 129, 0.2)');
    passGradient.addColorStop(1, 'rgba(16, 185, 129, 0)');

    const failGradient = ctx.createLinearGradient(0, 0, 0, 400);
    failGradient.addColorStop(0, 'rgba(244, 63, 94, 0.2)');
    failGradient.addColorStop(1, 'rgba(244, 63, 94, 0)');

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
              {
                label: '✅ Pass',
                data: passData,
                borderColor: '#10B981', // Emerald
                borderWidth: 4,
                backgroundColor: passGradient,
                fill: true,
                tension: 0.4,
                pointRadius: 0,
                pointHoverRadius: 8,
                pointBackgroundColor: '#10B981',
                pointBorderColor: '#FFFFFF',
                pointBorderWidth: 2
              },
              {
                label: '❌ Fail',
                data: failData,
                borderColor: '#F43F5E', // Rose
                borderWidth: 4,
                backgroundColor: failGradient,
                fill: true,
                tension: 0.4,
                pointRadius: 0,
                pointHoverRadius: 8,
                pointBackgroundColor: '#F43F5E',
                pointBorderColor: '#FFFFFF',
                pointBorderWidth: 2
              }
            ]
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
              legend: {
                position: 'top',
                align: 'end',
                labels: {
                  usePointStyle: true,
                  boxWidth: 8,
                  padding: 20,
                  color: '#64748B',
                  font: { family: 'Sora', size: 12, weight: 600 }
                }
              },
              tooltip: {
                backgroundColor: '#1E293B',
                titleFont: { family: 'Sora', size: 13 },
                bodyFont: { family: 'Sora', size: 12 },
                padding: 12,
                cornerRadius: 8,
                displayColors: true
              }
            },
            scales: {
              y: {
                beginAtZero: true,
                grid: { color: '#F1F5F9', drawTicks: false },
                ticks: { color: '#94A3B8', font: { family: 'Sora', size: 11 }, padding: 10 }
              },
              x: {
                grid: { display: false },
                ticks: { color: '#94A3B8', font: { family: 'Sora', size: 11 }, padding: 10 }
              }
            }
          }
        });
      },
      error: (err) => {
        console.error("Error loading monthly summary:", err);
      }
    });
  }
  createFailureReasonsChart() {
    const canvas = document.getElementById('failureReasonsChart') as HTMLCanvasElement;
    if (!canvas) return;

    if (this.failureReasonsChart) this.failureReasonsChart.destroy();

    this.dashboardService.getFailureReasonsSummary().subscribe({
      next: (response) => {
        const labels = response.map((r: { reason: any; }) => {
          switch (r.reason) {
            case 'Score': return 'Credit Score';
            case 'Customer Salary': return 'Income';
            case 'Multiple Defaults': return 'Multiple Defaults';
            default: return r.reason;
          }
        });

        const data = response.map((r: { count: any; }) => r.count);

        this.failureReasonsChart = new Chart(canvas, {
          type: 'bar',
          data: {
            labels: labels,
            datasets: [{
              label: 'Rejection',
              data: data,
              backgroundColor: [
                '#6366F1', // Indigo
                '#8B5CF6', // Violet
                '#EC4899', // Pink
                '#F59E0B'  // Amber
              ],
              borderRadius: 12,
              barThickness: 48
            }]
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
              legend: { display: false },
              tooltip: {
                backgroundColor: '#1E293B',
                padding: 12,
                cornerRadius: 8
              }
            },
            scales: {
              y: {
                beginAtZero: true,
                grid: { color: '#F1F5F9', drawTicks: false },
                ticks: { color: '#94A3B8', font: { family: 'Sora' }, padding: 10 }
              },
              x: {
                grid: { display: false },
                ticks: { color: '#94A3B8', font: { family: 'Sora' }, padding: 10 }
              }
            }
          }
        });
      },
      error: (err) => {
        console.error('Failed to load Failure Reasons Data:', err);
      }
    });
  }

  createProcessingTimeChart(): void {
    this.dashboardService.getProcessingTimeDistribution().subscribe({
      next: (response) => {
        const labels = response.map((item: any) => item.range);
        const data = response.map((item: any) => item.count);

        const canvas = document.getElementById('processingTimeChart') as HTMLCanvasElement;
        if (!canvas) return;
        if (this.processingTimeChart) this.processingTimeChart.destroy();

        this.processingTimeChart = new Chart(canvas, {
          type: 'bar',
          data: {
            labels: labels,
            datasets: [{
              label: 'Evaluations',
              data: data,
              backgroundColor: 'rgba(99, 102, 241, 0.2)', // Indigo dim
              borderColor: '#6366F1',
              borderWidth: 2,
              hoverBackgroundColor: '#6366F1',
              borderRadius: 8,
              barThickness: 50
            }]
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
              legend: { display: false },
              tooltip: {
                backgroundColor: '#1E293B',
                padding: 12,
                cornerRadius: 8
              }
            },
            scales: {
              y: {
                beginAtZero: true,
                grid: { color: '#F1F5F9', drawTicks: false },
                ticks: { color: '#94A3B8', font: { family: 'Sora' }, padding: 10 }
              },
              x: {
                grid: { display: false },
                ticks: { color: '#94A3B8', font: { family: 'Sora' }, padding: 10 }
              }
            }
          }
        });
      },
      error: (err) => {
        console.error('Failed to load Processing Time Distribution:', err);
      }
    });
  }
  createCustomerSegmentChart() {
    const canvas = document.getElementById('customerSegmentChart') as HTMLCanvasElement;
    if (!canvas) return;
    if (this.customerSegmentChart) this.customerSegmentChart.destroy();

    this.customerSegmentChart = new Chart(canvas, {
      type: 'bar',
      data: {
        labels: ['Beginner', 'Intermediate', 'Expert'],
        datasets: [
          {
            label: '✅ Pass',
            data: [80, 95, 120],
            backgroundColor: '#10B981',
            borderRadius: 4,
            barThickness: 20
          },
          {
            label: '❌ Fail',
            data: [15, 25, 10],
            backgroundColor: '#F43F5E',
            borderRadius: 4,
            barThickness: 20
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'top',
            align: 'end',
            labels: { usePointStyle: true, boxWidth: 6, font: { family: 'Sora', size: 11 } }
          }
        },
        scales: {
          y: {
            beginAtZero: true,
            grid: { color: '#F1F5F9', drawTicks: false },
            ticks: { color: '#94A3B8', font: { family: 'Sora', size: 10 } }
          },
          x: {
            grid: { display: false },
            ticks: { color: '#94A3B8', font: { family: 'Sora', size: 10 } }
          }
        }
      }
    });
  }

  createRiskScoreChart() {
    const canvas = document.getElementById('riskScoreChart') as HTMLCanvasElement;
    if (!canvas) return;
    if (this.riskScoreChart) this.riskScoreChart.destroy();

    this.riskScoreChart = new Chart(canvas, {
      type: 'scatter',
      data: {
        datasets: [
          {
            label: '✅ Pass',
            data: [{ x: 101, y: 750 }, { x: 102, y: 680 }, { x: 108, y: 710 }, { x: 115, y: 790 }],
            backgroundColor: '#10B981'
          },
          {
            label: '❌ Fail',
            data: [{ x: 201, y: 450 }, { x: 205, y: 480 }, { x: 210, y: 420 }, { x: 220, y: 460 }],
            backgroundColor: '#F43F5E'
          }
        ]
      },
      options: {
        responsive: true,
        plugins: {
          legend: {
            position: 'top',
            align: 'end',
            labels: { usePointStyle: true, boxWidth: 6, font: { family: 'Sora', size: 11 } }
          }
        },
        scales: {
          y: {
            title: { display: true, text: 'Credit Score', font: { family: 'Sora', size: 11, weight: 600 } },
            grid: { color: '#F1F5F9' },
            ticks: { font: { family: 'Sora', size: 10 } }
          },
          x: {
            title: { display: true, text: 'Evaluation ID', font: { family: 'Sora', size: 11, weight: 600 } },
            grid: { color: '#F1F5F9' },
            ticks: { font: { family: 'Sora', size: 10 } }
          }
        }
      }
    });
  }

  updateCharts() {
    this.createPassFailTrendChart();
    this.createFailureReasonsChart();
    this.createCustomerSegmentChart();
    this.createProcessingTimeChart();
    this.createRiskScoreChart();
  }

  openApiDetails(evaluationHistoryId: number): void {
    const dialogRef = this.dialog.open(ApiDetailsDialogComponent, {
      width: '1500px',
      maxWidth: '95vw',
      height: '90vh',
      panelClass: 'premium-dialog',
      data: { evaluationHistoryId }
    });
  }
}
