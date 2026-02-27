import { Component, Inject, ViewChild } from '@angular/core';
import { AuditService } from '../../../../core/services/setting/audit.service';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort, SortDirection } from '@angular/material/sort';
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
  readonly defaultSortColumn = 'actionDate';
  readonly defaultSortDirection: SortDirection = 'desc';

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
    this.dataSource.sortingDataAccessor = (item, property) => {
      if (property?.toLowerCase().includes('date')) {
        return this.parseAuditTimestamp(item[property]);
      }
      return item[property];
    };
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
    this.initializeSort();
  }
  getAuditLog() {
    this.isLoading = true;

    this.auditservice.getAuditLog(this.pageIndex, this.pageSize).subscribe({
        next: (response) => {
          console.log(response.data);
        if (response.isSuccess && response.data) {
          const stamped = response.data.data.map((row: any) => ({
            ...row,
            __actionTimestamp: this.parseAuditTimestamp(row.actionDate || row.actionDateTime)
          }));
          const sorted = [...stamped].sort((a, b) => b.__actionTimestamp - a.__actionTimestamp);
          this.auditLogList = sorted;
          this.dataSource.data = sorted;
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
          this.applyDefaultSort();
          this.isLoading = false;
        },
        error: (error: any) => {
          console.log(error?.message || error);
          this.isLoading = false;
        }
      });
  }

  private parseAuditTimestamp(value?: string | number | null): number {
    if (value == null) {
      return 0;
    }

    const asString = typeof value === 'string' ? value : String(value);
    const guess = new Date(asString);
    if (!Number.isNaN(guess.getTime())) {
      return guess.getTime();
    }

    const normalized = asString
      .replace(/,\s*/g, ' ')
      .replace(/(\d{1,2})\/(\d{1,2})\/(\d{2})(?!\d)/, match => {
        const parts = match.split('/');
        const year = Number(parts[2]);
        const expanded = year < 100 ? 2000 + year : year;
        return `${parts[0]}/${parts[1]}/${expanded}`;
      });

    const parsed = new Date(normalized);
    return Number.isNaN(parsed.getTime()) ? 0 : parsed.getTime();
  }

  private initializeSort() {
    this.dataSource.sort = this.sort;
    if (this.sort) {
      this.applyDefaultSort();
    }
  }

  private applyDefaultSort() {
    if (!this.sort) {
      return;
    }

    queueMicrotask(() => {
      this.sort.active = this.defaultSortColumn;
      this.sort.direction = this.defaultSortDirection;
      this.sort.sortChange.emit({ active: this.defaultSortColumn, direction: this.defaultSortDirection });
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

