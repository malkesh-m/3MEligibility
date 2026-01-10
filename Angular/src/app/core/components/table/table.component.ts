import { Component, EventEmitter, input, Input, Output, SimpleChanges, ViewChild } from '@angular/core';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { MatDialog } from '@angular/material/dialog';
import { DeleteDialogComponent } from '../../../core/components/delete-dialog/delete-dialog.component';
import { RolesService } from '../../services/setting/role.service';
@Component({
  selector: 'app-table',
  standalone: false,
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.scss']
})
export class TableComponent {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @Input() rows: any[] = [];
  @Input() cols: any[] = [];
  @Input() header: any[] = [];
  @Input() checkedSelectedId: string = '';
  @Input() hasValidationAction: boolean = false;
  @Output() actionEvent = new EventEmitter<{ action: string; data: any }>();
  dataSource = new MatTableDataSource<any>();
  headers: string[] = [];
  selectedRows: Set<number> = new Set();
  selectedRowsItem: Set<number> = new Set();
  productselectedRowsItem: Set<number> = new Set();
  paramselectedRowsItem: Set<number> = new Set();
  paginatedRows: any[] = [];
  productparamSelectedItem: { productId: number, parameterId: number }[] = [];
  pageSize: number = 10;
  currentPage: number = 0;
  @Input() displayedColumns: string[] = [];
  @Input() editRoleId: number = 0;
  @Input() deleteRoleId: number = 0;
  @Input() validateRoleId: number = 0;

  constructor(
    private dialog: MatDialog,
    private rolesService:RolesService
  ) { }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;

    this.dataSource.sortingDataAccessor = (item, property) => {
      // Dates
      if (property === 'Created Date' || property === 'Updated Date') {
        return item[property] ? new Date(item[property]) : null;
      }
      return item[property];
    };
  }

  ngOnInit() {
    this.headers = this.header;
    this.displayedColumns = this.headers;
    this.selectedRows.clear();
    this.selectedRowsItem.clear();
    this.updatePaginatedRows();
  }

  hasPermission(roleId: number): boolean {
    return this.rolesService.hasPermission(roleId);
  }
  
  get finalColumns(): string[] {
    return ['Select', ...this.displayedColumns, 'Actions'];
  }
  
  toggleColumn(column: string) {
    const index = this.displayedColumns.indexOf(column);

    if (index > -1) {
      // Remove column if it's already visible
      this.displayedColumns.splice(index, 1);
    } else {
      // Ensure it appears after its respective main column
      const referenceColumn = column === 'Created Date' ? 'Created By' : 'Updated By';
      const referenceIndex = this.displayedColumns.indexOf(referenceColumn);

      if (referenceIndex !== -1) {
        this.displayedColumns.splice(referenceIndex + 1, 0, column);
      } else {
        this.displayedColumns.push(column);
      }
    }

    this.displayedColumns = [...this.displayedColumns]; // Ensure reactivity
  }

  ngOnChanges(changes: SimpleChanges) {    
    this.dataSource.paginator = this.paginator;
    this.dataSource.data = this.rows;
    this.dataSource.sort = this.sort;
    this.selectedRows.clear(); // Deselect all rows on the current page
    this.selectedRowsItem.clear();
  }

  applyFilter(filter: string) {
    this.dataSource.filter = filter;
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
  
    }
  }

  // toggleSelection(rowIndex: number, rowItem: any) {
  //   if (this.selectedRows.has(rowIndex)) {
  //     this.selectedRows.delete(rowIndex);
  //     this.selectedRowsItem.delete(rowItem[this.checkedSelectedId])
  //   } else {
  //     this.selectedRows.add(rowIndex);
  //     this.selectedRowsItem.add(rowItem[this.checkedSelectedId])
  //   }
  // }
  

  toggleSelection(rowIndex: number, rowItem: any) {
    const productId = Number(rowItem.ProductId);  // Ensure it's a number
    const parameterId = Number(rowItem.ParameterId);  // Ensure it's a number
    const existingRecordIndex = this.productparamSelectedItem.findIndex(item => 
      item.productId === productId && item.parameterId === parameterId
    );
    if (this.selectedRows.has(rowIndex)) {
      this.selectedRows.delete(rowIndex);
      this.selectedRowsItem.delete(rowItem[this.checkedSelectedId])
      if (existingRecordIndex !== -1) {
        this.productparamSelectedItem.splice(existingRecordIndex, 1);
      }
    } else {
      this.selectedRows.add(rowIndex);
      this.selectedRowsItem.add(rowItem[this.checkedSelectedId])
      this.productparamSelectedItem.push({productId,parameterId});
    }
  }

  toggleSelectAll(event: MatCheckboxChange) {
    const checked = event.checked;
    this.updatePaginatedRows(); // Make sure paginated rows are updated
  
    if (checked) {
      this.selectedRows.clear(); // Clear any previous selections
      this.selectedRowsItem.clear();
      this.paginatedRows.forEach((row, index) => {
        this.selectedRows.add(index);
        this.selectedRowsItem.add(row[this.checkedSelectedId]);
      });
    } else {
      this.selectedRows.clear(); // Deselect all rows on the current page
      this.selectedRowsItem.clear();
    }
  }

  
  onPageChange(event: PageEvent) {
    this.pageSize = event.pageSize;
    this.currentPage = event.pageIndex;
    this.updatePaginatedRows();
  }

  deleterow(row: any): void {
    const rowIndex = this.rows.indexOf(row); // Get index before deletion
  
    // Remove the row from the table
    this.rows = this.rows.filter(r => r !== row);
  
    // Remove the row from selection
    this.selectedRows.delete(rowIndex);
    this.selectedRowsItem.delete(row[this.checkedSelectedId]);
  
    // Adjust selectedRows indices to match the new row order
    const updatedSelectedRows = new Set<number>();
    this.selectedRows.forEach(selectedIndex => {
      if (selectedIndex > rowIndex) {
        updatedSelectedRows.add(selectedIndex - 1); // Shift indices down
      } else if (selectedIndex < rowIndex) {
        updatedSelectedRows.add(selectedIndex); // Keep unaffected selections
      }
    });
  
    this.selectedRows = updatedSelectedRows; // Apply updated selection
  
    // Emit delete action
    this.actionEvent.emit({ action: 'delete', data: row });
  
    // Ensure selection state is updated
    this.updatePaginatedRows();
    this.selectedRows.clear();
    this.selectedRowsItem.clear();
  }
  
  

  updatePaginatedRows() {
    const startIndex = this.currentPage * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.paginatedRows = this.rows.slice(startIndex, endIndex);
    this.selectedRows.clear();
    this.selectedRowsItem.clear();
  }

}
