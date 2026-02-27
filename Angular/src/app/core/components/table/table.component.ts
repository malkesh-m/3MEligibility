import { Component, EventEmitter, input, Input, Output, SimpleChanges, ViewChild } from '@angular/core';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort, SortDirection } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { MatDialog } from '@angular/material/dialog';
import { DeleteDialogComponent } from '../../../core/components/delete-dialog/delete-dialog.component';
import { PermissionsService } from '../../services/setting/permission.service';
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
  readonly wideColumnHeaders = new Set(['Exp shown', 'Product Card Name', 'Product Name', 'CardName']);
  selectedRows: Set<number> = new Set();
  selectedRowsItem: Set<number> = new Set();
  productselectedRowsItem: Set<number> = new Set();
  paramselectedRowsItem: Set<number> = new Set();
  paginatedRows: any[] = [];
  productparamSelectedItem: { productId: number, parameterId: number }[] = [];
  pageSize: number = 10;
  currentPage: number = 0;
  @Input() displayedColumns: string[] = [];
  @Input() defaultSortColumn: string = '';
  @Input() defaultSortDirection: SortDirection = 'desc';
  @Input() editPermissionId: string = '';
  @Input() deletePermissionId: string = '';
  @Input() validatePermissionId: string = '';
  private defaultSortApplied = false;

  constructor(
    private dialog: MatDialog,
    private PermissionsService: PermissionsService
  ) { }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;

    this.dataSource.sortingDataAccessor = (item, property) => {
      const value = item[property];

      if (this.isDateColumn(property) || this.isDateValue(value)) {
        return this.parseDateValue(value);
      }

      return value;
    };
    this.applyDefaultSort();
  }

  ngOnInit() {
    this.headers = this.header;
    this.displayedColumns = this.headers;
    this.selectedRows.clear();
    this.selectedRowsItem.clear();
    this.updatePaginatedRows();
  }

  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }

  get finalColumns(): string[] {
    return ['Select', ...this.displayedColumns, 'Actions'];
  }

  getColumnMinWidth(header: string): number | null {
    if (!this.wideColumnHeaders.has(header)) return null;
    if (header === 'Exp shown') return 140;
    return 140;
  }

  getColumnMaxWidth(header: string): number | null {
    if (!this.wideColumnHeaders.has(header)) return null;
    if (header === 'Exp shown') return 210;
    return 230;
  }

  shouldPreventHeaderWrap(header: string): boolean {
    return this.wideColumnHeaders.has(header);
  }

  private readonly dateColumnPattern = /date|datetime/i;

  private isDateColumn(column: string): boolean {
    return this.dateColumnPattern.test(column);
  }

  private isDateValue(value: any): boolean {
    if (value instanceof Date) {
      return true;
    }
    if (typeof value !== 'string') {
      return false;
    }
    const normalized = value.trim();
    return /\d{4}-\d{2}-\d{2}/.test(normalized) || /\d{1,2}\/\d{1,2}\/\d{2,4}/.test(normalized);
  }

  private parseDateValue(value: any): Date | null {
    if (value instanceof Date) {
      return value;
    }
    if (value == null) {
      return null;
    }

    const asString = typeof value === 'string' ? value : String(value);
    const normalized = asString
      .replace(/,\s*/g, ' ')
      .replace(/(\d{1,2})\/(\d{1,2})\/(\d{2})(?!\d)/, (_, month, day, year) => {
        const yearNum = Number(year);
        const expandedYear = yearNum < 100 ? 2000 + yearNum : yearNum;
        return `${month}/${day}/${expandedYear}`;
      });

    const parsed = new Date(normalized);
    return Number.isNaN(parsed.getTime()) ? null : parsed;
  }

  private applyDefaultSort() {
    if (!this.sort || !this.displayedColumns?.length) {
      this.defaultSortApplied = false;
      return;
    }

    const preferredDateColumn = this.displayedColumns.find(column =>
      column.toLowerCase().includes('date') && column !== 'Select' && column !== 'Actions'
    );

    const explicitColumn = this.defaultSortColumn?.trim();
    const column = explicitColumn && !['Select', 'Actions'].includes(explicitColumn)
      ? explicitColumn
      : preferredDateColumn ?? this.displayedColumns.find(column => column !== 'Select' && column !== 'Actions') ?? '';

    if (!column) {
      this.defaultSortApplied = false;
      return;
    }

    const direction = this.defaultSortDirection || 'desc';

    if (this.defaultSortApplied && this.sort.active === column && this.sort.direction === direction) {
      return;
    }

    queueMicrotask(() => {
      this.sort.active = column;
      this.sort.direction = direction;
      this.sort.sortChange.emit({ active: column, direction });
      this.defaultSortApplied = true;
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    this.dataSource.paginator = this.paginator;
    this.dataSource.data = this.rows;
    this.dataSource.sort = this.sort;
    this.selectedRows.clear(); // Deselect all rows on the current page
    this.selectedRowsItem.clear();
    if (changes['header'] && this.header?.length) {
      this.headers = this.header;
      this.displayedColumns = [...this.headers];
    }
    this.applyDefaultSort();
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
      this.productparamSelectedItem.push({ productId, parameterId });
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




