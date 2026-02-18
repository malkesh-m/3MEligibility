import { Component, ViewChild } from '@angular/core';
import { LogService } from '../../../../core/services/setting/log.service';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-logs',
  standalone: false,

  templateUrl: './logs.component.html',
  styleUrl: './logs.component.scss'
})
export class LogsComponent {
  LogList: any[] = []
  dataSource = new MatTableDataSource<any>([]);
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  displayedColumns: string[] = ['id', 'message', 'level', 'timeStamp', 'exception', 'Actions'];
  totalCount = 0;
  pageIndex = 0;
  pageSize = 10;
  LoadAudit: boolean = false;
  isLoading: boolean = false;
  isUploading: boolean = false;
  isDownloading: boolean = false
  message: string = this.translate.instant("Loading data, please wait...");

  constructor(private logService: LogService, private translate: TranslateService) {

  }
  ngAfterViewInit() {
    if (this.paginator) {
      this.paginator.pageIndex = this.pageIndex;
      this.paginator.pageSize = this.pageSize;
    }
  }
  ngOnInit() {
    this.getLogs();
  }
  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.getLogs();
  }
  getLogs() {
    this.isLoading = true;
    this.logService.getLog(this.pageIndex, this.pageSize).subscribe({
      next: (response) => {
        if (response.isSuccess) {

          this.LogList = response.data.data;
          this.totalCount = response.data.totalCount;
          this.dataSource.data = this.LogList;
          if (this.paginator) {
            this.paginator.length = this.totalCount;
            this.paginator.pageIndex = this.pageIndex;
            this.paginator.pageSize = this.pageSize;
          }

          if (this.sort) {
            this.dataSource.sort = this.sort;
          }
          this.dataSource.sort = this.sort;
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.log(error.message);
        this.isLoading = false;
      }

    });
  }
}
