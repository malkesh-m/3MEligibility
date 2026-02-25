import { Component, Inject, ViewChild } from '@angular/core';
import { AuditService } from '../../../../core/services/setting/audit.service';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MakerCheckerService } from '../../../../core/services/setting/makerchecker.service';
import { PermissionsService } from '../../../../core/services/setting/permission.service';
import { TranslateService } from '@ngx-translate/core';
import { MakerCheckerDetailsDialogComponent } from '../../maker-checker/maker-checker-details-dialog/maker-checker-details-dialog.component';

@Component({
  selector: 'app-audit-log',
  standalone: false,

  templateUrl: './audit-log.component.html',
  styleUrl: './audit-log.component.scss'
})
export class AuditLogComponent {
  auditLogList: any[] = []
  dataSource = new MatTableDataSource<any>([]);
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  parsedOldJson: any;
  parsedNewJson: any;
  comment: string = '';
  //LoadAudit: boolean = false;
  //rowcount !: number;
  //pageindex: number = 0;
  //pagesize: number = 10;
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  LoadAudit = false;
  isLoading: boolean = false;
  isUploading: boolean = false
  isDownloading: boolean = false
  message: string = this.translate.instant("Loading data, please wait...");

  parseJson(jsonString: string): any {
    try {
      return JSON.parse(jsonString);
    } catch (e) {
      return { error: 'Invalid JSON' };
    }
  }


  constructor(
    private dialog: MatDialog,
    private auditservice: AuditService,
    private translate: TranslateService
  ) { }
  ngOnInit() {
    this.getAuditLog();
  }
  displayedColumns: string[] = ['recordId', 'actionName', 'actionDate', 'tableName', 'ipAddress', 'userName', 'updatedByDateTime', 'Actions'];
  openDetailsDialog(row: any): void {
    const dialogRef = this.dialog.open(MakerCheckerDetailsDialogComponent, {
      width: '80vw',
      maxWidth: '1000px',
      data: {
        ...row,
        isMakerChecker: false
      }
    });
    dialogRef.afterClosed().subscribe(result => {

      //  this.getAuditLog();
    });
  }
  //ngAfterViewInit() {
  //  if (this.paginator) {
  //    this.paginator.pageIndex = this.pageIndex;
  //    this.paginator.pageSize = this.pageSize;
  //  }
  //}
  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.getAuditLog();
  }
  ngAfterViewInit() {
    if (this.paginator) {
      this.paginator.pageIndex = this.pageIndex;
      this.paginator.pageSize = this.pageSize;
    }
  }
  getAuditLog() {
    this.isLoading = true;

    this.auditservice.getAuditLog(this.pageIndex, this.pageSize).subscribe({
      next: (response) => {
        console.log(response.data);
        if (response.isSuccess && response.data) {
          this.auditLogList = response.data.data;
          this.dataSource.data = response.data.data;
          this.totalCount = response.data.totalCount;
          if (this.paginator) {
            this.paginator.length = this.totalCount;
            this.paginator.pageIndex = this.pageIndex;
            this.paginator.pageSize = this.pageSize;
          }
          if (this.sort) {
            this.dataSource.sort = this.sort;
          }
        }

        this.dataSource.sort = this.sort;
        this.isLoading = false;
      },
      error: (error) => {
        console.log(error.message);
        this.isLoading = false;
      }
    });
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }
}


