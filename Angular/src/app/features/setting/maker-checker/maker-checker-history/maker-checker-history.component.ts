import { Component, OnInit, ViewChild } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { MatDialog } from '@angular/material/dialog';
import { MakerCheckerService, MakerChecker } from '../../../../core/services/setting/makerchecker.service';
import { MakerCheckerDetailsDialogComponent } from '../maker-checker-details-dialog/maker-checker-details-dialog.component';
export interface MakerCheckerHistory {
  makerCheckerId: number;
  tableName: string;
  actionName: string;
  makerName: string;
  makerDate: string;
  checkerName: string;
  checkerDate: string;
  status: number;
  statusName: string;
}

@Component({
  selector: 'app-maker-checker-history',
  standalone: false,
  templateUrl: './maker-checker-history.component.html',
  styleUrl: './maker-checker-history.component.scss'
})
export class MakerCheckerHistoryComponent implements OnInit {
  [x: string]: any;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  isLoading: boolean = false;
  message: string = this.translate.instant("Loading data, please wait...")
  isUploading: boolean = false;
  isDownloading: boolean = false;

  displayedColumns: string[] = ['recordId', 'tableName', 'actionName', 'makerName', 'makerDate', 'checkerName', 'checkerDate', 'status', 'comment', 'actions'];
  dataSource = new MatTableDataSource<any>([]);
  //dialog: any;

  constructor(private dialog: MatDialog, private service: MakerCheckerService, private translate: TranslateService) { }

  ngOnInit(): void {
    this.loadHistory();
  }
  openDetailsDialog(row: any): void {
    const dialogRef = this.dialog.open(MakerCheckerDetailsDialogComponent, {
      width: '80vw', // Expands dialog width to 80% of viewport
      maxWidth: '1000px', // Maximum width limit
      data: {
        ...row,
        isMakerChecker: true,
        fromhistory: true
      }

    });
    dialogRef.afterClosed().subscribe(() => {

    });
  }



  loadHistory(): void {
    this.isLoading = true;
    this.service.getAll().subscribe(response => {
      if (response && response.data && Array.isArray(response.data)) {
        // Filter only Approved (2) and Declined (3) statuses
        this.dataSource.data = response.data.filter((item: MakerCheckerHistory) => item.status === 2 || item.status === 3);
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.isLoading = false;
      } else {

        console.error('Invalid response:', response);
        this.isLoading = false;
      }
    });
  }
}
