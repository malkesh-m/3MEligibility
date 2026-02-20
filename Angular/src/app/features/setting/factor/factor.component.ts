import { Component, OnInit, HostListener, ViewChild, AfterViewInit, inject, Injectable, QueryList, ViewChildren } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { MatSnackBar } from "@angular/material/snack-bar";
import { DeleteDialogComponent } from '../../../core/components/delete-dialog/delete-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { FactorsService } from '../../../core/services/setting/factors.service';
import { ParameterService } from '../../../core/services/setting/parameter.service';
import { UtilityService } from '../../../core/services/utility/utils';
import { NgForm, NgModel } from '@angular/forms';
import { AuthService } from '../../../core/services/auth/auth.service';
import { PermissionsService } from '../../../core/services/setting/permission.service';
import { TranslateService } from '@ngx-translate/core';

export interface FactorRecord {
  factorName: string;
  note: string;
  parameterId: number | null;
  conditionId: number | null;
  value1: string | boolean | Date | number;
  value2: string;
  selected: boolean;
  factorId?: number | null;
  createdBy: string;
  updatedBy: string;
  input: string;
}

export interface Conditions {
  conditionId: number;
  conditionValue: string;
}

export interface Parameters {
  parameterId: number;
  parameterName: string;
  hasFactors: boolean;
  identifier: number;
  isKyc: boolean;
  isRequired: boolean;
  factorId: number;
  dataTypeId: number;
  conditionId: number;
  factorOrder: string;
}

@Component({
  selector: 'app-factor',
  standalone: false,
  templateUrl: './factor.component.html',
  styleUrl: './factor.component.scss'
})

@Injectable({
  providedIn: 'root',
})

