import { Component, inject, ViewChild } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BulkImportService } from '../../../core/services/setting/bulk-import.service';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { AuthService } from '../../../core/services/auth/auth.service';
import { PermissionsService } from '../../../core/services/setting/permission.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-bulk-import',
  standalone: false,
  templateUrl: './bulk-import.component.html',
  styleUrl: './bulk-import.component.scss'
})
export class BulkImportComponent {
  displayedColumns: string[] = ['name', 'importTime', 'endTime', 'totalRecords', 'successCount', 'failureCount', 'actions'];
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  private _snackBar = inject(MatSnackBar);
  selectedFile: File | null = null;
  dropdownList: string[] = [
    "All",
    "Lists",
    "ListItem",
    "Parameter",
    "Factors",
    "Category",
    "Stream",
    "ERules",
    "ECards",
    "StreamCards"
  ];
  selectedList: string = 'All';
  uploadedFileName: string | null = null;
  documents = [];
  isDownloading: boolean = false;
  dataSource = new MatTableDataSource<any>([]);
  pageSize: number = 5;
  isLoading: boolean = true; // Show loader on page load
  isUploading: boolean = false;
  message: string = this.translate.instant("Loading data, please wait...");
  loggedInUser: any = null;
  createdBy: string = '';

  constructor(
    private bulkImportService: BulkImportService,
    private authService: AuthService,
    private PermissionsService: PermissionsService,
    private translate: TranslateService
  ) { }
  ngOnInit() {
    this.fetchBulkImportHistory();
  }

  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  fetchBulkImportHistory() {
    this.isLoading = true;
    this.message = this.translate.instant("Loading data, please wait...");
    this.bulkImportService.fetchBulkImportHistory().subscribe({
      next: (res: any) => {
        this.isLoading = false;
        this.dataSource.data = res.data;
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        // Set default sort
        this.dataSource.sortingDataAccessor = (item, property) => {
          switch (property) {
            case 'importTime': return new Date(item.importTime).getTime();
            default: return item[property];
          }
        };

        this.sort.active = 'importTime'; // Set default sorting column
        this.sort.direction = 'desc'; // Default sort direction
        this.dataSource.sort = this.sort;
      },
      error: (error: any) => {
        this.isLoading = false;
        this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 4000,
        });
      },
    });
  }

  downloadTemplate() {
    if (!this.isDownloading) {
      this.isDownloading = true;
      this.message = this.translate.instant("Please wait, template is downloading...");
      this.bulkImportService.downloadTemplate(this.selectedList).subscribe({
        next: (response) => {
          this.isDownloading = false;
          const blob = new Blob([response], {
            type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
          });
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = 'Bulk-Import-Template.xlsm'; // Filename for the download
          a.click();
          window.URL.revokeObjectURL(url);
          this._snackBar.open(this.translate.instant('Bulk Import Template Download Successfully.'), this.translate.instant('Okay'), {
            duration: 4000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
        },
        error: (error: any) => {
          this.isDownloading = false
          this._snackBar.open(this.translate.instant('Failed to Download Template'), this.translate.instant('Okay'), {
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
        },
      });
    }
  }

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
    event.target.value = '';

    if (this.selectedFile) {
      const fileExtension = this.selectedFile.name.split('.').pop()?.toLowerCase();

      if (fileExtension !== 'xlsx' && fileExtension !== 'xlsm') {
        this._snackBar.open(this.translate.instant('File format not allowed.'), this.translate.instant('Okay'), {
          duration: 4000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.selectedFile = null;
        return;
      }

      this.uploadedFileName = this.selectedFile.name;
      this.BulkImport(this.selectedFile);
    } else {
      this._snackBar.open(this.translate.instant('Please select a file first.'), this.translate.instant('Okay'), {
        duration: 4000,
        horizontalPosition: 'right',
        verticalPosition: 'top',
      });
    }
  }

  BulkImport(selectedFile: File) {
    this.authService.currentUser$.subscribe((user) => {
      this.loggedInUser = user;
    });
    this.createdBy = this.loggedInUser.user.userName;
    this.isUploading = true;
    this.message = this.translate.instant("Uploading file, please wait...");
    this.bulkImportService.bulkImport(selectedFile).subscribe({
      next: (response) => {
        this.isUploading = false;
        this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 4000,
        });
        this.fetchBulkImportHistory();
      },
      error: (error) => {
        this.isUploading = false;
        this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 4000,
        });
      },
    });
  }

  downloadFile(doc: any) {
    this.isDownloading = true;
    this.bulkImportService.downloadFile(doc.id).subscribe({
      next: (response: Blob) => {
        this.isDownloading = false;
        const url = window.URL.createObjectURL(response);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${doc.name}`; // Dynamic filename based on document name
        a.click();
        window.URL.revokeObjectURL(url);
        this._snackBar.open(this.translate.instant('File downloaded successfully.'), this.translate.instant('Okay'), {
          duration: 4000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (error: any) => {
        this.isDownloading = false;
        this._snackBar.open(this.translate.instant('Failed to download file.'), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 4000,
        });
      },
    });
  }
}



