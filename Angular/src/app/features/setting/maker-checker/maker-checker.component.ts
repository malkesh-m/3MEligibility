import { AfterViewInit, Component, effect, inject, OnInit, Signal, ViewChild } from '@angular/core';

//import { Observable } from 'rxjs';
import { MakerChecker, MakerCheckerService } from '../../../core/services/setting/makerchecker.service';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { MatDialog } from '@angular/material/dialog';
import { MakerCheckerDetailsDialogComponent } from './maker-checker-details-dialog/maker-checker-details-dialog.component';
import { PermissionsService } from '../../../core/services/setting/permission.service';

@Component({
  selector: 'app-maker-checker',
  standalone: false,
  templateUrl: './maker-checker.component.html',
  styleUrl: './maker-checker.component.scss'
})
export class MakerCheckerComponent implements OnInit, AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  private service = inject(MakerCheckerService);
  makerCheckers: Signal<MakerChecker[]> = this.service.makerCheckerList;
  statusList: Signal<{ id: number; name: string }[]> = this.service.statusList;
  displayedColumns: string[] = ['makerCheckerId', 'tableName', 'actionName', 'status', 'actions'];
  dataSource = new MatTableDataSource<MakerChecker>([]); // Initialize empty data source
  isLoading: boolean = false;
  message: string = "Loading data, please wait..."
  isUploading: boolean = false;
  isDownloading: boolean = false;

  constructor(private dialog: MatDialog, private Makerservice: MakerCheckerService, private PermissionsService: PermissionsService) {
  }

  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }

  ngAfterViewInit(): void {
    if (this.paginator) {
      this.dataSource.paginator = this.paginator;
    }
    if (this.sort) {
      this.dataSource.sort = this.sort;
    }
  }

  ngOnInit(): void {
    this.service.getMakerCheckerStatuses();
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.Makerservice.getAll().subscribe(response => {
      if (response && Array.isArray(response.data)) {
        // Extract `data` array and filter out "Approved" records
        console.log(response.data);
        const filteredData = response.data.filter((item: MakerChecker) => item.statusName !== 'Approved' && item.statusName !== 'Declined').sort((a: any, b: any) =>
          new Date(b.lastModifiedDateTime).getTime() - new Date(a.lastModifiedDateTime).getTime()
        );

        // const filteredData = response.data.sort((a: any, b: any) =>
        //   new Date(b.lastModifiedDateTime).getTime() - new Date(a.lastModifiedDateTime).getTime()
        // );

        // Assign filtered data correctly to MatTableDataSource
        this.dataSource.data = filteredData;

        // Re-attach paginator and sorting
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.isLoading = false;
      } else {
        console.error("Invalid response format:", response);
        this.isLoading = false;
      }
    });
  }


  getStatusName(statusId: number): string {
    return this.service.getStatusName(statusId);
  }

  updateStatus(makerCheckerId: number, statusName: string, comment: string): void {
    this.service.updateStatus(makerCheckerId, statusName, comment).subscribe(() => {
      this.service.getAll(); // Refresh data after update
      this.loadData();
    });
  }

  // this function triggers when we search for an entry thorugh different columns
  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value.trim().toLowerCase();
    this.dataSource.filter = filterValue;
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  openDetailsDialog(row: any): void {
    const dialogRef = this.dialog.open(MakerCheckerDetailsDialogComponent, {
      width: '80vw', // Expands dialog width to 80% of viewport
      maxWidth: '1000px', // Maximum width limit
      data: {
        ...row,
        isMakerChecker: true,
        fromhistory: false
        
      }
    
    });
    dialogRef.afterClosed().subscribe(result => {
      //if (result?.action) {
      //  this.updateStatus(result.id, result.action, result.comment);
      //}
      this.loadData();
    });
  }
}