export class FactorsComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = ['select', 'parameterName', 'factorName', 'value1', 'value2', 'createdBy', 'updatedBy', 'actions'];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  private _snackBar = inject(MatSnackBar);
  records: FactorRecord[] = [];

  dataSource = new MatTableDataSource<FactorRecord>(this.records); // Initialize empty data source
  isCalculatedParameter: boolean = false;
  formVisible = false;
  menuVisible = false;
  isEditMode = false;

  formData: FactorRecord = {
    factorName: '',
    note: '',
    parameterId: null,
    conditionId: null,
    value1: '',
    value2: '',
    selected: false,
    factorId: null,
    createdBy: '',
    updatedBy: '',
    input: ''
  };

  searchTerm: string = '';
  parametersList: Parameters[] = [];
  conditionsList: Conditions[] = [];
  AllList: any[] = [];
  isVal1Dropdown: boolean = false;
  isVal2Dropdown: boolean = false;
  selectedRows: Set<number> = new Set();
  selectedRowsItem: Set<number> = new Set();
  pageSize: number = 10;
  dataTypes: { dataTypeId: number; dataTypeName: string }[] = [];
  selectedDataType: string = 'Text';
  selectedFile: File | null = null;
  isDownloading: boolean = false;
  isLoading: boolean = false; // Show loader on page load
  isUploading: boolean = false;
  message: string = this.translate.instant("Loading data, please wait...");
  loggedInUser: any = null;
  SelectMultiple: boolean = false;

  constructor(
    private factorsService: FactorsService,
    private dialog: MatDialog,
    private PermissionsService: PermissionsService,
    private parameterService: ParameterService,
    private utilityService: UtilityService,
    private authService: AuthService,
    private translate: TranslateService
  ) { }

  async ngOnInit() {
    await Promise.all([this.fetchParametersList(), this.fetchConditonsList()]);
    this.fetchFactorsList(); // Fetch entities options
    this.fetchDataTypes();

  }

  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }
  showMaxLengthWarning: boolean = false;
  onEntityNameInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;

    if (value.length > 50) {
      this.showMaxLengthWarning = true;
      this.formData.factorName = value.slice(0, 50); // truncate
    } else {
      this.showMaxLengthWarning = false;
      this.formData.factorName = value;
    }
  }
  toggleColumn(column: string, afterColumn: string) {
    const index = this.displayedColumns.indexOf(column);

    if (index > -1) {
      // Remove column if already visible
      this.displayedColumns.splice(index, 1);
    } else {
      // Find the index of the afterColumn and insert right after it
      const afterIndex = this.displayedColumns.indexOf(afterColumn);
      if (afterIndex !== -1) {
        this.displayedColumns.splice(afterIndex + 1, 0, column);
      } else {
        this.displayedColumns.push(column); // Default push if not found
      }
    }

    this.displayedColumns = [...this.displayedColumns]; // Ensure reactivity
  }

  ngAfterViewInit() {
    // Bind paginator and sort after the view is initialized
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  toggleMenu() {
    this.menuVisible = !this.menuVisible;
  }

  closeMenu() {
    this.menuVisible = false;
  }

  fetchFactorsList() {
    this.isLoading = true;
    this.factorsService.getFactorsList().subscribe({
      next: (response) => {
        if (response && response.data) {
          // Sorting the response data based on lastModifiedDateTime in descending order
          const sortedData = response.data.sort((a: any, b: any) =>
            new Date(b.lastModifiedDateTime).getTime() - new Date(a.lastModifiedDateTime).getTime()
          );
          this.dataSource.data = sortedData.map((factor: any) => {
            const parameter = this.parametersList.find(
              (param) => param.parameterId === factor.parameterId
            );
            this.isLoading = false;
            return {
              ...factor,
              parameterName: parameter ? parameter.parameterName : this.translate.instant('Unknown Parameter'),
              selected: false,
            };


          });
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error fetching entities:', error);
        this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        this.isLoading = false;
      },
    });
  }

  fetchConditonsList(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.factorsService.getConditionsList().subscribe({
        next: (response) => {
          if (response) {
            this.conditionsList = response;
          } else {
            console.error('Invalid parameters response:', response);
          }
          resolve();
        },
        error: (err) => reject(err),
      });
    });
  }

  fetchParametersList(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.factorsService.getParametersList().subscribe({
        next: (response) => {
          if (response && response.data) {
            this.parametersList = response.data;
            this.fetchFactorsList();
          } else {
            console.error('Invalid parameters response:', response);
          }
          resolve();
        },
        error: (err) => reject(err),
      });
    });
  }

  // Get current page data
  getCurrentPageData(): any[] {
    if (!this.paginator) return []; // Return empty array if paginator is undefined
    const startIndex = this.paginator.pageIndex * this.paginator.pageSize;
    const endIndex = startIndex + this.paginator.pageSize;
    return this.dataSource.filteredData.slice(startIndex, endIndex);
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value.trim().toLowerCase();
    this.dataSource.filter = filterValue;
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  //addRecord(form: NgForm): void {

  //      this.authService.currentUser$.subscribe((user) => {
  //          this.loggedInUser = user;
  //      });
  //      if (form.invalid) {
  //          // Mark all controls as touched to display validation messages
  //          Object.keys(form.controls).forEach((controlName) => {
  //              form.controls[controlName].markAsTouched();
  //          });

  //          this._snackBar.open('Please fill in the required fields.', 'Close', {
  //              horizontalPosition: 'right',
  //              verticalPosition: 'top', duration: 3000
  //              //duration: 5000, // Auto-close after 5 seconds
  //          });

  //          return; // Prevent form submission
  //      }
  //      const requestBody = {
  //          ...this.formData,
  //          parameterId: this.formData.parameterId ? Number(this.formData.parameterId) : null,
  //          conditionId: this.formData.conditionId ? Number(this.formData.conditionId) : null,
  //          factorId: this.isEditMode ? Number(this.formData.factorId) : 0, // Assign 0 for new records,
  //          createdBy:'',
  //          updatedBy:'',
  //          note: 'string'
  //      };
  //      if (this.isEditMode) {
  //          // Call update API
  //          requestBody.createdBy = this.formData.createdBy;
  //          requestBody.updatedBy = this.loggedInUser.user.userName;
  //          this.factorsService.updateFactor(requestBody).subscribe({
  //              next: (response) => {
  //                  if (response.isSuccess) {
  //                      console.log('Factor updated successfully:', response.message);
  //                      this.fetchFactorsList(); // Refresh the table
  //                      this.closeForm(); // Close the form
  //                      this._snackBar.open(response.message, 'Okay', {
  //                          horizontalPosition: 'right',
  //                          verticalPosition: 'top', duration: 3000
  //                      });
  //                  } else {
  //                      console.error('Error updating factor:', response.message);
  //                      this._snackBar.open(response.message, 'Okay', {
  //                          horizontalPosition: 'right',
  //                          verticalPosition: 'top', duration: 3000
  //                      });
  //                  }
  //              },
  //              error: (error: any) => {
  //                  console.error('API Error:', error);
  //                  this._snackBar.open(error.message, 'Okay', {
  //                      horizontalPosition: 'right',
  //                      verticalPosition: 'top', duration: 3000
  //                  });
  //              },
  //          });
  //      } else {
  //          // Add new record
  //          // const requestBody = {
  //          //     ...this.formData,
  //          //     factorId: 0, // Assign a numeric value, e.g., 0 for no parent
  //          // };
  //          requestBody.createdBy = this.loggedInUser.user.userName;
  //          requestBody.updatedBy = this.loggedInUser.user.userName;
  //          this.factorsService.saveFactor(requestBody).subscribe({
  //              next: (response) => {
  //                  if (response.isSuccess) {
  //                      console.log('Factor added successfully:', response.message);
  //                      this.fetchFactorsList(); // Refresh the table after adding
  //                      this.closeForm(); // Close the form
  //                      this._snackBar.open(response.message, 'Okay', {
  //                          horizontalPosition: 'right',
  //                          verticalPosition: 'top', duration: 3000
  //                      });
  //                  } else {
  //                      console.error('Error adding factor:', response.message);
  //                      this._snackBar.open(response.message, 'Okay', {
  //                          horizontalPosition: 'right',
  //                          verticalPosition: 'top', duration: 3000
  //                      });
  //                  }
  //              },
  //              error: (error) => {
  //                  console.error('API Error:', error);
  //                  this._snackBar.open(error.message, 'Okay', {
  //                      horizontalPosition: 'right',
  //                      verticalPosition: 'top', duration: 3000
  //                  });
  //              },
  //          });
  //      }
  //  }

  //new Add Record Logic
  addRecord(form: NgForm): void {

    this.authService.currentUser$.subscribe((user) => {
      this.loggedInUser = user;
    });
    if (form.invalid) {
      // Mark all controls as touched to display validation messages
      Object.keys(form.controls).forEach((controlName) => {
        form.controls[controlName].markAsTouched();
      });

      this._snackBar.open(this.translate.instant('Please fill in the required fields.'), this.translate.instant('Close'), {
        horizontalPosition: 'right',
        verticalPosition: 'top', duration: 3000
        //duration: 5000, // Auto-close after 5 seconds
      });

      return; // Prevent form submission
    }
    if (typeof this.formData.value1 === 'boolean') {
      this.formData.value1 = this.formData.value1 ? 'True' : 'False';
    }
    const requestBody = {
      ...this.formData,
      parameterId: this.formData.parameterId ? Number(this.formData.parameterId) : null,
      conditionId: this.formData.conditionId ? Number(this.formData.conditionId) : null,
      factorId: this.isEditMode ? Number(this.formData.factorId) : 0, // Assign 0 for new records,
      createdBy: '',
      updatedBy: '',
      note: 'string'
    };
    if (this.isEditMode) {
      // Call update API
      // requestBody.createdBy = this.formData.createdBy;
      // requestBody.updatedBy = this.loggedInUser.user.userName;
      this.factorsService.updateFactor(requestBody).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.fetchFactorsList(); // Refresh the table
            this.closeForm(); // Close the form
            this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          } else {
            console.error('Error updating factor:', response.message);
            this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        },
        error: (error: any) => {
          console.error('API Error:', error);
          this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
        },
      });
    } else {
      // Add new record
      // const requestBody = {
      //     ...this.formData,
      //     factorId: 0, // Assign a numeric value, e.g., 0 for no parent
      // };
      const rawValue = String(this.formData.value1 || '');

      if (this.SelectMultiple && rawValue.includes(',')) {
        const values = rawValue
          .split(',')
          .map(val => val.trim())
          .filter(val => val); // Removes empty strings

        let successCount = 0;

        values.forEach((value, index) => {
          const individualRecord = {
            ...requestBody,
            value1: value,
            factorName: `${requestBody.factorName}`
          };

          this.factorsService.saveFactor(individualRecord).subscribe({
            next: (response) => {
              if (response.isSuccess) {
                successCount++;
                if (successCount === values.length) {
                  this.fetchFactorsList();
                  this.closeForm();
                  this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
                    horizontalPosition: 'right',
                    verticalPosition: 'top',
                    duration: 3000
                  });
                }
              } else {
                this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
                  horizontalPosition: 'right',
                  verticalPosition: 'top',
                  duration: 3000
                });
              }
            },
            error: (error) => {
              this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
                horizontalPosition: 'right',
                verticalPosition: 'top',
                duration: 3000
              });
            }
          });
        });
      } else {
        // Save as single record
        this.factorsService.saveFactor(requestBody).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.fetchFactorsList();
              this.closeForm();
              this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
                horizontalPosition: 'right',
                verticalPosition: 'top',
                duration: 3000
              });
            } else {
              this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
                horizontalPosition: 'right',
                verticalPosition: 'top',
                duration: 3000
              });
            }
          },
          error: (error) => {
            this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top',
              duration: 3000
            });
          }
        });
      }
    }
  }

  trackByUserId(index: number, group: any): number {
    return group.listId;
  }

  sanitizeInput(event: any) {
    event.target.value = this.utilityService.sanitizeCode(event.target.value);
  }

  sanitizeCode(event: any) {
    const allowComma = this.SelectMultiple;
    const inputName = event.target.name;

    if (this.selectedDataType === 'Numeric') {
      let inputValue = event.target.value.toUpperCase();

      // Check if the value is "ALL" - if so, allow it
      if (inputValue === 'ALL' || inputValue === 'all' || inputValue === 'A' || inputValue === 'L' || inputValue === 'l' || inputValue === 'a') {
        event.target.value = 'ALL';
      } else {
        // CORRECTED: Use proper regex to remove non-digit and non-comma characters
        let regex = allowComma ? /[^\d,]/g : /[^\d]/g;
        inputValue = inputValue.replace(regex, '');

        // Additional cleanup for comma-separated values
        if (allowComma) {
          inputValue = inputValue
            .replace(/^,+/g, '') // Remove leading commas
            .replace(/,+$/g, '') // Remove trailing commas  
            .replace(/,+/g, ','); // Replace multiple commas with single comma
        }

        event.target.value = inputValue;
      }

      // Update form data (MOVED OUTSIDE if-else to cover both cases)
      if (inputName === 'value1') {
        this.formData.value1 = event.target.value;
      } else if (inputName === 'value2') {
        this.formData.value2 = event.target.value;
      }
    }
    else if (this.selectedDataType === 'Text' || this.selectedDataType === 'Alphanumeric') {
      // For text fields, use your existing logic
      let regex = allowComma ? /[^\u0600-\u06FFa-zA-Z0-9\s,]/g : /[^\u0600-\u06FFa-zA-Z0-9\s]/g;
      event.target.value = event.target.value.replace(regex, '');

      // Update the correct form data property based on input name
      if (inputName === 'value1') {
        this.formData.value1 = event.target.value;
      } else if (inputName === 'value2') {
        this.formData.value2 = event.target.value;
      }
    }
    // For Boolean and Date types, no sanitization needed
  }

  // Toggle Single Row Selection
  toggleSelection(event: MatCheckboxChange, factorId: number) {
    if (event.checked) {
      this.selectedRows.add(factorId);
    } else {
      this.selectedRows.delete(factorId);
    }
  }

  // Toggle Select All Rows on Current Page
  toggleSelectAll(event: MatCheckboxChange) {
    const currentPageData = this.getCurrentPageData(); // Paginated rows
    if (event.checked) {
      currentPageData.forEach((row: any) => this.selectedRows.add(row.factorId));
    } else {
      currentPageData.forEach((row: any) => this.selectedRows.delete(row.factorId));
    }
  }

  // Check if All Rows on Current Page are Selected
  isAllPageSelected(): boolean {
    const currentPageData = this.getCurrentPageData();
    return currentPageData.every((row: any) => this.selectedRows.has(row.factorId));
  }

  // Check if Some Rows are Selected (Indeterminate State)
  isSomePageSelected(): boolean {
    const currentPageData = this.getCurrentPageData();
    return currentPageData.some((row: any) => this.selectedRows.has(row.factorId)) && !this.isAllPageSelected();
  }

  deleteFactor(row: any): void {
    const dialogRef = this.dialog.open(DeleteDialogComponent, {
      data: {
        title: this.translate.instant('Confirm'),
        message: this.translate.instant('Are you sure you want to delete the factor: "{{factor}}"?', { factor: row.factorName })
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result?.delete) {
        this.performDelete(row);
      }
    })
  }

  performDelete(row: any): void {
    this.factorsService.deleteFactor(row.factorId).subscribe({
      next: (response) => {
        console.log('Factor deleted:', response);
        this.fetchFactorsList();
        this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        this.selectedRows.clear();
      },
      error: (error) => {
        console.error('Error deleting factor:', error);
        this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        this.selectedRows.clear();
      },
    });
  }

  // Delete Selected Rows
  deleteMulSelectedRows() {
    if (this.selectedRows.size === 0) {
      this._snackBar.open(this.translate.instant('Please select at least one row to delete'), this.translate.instant('Okay'), {
        horizontalPosition: 'right',
        verticalPosition: 'top', duration: 3000
      });
      return;
    }

    const dialogRef = this.dialog.open(DeleteDialogComponent, {
      data: {
        title: this.translate.instant('Confirm'),
        message: this.translate.instant('Are you sure you want to delete the selected records?')
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result?.delete) {
        const factorIdsToDelete = Array.from(this.selectedRows);

        this.factorsService.deleteMultipleFactors(factorIdsToDelete).subscribe({
          next: (response) => {
            console.log('Entities deleted successfully:', response.message);
            this.selectedRows.clear(); // Clear selected rows
            this.fetchFactorsList(); // Refresh table data
            this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          },
          error: (error) => {
            this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          },
        });
      }
    });
  }

  refreshTable() {
    // Update data source to reflect changes
    this.dataSource.data = [...this.dataSource.data];
  }

  openForm() {
    this.SelectMultiple = false;
    this.showMaxLengthWarning = false;
    this.isVal1Dropdown = false;
    this.isVal2Dropdown = false;
    this.selectedDataType = 'Text';
    this.formVisible = true;
    this.isEditMode = false;
    this.formData = {
      factorName: '',
      note: '',
      parameterId: null,
      conditionId: null,
      value1: '',
      value2: '',
      selected: false,
      factorId: null,
      createdBy: '',
      updatedBy: '',
      input: ''

    }
  }

  closeForm() {
    this.formVisible = false;
    this.isEditMode = false;
    this.formData = {
      factorName: '',
      note: '',
      parameterId: null,
      conditionId: null,
      value1: '',
      value2: '',
      selected: false,
      factorId: null,
      createdBy: '',
      updatedBy: '',
      input: ''

    }
  }

  editRecord(record: FactorRecord) {
    this.showMaxLengthWarning = false;
    this.SelectMultiple = false;
    this.isEditMode = true;
    this.formVisible = true;
    this.formData = { ...record };

    const dropdownConditions = this.conditionsList.find(condition => condition.conditionId === record.conditionId);
    if (dropdownConditions) {
      if (dropdownConditions.conditionValue === 'In List' || dropdownConditions.conditionValue === 'Not In List') {
        this.isVal1Dropdown = true;
        this.isVal2Dropdown = false;
        this.fetchAllLists();
      } else if (dropdownConditions.conditionValue === 'Range') {
        this.isVal1Dropdown = false;
        this.isVal2Dropdown = true;
      } else {
        this.isVal1Dropdown = false;
        this.isVal2Dropdown = false;
      }
    }

    const selectedParameter = this.parametersList.find(param => param.parameterId === record.parameterId);
    if (selectedParameter) {
      const dataType = this.dataTypes.find(dt => dt.dataTypeId === selectedParameter.dataTypeId);
      if (dataType) {
        this.selectedDataType = dataType.dataTypeName;
      }
    }
    if (this.selectedDataType?.toLowerCase() === 'boolean') {
      if (typeof this.formData.value1 === 'string') {
        this.formData.value1 = this.formData.value1.toLowerCase() === 'true';
      }
    }
  }
  isRange: boolean = false;   // Add this property at top of component

  onConditionChange(event: any): void {
    const conditionId = Number(event.target.value);
    const selectedCondition = this.conditionsList.find(c => c.conditionId === conditionId);

    this.formData.value1 = '';
    this.formData.value2 = '';
    this.isVal1Dropdown = false;
    this.isVal2Dropdown = false;
    this.isRange = false;  // reset

    if (selectedCondition) {
      const conditionValue = selectedCondition.conditionValue;

      if (conditionValue === 'In List' || conditionValue === 'Not In List') {
        this.isVal1Dropdown = true;
        this.isVal2Dropdown = false;
        this.fetchAllLists();
      }
      else if (conditionValue === 'Range') {
        this.isRange = true;
        this.isVal2Dropdown = true;
      }
      else {
        this.isVal1Dropdown = false;
        this.isVal2Dropdown = false;
      }
    }
  }


  onParameterChange(event: any): void {
    const parameterId = Number(event.target.value);
    const selectedParameter = this.parametersList.find(param => param.parameterId === parameterId);

    this.formData.value1 = '';
    this.formData.value2 = '';
    this.isVal1Dropdown = false;
    this.isVal2Dropdown = false;

    if (selectedParameter) {
      const dataType = this.dataTypes.find(dt => dt.dataTypeId === selectedParameter.dataTypeId);
      if (dataType) {
        this.selectedDataType = dataType.dataTypeName;
      }
    }
  }

  //validateValue1Input(event: KeyboardEvent | ClipboardEvent): void {
  //    const dataType = this.selectedDataType;
  //    let isValidInput = false;

  //    // Type narrow event types properly
  //    if (event instanceof KeyboardEvent || event instanceof ClipboardEvent) {
  //        const pastedText = event instanceof ClipboardEvent ? event.clipboardData?.getData('text') : undefined;

  //        switch (dataType) {
  //            case 'Numeric':
  //                if (event instanceof KeyboardEvent) {
  //                    const charCode = event.key.charCodeAt(0);
  //                    isValidInput = charCode >= 48 && charCode <= 57; // Check if it's a numeric key
  //                } else if (event instanceof ClipboardEvent) {
  //                    // Ensure pasted text is not undefined
  //                    isValidInput = pastedText ? /^\d+$/.test(pastedText) : false;
  //                }
  //                break;

  //            case 'Text':
  //                if (event instanceof KeyboardEvent) {
  //                    const regexText = /^[a-zA-Z\s]*$/;
  //                    isValidInput = regexText.test(event.key);
  //                } else if (event instanceof ClipboardEvent) {
  //                    isValidInput = pastedText ? /^[a-zA-Z\s]*$/.test(pastedText) : false;
  //                }
  //                break;

  //            case 'Alphanumeric':
  //                if (event instanceof KeyboardEvent) {
  //                    const regexAlphanumeric = /^[a-zA-Z0-9\s]*$/;
  //                    isValidInput = regexAlphanumeric.test(event.key);
  //                } else if (event instanceof ClipboardEvent) {
  //                    isValidInput = pastedText ? /^[a-zA-Z0-9\s]*$/.test(pastedText) : false;
  //                }
  //                break;

  //            case 'Boolean':
  //                // For Boolean type, only allow `true` or `false`
  //                event.preventDefault();
  //                return;

  //            case 'Date':
  //                // No input allowed in Date case (can be modified if date input handling is needed)
  //                event.preventDefault();
  //                return;

  //            default:
  //                break;
  //        }
  //    }

  //    // If the input is invalid, prevent the event
  //    if (!isValidInput) {
  //        event.preventDefault();
  //    }
  //}
  allowOnlyNumeric(event: KeyboardEvent) {
    if (this.selectedDataType !== 'Numeric') return; // Only numeric fields

    const key = event.key;
    const allowedChars = this.SelectMultiple ? '0123456789AlLa,' : '0123456789AlLa';

    if (!allowedChars.includes(key)) {
      event.preventDefault();
    }
  }

  onPasteNumeric(event: ClipboardEvent) {
    if (this.selectedDataType !== 'Numeric') return;

    const pastedText = event.clipboardData?.getData('text') || '';
    const regex = this.SelectMultiple ? /^[AlLa0-9,]*$/ : /^[0-9]*$/;

    if (!regex.test(pastedText)) {
      event.preventDefault();
    }
  }
  validateValue1Input(event: KeyboardEvent | ClipboardEvent): void {
    const dataType = this.selectedDataType;
    const allowComma = this.SelectMultiple;
    let isValidInput = true;

    // Skip validation for Boolean/Date because they are dropdowns or date inputs
    if (dataType === 'Boolean' || dataType === 'Date') return;

    // Handle keyboard typing
    if (event instanceof KeyboardEvent) {
      const key = event.key;

      if (dataType === 'Numeric') {
        const regex = allowComma ? /^[aAlL0-9,]*$/ : /^[AlLa0-9]*$/;
        isValidInput = regex.test(key);
      }
      else if (dataType === 'Text') {
        const regex = allowComma ? /^[a-zA-Z0-9\s,(){}\[\]]$/ : /^[a-zA-Z0-9\s(){}\[\]]$/;
        isValidInput = regex.test(key);
      }
      else if (dataType === 'Alphanumeric') {
        const regex = allowComma ? /^[a-zA-Z0-9\s,(){}\[\]]$/ : /^[a-zA-Z0-9\s(){}\[\]]$/;
        isValidInput = regex.test(key);
      }
    }

    if (!isValidInput) {
      event.preventDefault();
    }
  }

  // Handle paste event to prevent invalid input
  onPasteValue1(event: ClipboardEvent): void {
    // Call validate for paste event
    this.validateValue1Input(event);
  }

  fetchDataTypes(): void {
    this.parameterService.getDataTypes().subscribe({
      next: (data) => {
        this.dataTypes = data;
      },
      error: (error) => {
        this._snackBar.open(error, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      },
    });
  }

  fetchAllLists() {
    this.factorsService.fetchAllLists().subscribe({
      next: (response) => {
        if (response) {
          this.AllList = response.data;
        } else {
          console.error('Invalid parameters response:', response);
        }
      },
      error: (error) => {
        console.error('Error fetching parameters:', error);
      },
    });
  }

  // exportFactors() {
  //     this.factorsService.exportFactors().subscribe({
  //         next: (response: Blob) => {
  //             console.log('Blob resp: ', response);
  //             const url = window.URL.createObjectURL(response);
  //             const anchor = document.createElement('a');
  //             anchor.href = url;
  //             anchor.download = 'Factors.xlsx';
  //             document.body.appendChild(anchor);
  //             anchor.click();
  //             document.body.removeChild(anchor);
  //             window.URL.revokeObjectURL(url);
  //         },
  //         error: (error) => {
  //             console.error('Error deleting selected factors:', error);
  //         }
  //     });
  // }

  exportFactors() {
    const selectedIds = Array.from(this.selectedRows);
    const searchTerm = selectedIds.length === 0 ? (this.searchTerm || '').trim() : '';
    this.factorsService.exportFactors(selectedIds, searchTerm).subscribe({
      next: (response: Blob) => {
        console.log('Blob resp: ', response);
        const url = window.URL.createObjectURL(response);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = 'Factors.xlsx';
        document.body.appendChild(anchor);
        anchor.click();
        document.body.removeChild(anchor);
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Error deleting selected factors:', error);
      }
    });
  }

  downloadTemplate() {
    this.isDownloading = true;
    this.message = "Please wait, template is downloading...";
    this.factorsService.downloadTemplate().subscribe((response) => {
      this.isDownloading = false;
      const blob = new Blob([response], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'Factors-Template.xlsx'; // Filename for the download
      a.click();
      window.URL.revokeObjectURL(url);
      this._snackBar.open('Factors Template Download Successfully.', 'Okay', {
        duration: 2000,
        horizontalPosition: 'right',
        verticalPosition: 'top'
      });
    });
  }

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
    event.target.value = '';
    if (this.selectedFile) {
      this.importFactors(this.selectedFile);
    } else {
      this._snackBar.open('Please select a file first.', 'Okay', {
        duration: 2000,
        horizontalPosition: 'right',
        verticalPosition: 'top'
      });
    }
  }

  importFactors(selectedFile: File) {
    // this.authService.currentUser$.subscribe((user) => {
    //   this.loggedInUser = user;
    // });
    this.isUploading = true;
    this.message = "Uploading file, please wait...";
    this.factorsService.importFactor(selectedFile).subscribe({
      next: (response) => {
        this.isUploading = false;
        this.fetchFactorsList();
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      },
      error: (error) => {
        this.isUploading = false;
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      },
    });
  }
}



