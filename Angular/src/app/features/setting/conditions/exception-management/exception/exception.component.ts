import { Component, EventEmitter, inject, Input, Output, SimpleChanges, ViewChild} from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { ExceptionManagementService } from '../../../../../core/services/setting/exception-management.service';
import { FactorsService } from '../../../../../core/services/setting/factors.service';
import { NgForm } from '@angular/forms';
import { UtilityService } from '../../../../../core/services/utility/utils';

export interface ExceptionRecord {
  exceptionManagementId: number;
  exceptionName: string;
  isTemporary: boolean; // Updated to match API
  startDate: string | null;
  endDate: string | null;
  fixedPercentage: string;
  variationPercentage: string;
  scope: string | string[]; // Allow both string and array
  description: string;
  isActive: boolean; // Updated to match API
  createdByDateTime: string;
  updatedByDateTime: string;
  expShown: string |null; // Required field based on API
  expression: string | null;
}

@Component({
  selector: 'app-exception',
  standalone: false,

  templateUrl: './exception.component.html',
  styleUrl: './exception.component.scss'
})

export class ExceptionComponent {
  displayedColumns: string[] = ['ExceptionName', 'TemporaryException', 'Scope', 'Description', 'Status', 'CreatedDate', 'UpdatedDate','FixedPercentage','VariationPercentage','ExpShown', 'Actions'];
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @Input({required: true}) tableData: any;
  @Input({required: true}) searchTerm: string = '';
  @Output() editForm = new EventEmitter<any>();
  @Output() deleteException = new EventEmitter<any>();
  private _snackBar = inject(MatSnackBar);
  dataSource = new MatTableDataSource<any>([]); // Initialize empty data source
  exceptionScopes: string[] = ['Limit Amount', 'Product Eligibility'];
  limitAmountEnabled: boolean = false;
  productEligibilityEnabled: boolean = false;
  formVisible = false;
  menuVisible = false;
  isEditMode = false;

  constructor(private utilityService: UtilityService) {
    
  }

  ngOnChanges(changes: SimpleChanges): void {
    console.log(changes)
    if (changes['tableData']) {
      this.dataSource.data = this.tableData;
      //console.log(this.dataSource.data);
      this.dataSource.paginator = this.paginator
      this.dataSource.sort = this.sort
    }

    if(changes['searchTerm']) {
      //console.log("search term changed to: ", changes['searchTerm'])
      this.dataSource.filter = this.searchTerm;
      if (this.dataSource.paginator) {
        this.dataSource.paginator.firstPage();
      }
    }
  }

  toggleColumn(column1: string, column2: string) {
    const index1 = this.displayedColumns.indexOf(column1);
    const index2 = this.displayedColumns.indexOf(column2);
    const temporaryExceptionIndex = this.displayedColumns.indexOf('TemporaryException');
    
    if (index1 > -1 && index2 > -1) {
      // Remove both columns if they are currently visible
      this.displayedColumns.splice(index1, 1);
      this.displayedColumns.splice(index1, 1);
    } else {
      // Add columns after TemporaryException if they are not visible
      if (index1 === -1) {
        this.displayedColumns.splice(temporaryExceptionIndex + 1, 0, column1);
      }
      if (index2 === -1) {
        this.displayedColumns.splice(temporaryExceptionIndex + 2, 0, column2);
      }
    }
  
    this.displayedColumns = [...this.displayedColumns]; // Ensure reactivity
  }
  
  sanitizeCode(event: any) {
    event.target.value = this.utilityService.sanitizeCode(event.target.value);
  }

   // edit the form when edit icon is clicked
  editRecord(record: ExceptionRecord) {
   
      this.isEditMode = true;
    this.formVisible = true;

      this.editForm.emit(record)
 
      //this.formData = { ...record };
    }

  async deleteRecord(record: any) {
    //console.log(record)
        const confirmDelete = window.confirm(
          `Are you sure you want to delete the Exception: "${record.ExceptionName}"?`
        );
      if (confirmDelete) {
       
        this.deleteException.emit(record.exceptionManagementId);
          // Call Delete API
      }
    }
    

}
