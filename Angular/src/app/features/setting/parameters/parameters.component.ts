import { Component, inject, ViewChild } from '@angular/core';
import { ParameterService } from '../../../core/services/setting/parameter.service';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { MatSnackBar } from '@angular/material/snack-bar';
import { EntityService } from '../../../core/services/setting/entity.service';
import { DeleteDialogComponent } from '../../../core/components/delete-dialog/delete-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { UtilityService } from '../../../core/services/utility/utils';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth/auth.service';
import { PermissionsService } from '../../../core/services/setting/permission.service';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { ComputedValueModel, ComputedValuesDialogComponent } from './computed-values-dialog/computed-values-dialog.component';
import { TranslateService } from '@ngx-translate/core';
type TabKeys = 'product' | 'customer';

export interface Parameter {
  parameterId: number;
  parameterName: string;
  hasFactors: boolean;
  identifier: number;
  // isKyc: boolean;
  isRequired: boolean | null;
  entityId: number | 0;
  dataTypeId: number;
  conditionId: number | null;
  factorOrder: string | null;
  selected?: boolean;
  dataTypeName?: string;
  conditionValue?: string;
  createdBy: string;
  updatedBy: string;
  isMandatory: boolean;
  computedValues?: ComputedValueModel[] | null;
  rejectionReason: string | null;
  rejectionReasonCode: string | null;
}

@Component({
  selector: 'app-parameters',
  standalone: false,

  templateUrl: './parameters.component.html',
  styleUrl: './parameters.component.scss'
})
export class ParametersComponent {
  private _snackBar = inject(MatSnackBar);
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  parameters: Parameter[] = [];
  dataSource: MatTableDataSource<Parameter> = new MatTableDataSource<Parameter>([]);
  // displayedColumns: string[] = ['select', 'parameterName', 'dataTypeId', 'hasFactors', 'conditionId', 'isKyc', 'actions'];
  // displayedColumns: string[] = ['select', 'parameterName', 'dataTypeId', 'hasFactors', 'conditionId','createdBy','updatedBy', 'actions'];
  // Define a union type for valid keys

  displayedColumns: Record<TabKeys, string[]> = {
    customer: ['select', 'parameterName', 'isMandatory', 'createdBy', 'updatedBy', 'actions'],
    product: ['select', 'parameterName', 'isMandatory', 'createdBy', 'updatedBy', 'actions']
  };
  activeTab: TabKeys = 'product'; // Ensure activeTab is of type 'customer' or 'product'

  factorOrders: string[] = ['Ascending', 'Descending'];
  // activeTab: string = 'customer';
  selectedParameter: Parameter | null = null;
  isEditDialogOpen: boolean = false;
  formVisible = false;
  menuVisible = false;
  formData: Parameter = {
    parameterId: 0,
    parameterName: '',
    hasFactors: false,
    identifier: 0,
    // isKyc: false,
    isRequired: null,
    entityId: 0,
    dataTypeId: 0,
    conditionId: null,
    factorOrder: null,
    createdBy: '',
    updatedBy: '',
    isMandatory: false,
    computedValues: [],
    rejectionReason: null,
    rejectionReasonCode: null
  };

  entitiesList: { entityId: number; entityName: string }[] = [];
  dataTypes: { dataTypeId: number; dataTypeName: string }[] = [];
  conditions: { conditionId: number; conditionValue: string }[] = [];
  searchTerms: { [key: string]: string } = {
    customer: '',
    product: ''
  };
  selectedFile: File | null = null;
  isEditMode: boolean = false; // To differentiate between Add and Edit mode
  expressionForm: FormGroup;
  isInsertNewRecord: boolean = false;
  updatedIndexId: number = 0;
  selectedRows: Set<number> = new Set();
  selectedRowsItem: Set<number> = new Set();
  showMaxLengthWarning = false;
  computedValuesData: ComputedValueModel[] | null | undefined;

  constructor(private parameterService: ParameterService, private entityService: EntityService, private PermissionsService: PermissionsService, private dialog: MatDialog, private utilityService: UtilityService, private fb: FormBuilder, private authService: AuthService, private translate: TranslateService) {
    this.expressionForm = this.fb.group({
      parameterName: ['', Validators.required],
      dataTypeId: ['', Validators.required],
      hasFactors: [false], // Default is unchecked
      factorOrder: [{ value: '', disabled: true }], // Initially disabled
      conditionId: [{ value: '', disabled: true }],
      isMandatory: [''],
      computedValues: this.fb.array([]),
      rejectionReason: ['', Validators.required],
      rejectionReasonCode: ['', Validators.required]
    });

  }
  searchTerm: string = '';
  isDownloading: boolean = false;
  isLoading: boolean = false; // Show loader on page load
  isUploading: boolean = false;
  message: string = "Loading data, please wait...";
  loggedInUser: any = null;
  createdBy: string = '';
  autoResize(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    textarea.style.height = 'auto';
    textarea.style.height = textarea.scrollHeight + 'px';
  }
  ngOnInit(): void {
    // this.fetchEntitiesList();
    this.fetchAllParameters();
    this.fetchConditions();
    this.fetchDataTypes();
    this.dataSource.filterPredicate = (data: Parameter, filter: string) => {
      return data.parameterName.toLocaleLowerCase().includes(filter)
        || data.hasFactors.toString().toLocaleLowerCase().includes(filter)
        || (data.conditionValue ? data.conditionValue.toLocaleLowerCase().includes(filter) : false)
        || (data.dataTypeName ? data.dataTypeName.toLocaleLowerCase().includes(filter) : false);
    };
  }
  // This should be set dynamically

  validationErrors = {
    factorOrder: '',
    conditionId: ''
  };

  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }

  toggleColumn(column: string, afterColumn: string): void {
    const tabColumns = this.displayedColumns[this.activeTab];

    if (tabColumns.includes(column)) {
      // Remove column if already shown
      this.displayedColumns[this.activeTab] = tabColumns.filter(c => c !== column);
    } else {
      // Find index of the reference column
      const index = tabColumns.indexOf(afterColumn);
      if (index !== -1) {
        this.displayedColumns[this.activeTab] = [
          ...tabColumns.slice(0, index + 1), // Columns before the reference column
          column, // Insert the new column
          ...tabColumns.slice(index + 1) // Columns after the reference column
        ];
      } else {
        this.displayedColumns[this.activeTab].push(column); // Fallback: add to end
      }
    }
  }

  fetchAllParameters(): void {
    this.isLoading = true;
    this.parameterService.getParameters().subscribe({
      next: (response) => {
        // Sorting the response data based on lastModifiedDateTime in descending order
        const sortedData = response.data.sort((a: any, b: any) =>
          new Date(b.lastModifiedDateTime).getTime() - new Date(a.lastModifiedDateTime).getTime()
        );
        this.parameters = sortedData.map((param: Parameter) => ({
          ...param,
          selected: false, // Initialize the selected property
        }));
        const tabIdentifier = this.activeTab === 'customer' ? 1 : 2;
        this.dataSource.data = this.parameters.filter((param) => param.identifier === tabIdentifier); // Default to customers
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.isLoading = false;


      },
      error: (error) => {
        this._snackBar.open(this.translate.instant(error), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        this.isLoading = false;
      },
    });
  }

  onEntityNameInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;

    if (value.length > 50) {
      this.showMaxLengthWarning = true;
      this.formData.parameterName = value.slice(0, 50); // truncate
    } else {
      this.showMaxLengthWarning = false;
      this.formData.parameterName = value;
    }
  }

  fetchConditions(): void {
    this.parameterService.getConditions().subscribe({
      next: (response) => {
        this.conditions = response;
      },
      error: (error) => {
        this._snackBar.open(this.translate.instant(error), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      },
    });
  }

  get areFactorsEnabled(): boolean {
    return this.formData.hasFactors;
  }

  fetchDataTypes(): void {
    this.parameterService.getDataTypes().subscribe({
      next: (data) => {
        this.dataTypes = data;
      },
      error: (error) => {
        this._snackBar.open(this.translate.instant(error), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      },
    });
  }

  applyFilter() {
    const searchTerm = this.searchTerms[this.activeTab]?.toLowerCase() || '';
    this.dataSource.filter = searchTerm;
  }

  switchTab(tab: TabKeys): void {
    this.searchTerm = this.searchTerms[tab] || '';
    this.activeTab = tab;
    const filteredData = this.parameters.filter(param => param.identifier === (tab === 'customer' ? 1 : 2));

    this.dataSource.data = filteredData;
    this.dataSource.filter = this.searchTerm;
  }

  trackByUserId(index: number, group: any): number {
    return group.entityId;
  }

  editParameter(parameter: Parameter): void {

    this.showMaxLengthWarning = false;

    this.fetchEntitiesList();
    this.isEditMode = true;
    this.isInsertNewRecord = false;
    let factorOrderMapped = '';
    if (parameter.factorOrder === 'Asc') {
      factorOrderMapped = 'Ascending';
    } else if (parameter.factorOrder === 'Des') {
      factorOrderMapped = 'Descending';
    }
    this.updatedIndexId = parameter.parameterId;

    this.expressionForm.patchValue({
      parameterName: parameter.parameterName,
      hasFactors: parameter.hasFactors,
      entityId: parameter.entityId,
      dataTypeId: parameter.dataTypeId,
      conditionId: parameter.conditionId,
      isMandatory: parameter.isMandatory,
      factorOrder: factorOrderMapped,
      computedValues: parameter.computedValues,
      rejectionReason: parameter.rejectionReason,
      rejectionReasonCode: parameter.rejectionReasonCode
    });
    this.computedValuesData = parameter.computedValues;
    this.createdBy = parameter.createdBy;
    if (parameter.hasFactors) {
      this.expressionForm.get('factorOrder')?.enable();
      this.expressionForm.get('conditionId')?.enable();
    } else {
      this.expressionForm.get('factorOrder')?.disable();
      this.expressionForm.get('conditionId')?.disable();
    }

    this.formVisible = true; // Show the form
    this.menuVisible = false; // Close any open menus
  }

  fetchEntitiesList() {
    this.entityService.getEntitiesList().subscribe({
      next: (response) => {
        this.entitiesList = response.data.map((entity: any) => ({
          entityId: entity.entityId,
          entityName: entity.entityName,
        }));
      },
      error: (err) => {
        console.error('Error fetching entities list:', err);
      }
    });
  }

  submitEditForm(): void {
    // this.authService.currentUser$.subscribe((user) => {
    //   this.loggedInUser = user;
    // });
    if (this.expressionForm.invalid) {
      this._snackBar.open(this.translate.instant('Please fill out all required fields!'), this.translate.instant('Close'), { duration: 2000 });
      this.expressionForm.markAllAsTouched();
      return;
    }
    // Remove the selected property
    const { selected, ...cleanFormData } = this.formData;

    // Adjust factorOrder for consistency
    if (cleanFormData.factorOrder === "Ascending") {
      cleanFormData.factorOrder = "Asc";
    } else if (cleanFormData.factorOrder === "Descending") {
      cleanFormData.factorOrder = "Des";
    }

    const payload = {
      parameterId: this.isInsertNewRecord ? 0 : this.updatedIndexId,
      parameterName: this.expressionForm.value.parameterName.trim(),
      hasFactors: this.expressionForm.value?.hasFactors == null ? false : this.expressionForm.value?.hasFactors,
      identifier: this.activeTab === 'customer' ? 1 : 2,
      isRequired: true,
      createdBy: '',
      updatedBy: '',
      entityId: this.expressionForm.value.entityId,
      dataTypeId: this.expressionForm.value.dataTypeId,
      conditionId: this.expressionForm.value.conditionId,
      factorOrder: this.expressionForm.value.factorOrder == "Ascending" ? "Asc" : "Des",
      isMandatory: this.expressionForm.value.isMandatory ?? false,
      computedValues: this.expressionForm.value.computedValues ?? [],
      rejectionReason: this.expressionForm.value.rejectionReason ?? null,
      rejectionReasonCode: this.expressionForm.value.rejectionReasonCode ?? null
    };
    if (this.isEditMode) {
      // Edit mode
      // payload.createdBy = this.createdBy;
      // payload.updatedBy = this.loggedInUser.user.userName;
      this.parameterService.updateParameter(payload).subscribe({
        next: (response) => {
          this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
          this.formVisible = false;
          this.fetchAllParameters(); // Refresh the parameters list
        },
        error: (error) => {
          console.log(error);
          const msg = error?.error?.message || 'Access denied';
          this._snackBar.open(this.translate.instant(msg), this.translate.instant('Okay'), {
            horizontalPosition: 'right',
            verticalPosition: 'top',
            duration: 3000
          });

        }
      });
    } else {
      // Add mode
      // payload.createdBy = this.loggedInUser.user.userName;
      // payload.updatedBy = this.loggedInUser.user.userName;
      this.parameterService.addParameter(payload).subscribe({
        next: (response) => {
          this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
          this.formVisible = false;
          this.fetchAllParameters(); // Refresh the parameters list
        },
        error: (error) =>

          this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
            horizontalPosition: 'center',
            verticalPosition: 'top', duration: 3000,


          }),
      })

        ;
    }


    setTimeout(() => {
      this.switchTab(this.activeTab);
    }, 1000);
  }

  closeForm(): void {
    this.formVisible = false;
    this.isEditMode = false; // Reset edit mode
    this.formData = {
      parameterId: 0,
      parameterName: '',
      hasFactors: false,
      identifier: 0,
      // isKyc: false,
      isRequired: null,
      entityId: 0,
      dataTypeId: 0,
      conditionId: null,
      factorOrder: null,
      createdBy: '',
      updatedBy: '',
      isMandatory: false,
      computedValues: null,
      rejectionReason: null,
      rejectionReasonCode: null
    }; // Reset form data
    this.fetchAllParameters();
  }

  deleteParameter(parameter: Parameter): void {
    const dialogRef = this.dialog.open(DeleteDialogComponent, {
      data: {
        title: this.translate.instant('Confirm'),
        message: this.translate.instant('Are you sure you want to delete the parameter: "{{parameterName}}"?', { parameterName: parameter.parameterName })
      }
    });

    dialogRef.afterClosed().subscribe(result => {

      if (!result?.delete) return;

      this.parameterService.deleteParameter(parameter.parameterId).subscribe({
        next: (response) => {

          if (response.isSuccess) {

            this.parameters = this.parameters.filter(
              param => param.parameterId !== parameter.parameterId
            );

            this.dataSource.data = this.parameters.filter(
              param => param.identifier === (this.activeTab === 'customer' ? 1 : 2)
            );

            this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top',
              duration: 3000
            });

            this.fetchAllParameters();
            this.closeForm();

          } else {

            this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top',
              duration: 3000
            });

          }
        },

        error: (error) => {
          console.log(error);

          const msg = error.error?.message || error.message || 'Something went wrong.';

          this._snackBar.open(this.translate.instant(msg), this.translate.instant('Okay'), {
            horizontalPosition: 'right',
            verticalPosition: 'top',
            duration: 3000
          });
        }
      });
    });
  }

  toggleMenu(): void {
    this.menuVisible = !this.menuVisible;
  }

  closeMenu(): void {
    this.menuVisible = true;

  }

  addParameter(): void {
    this.showMaxLengthWarning = false;
    this.fetchEntitiesList();
    this.isEditMode = false;
    this.isInsertNewRecord = true;
    this.menuVisible = false;
    this.formData = {
      parameterId: 0, // New parameter
      parameterName: '',
      hasFactors: false,
      identifier: this.activeTab === 'customer' ? 1 : 2,
      // isKyc: false,
      isRequired: true,
      entityId: 0,
      dataTypeId: 0,
      conditionId: 0,
      factorOrder: null,
      createdBy: '',
      updatedBy: '',
      isMandatory: false,
      computedValues: null,
      rejectionReason: null,
      rejectionReasonCode: null
    };
    this.formVisible = true; // Show the form
    this.expressionForm.reset();
    this.handleHasFactorsChange();
  }

  deleteSelectedParameters(): void {
    this.menuVisible = false;
    if (this.selectedRows.size === 0) {
      this._snackBar.open(this.translate.instant('Please select at least one row to delete'), this.translate.instant('Okay'), {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000
      });
      return;
    }
    const dialogRef = this.dialog.open(DeleteDialogComponent);

    dialogRef.afterClosed().subscribe(result => {
      if (result?.delete) {
        const selectedIds = Array.from(this.selectedRows);
        this.parameterService.deleteMultipleParameters(selectedIds).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              // Remove the deleted parameters from the local list
              this.parameters = this.parameters.filter(
                (param) => !selectedIds.includes(param.parameterId)
              );
              this.dataSource.data = this.parameters.filter(
                (param) =>
                  param.identifier === (this.activeTab === 'customer' ? 1 : 2)
              );
              this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
                horizontalPosition: 'right',
                verticalPosition: 'top', duration: 3000
              });
              this.fetchAllParameters();

            }
            else {
              this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
                horizontalPosition: 'right',
                verticalPosition: 'top', duration: 3000
              });
            }
          },
          error: (err) => this._snackBar.open(this.translate.instant(err.message), this.translate.instant('Okay'), {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          })
        });
      }
    });
  }

  isAllPageSelected(): boolean {
    const currentPageData = this.getCurrentPageData();
    return currentPageData.every((row: any) => this.selectedRows.has(row.parameterId));
  }

  toggleSelectAll(event: MatCheckboxChange) {
    const currentPageData = this.getCurrentPageData(); // Paginated rows
    if (event.checked) {
      currentPageData.forEach((row: any) => this.selectedRows.add(row.parameterId));
    } else {
      currentPageData.forEach((row: any) => this.selectedRows.delete(row.parameterId));
    }
  }

  isSomePageSelected(): boolean {
    const currentPageData = this.getCurrentPageData();
    return currentPageData.some((row: any) => this.selectedRows.has(row.parameterId)) && !this.isAllPageSelected();
  }

  toggleSelection(event: MatCheckboxChange, parameterId: number) {
    if (event.checked) {
      this.selectedRows.add(parameterId);
    } else {
      this.selectedRows.delete(parameterId);
    }
  }

  getCurrentPageData(): any[] {
    if (!this.paginator) return []; // Return empty array if paginator is undefined
    const startIndex = this.paginator.pageIndex * this.paginator.pageSize;
    const endIndex = startIndex + this.paginator.pageSize;
    return this.dataSource.filteredData.slice(startIndex, endIndex);
  }

  sanitizeCode(event: any) {
    event.target.value = this.utilityService.sanitizeCode(event.target.value);
  }

  getConditionName(conditionId: number | null, parameter: Parameter): string {
    const condition = this.conditions.find((c) => c.conditionId === conditionId);
    parameter.conditionValue = condition ? condition.conditionValue : '';
    return condition ? condition.conditionValue : '';
  }

  getDataTypeName(dataTypeId: number, parameter: Parameter): string {
    const dataType = this.dataTypes.find((dt) => dt.dataTypeId === dataTypeId);
    parameter.dataTypeName = dataType ? dataType.dataTypeName : '';
    return dataType ? dataType.dataTypeName : '';
  }

  onFieldChange(field: string): void {
    if (this.areFactorsEnabled) {
      if (field === 'factorOrder') {
        this.validationErrors.factorOrder = this.formData.factorOrder ? '' : 'Factor Order is required when Factor is enabled';
      }
      if (field === 'conditionId') {
        this.validationErrors.conditionId = this.formData.conditionId ? '' : 'Condition is required when Factor is enabled';
      }
    }
  }

  handleHasFactorsChange(): void {
    const hasFactors = this.expressionForm.get('hasFactors')?.value;

    if (hasFactors) {
      // Enable factorOrder and conditionId when hasFactors is checked
      this.expressionForm.get('factorOrder')?.enable();
      this.expressionForm.get('conditionId')?.enable();
    } else {
      // Reset values and disable fields when hasFactors is unchecked
      this.expressionForm.get('factorOrder')?.reset();
      this.expressionForm.get('conditionId')?.reset();
      this.expressionForm.get('factorOrder')?.disable();
      this.expressionForm.get('conditionId')?.disable();
    }

    // Force validation update to disable the Add button if invalid
    this.expressionForm.updateValueAndValidity();
  }

  // handleHasFactorsChange(): void {
  //   // if (!this.formData.hasFactors) {
  //   //   // Clear the values if hasFactors is unchecked
  //   //   this.formData.factorOrder = null;
  //   //   this.formData.conditionId = null;

  //   // }
  //   if (this.areFactorsEnabled) {
  //     this.validationErrors.factorOrder = this.formData.factorOrder ? '' : 'Factor Order is required when Factor is enabled';
  //     this.validationErrors.conditionId = this.formData.conditionId ? '' : 'Condition is required when Factor is enabled';
  //   } else {
  //     // Clear errors if factors are disabled
  //     this.validationErrors.factorOrder = '';
  //     this.validationErrors.conditionId = '';
  //     this.formData.factorOrder = null;
  //     this.formData.conditionId = null;

  //   }
  // }

  // exportParameters() {
  //   let identifier = 0;
  //   let excelName = "";
  //   if (this.activeTab === 'customer') {
  //     identifier = 1;
  //     excelName = "Customer-Parameters.xlsx"
  //   } else {
  //     identifier = 2;
  //     excelName = "Product-Parameters.xlsx"
  //   }
  //   this.parameterService.exportParameters(identifier).subscribe({
  //     next: (response: Blob) => {
  //       const url = window.URL.createObjectURL(response);
  //       const anchor = document.createElement('a');
  //       anchor.href = url;
  //       anchor.download = excelName;
  //       document.body.appendChild(anchor);
  //       anchor.click();
  //       document.body.removeChild(anchor);
  //       window.URL.revokeObjectURL(url);
  //     },
  //     error: (error) => {
  //       this._snackBar.open(error.message, 'Okay', {
  //         horizontalPosition: 'right',
  //         verticalPosition: 'top', duration: 3000
  //       });
  //     }
  //   });
  // }

  exportParameters() {
    let identifier = 0;
    let excelName = "";
    if (this.activeTab === 'customer') {
      identifier = 1;
      excelName = "Customer-Parameters.xlsx"
    } else {
      identifier = 2;
      excelName = "Product-Parameters.xlsx"
    }
    const selectedIds = Array.from(this.selectedRows);
    const searchTerm = selectedIds.length === 0 ? (this.searchTerms[this.activeTab] || '').trim() : '';
    this.parameterService.exportParameters(identifier, selectedIds, searchTerm).subscribe({
      next: (response: Blob) => {
        const url = window.URL.createObjectURL(response);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = excelName;
        document.body.appendChild(anchor);
        anchor.click();
        document.body.removeChild(anchor);
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    });
  }

  downloadTemplate() {
    let excelName = "";
    if (this.activeTab === 'customer') {
      excelName = 'Customer-Template.xlsx'
    } else {
      excelName = 'Product-Template.xlsx'
    }
    this.isDownloading = true;
    this.message = "Please wait, template is downloading...";
    this.parameterService.downloadTemplate().subscribe((response) => {
      this.isDownloading = false;
      const blob = new Blob([response], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = excelName; // Filename for the download
      a.click();
      window.URL.revokeObjectURL(url);
      this._snackBar.open(this.translate.instant('Parameter Template Download Successfully.'), 'Okay', {
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
      this.importParameter(this.selectedFile);
    } else {
      this._snackBar.open(this.translate.instant('Please select a file first.'), this.translate.instant('Okay'), {
        duration: 2000,
        horizontalPosition: 'right',
        verticalPosition: 'top'
      });
    }
  }

  importParameter(selectedFile: File) {
    // this.authService.currentUser$.subscribe((user) => {
    //   this.loggedInUser = user;
    // });
    this.isUploading = true;
    this.message = "Uploading file, please wait...";
    this.parameterService.importParameter(selectedFile, this.activeTab === 'customer' ? 1 : 2).subscribe({
      next: (response) => {
        this.isUploading = false;
        this.fetchAllParameters();
        this.fetchEntitiesList();
        this.fetchConditions();
        this.fetchDataTypes();
        this.dataSource.filterPredicate = (data: Parameter, filter: string) => {
          return data.parameterName.toLocaleLowerCase().includes(filter)
            || data.hasFactors.toString().toLocaleLowerCase().includes(filter)
            || (data.conditionValue ? data.conditionValue.toLocaleLowerCase().includes(filter) : false)
            || (data.dataTypeName ? data.dataTypeName.toLocaleLowerCase().includes(filter) : false);
        };
        setTimeout(() => {
          this.switchTab(this.activeTab);
        }, 800);
        this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      },
      error: (error) => {
        this.isUploading = false;
        this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      },
    });
  }



  openComputedValuesDialog(): void {
    const computedValuesControl = this.expressionForm.get('computedValues');
    const dataTypeId = this.expressionForm.get('dataTypeId')?.value;

    const dialogRef = this.dialog.open(ComputedValuesDialogComponent, {
      width: '50%',
      maxWidth: 'none',
      data: {
        computedValues: computedValuesControl?.value?.length === 0
          ? this.computedValuesData
          : computedValuesControl?.value,
        dataTypeId: dataTypeId
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.expressionForm.setControl(
          'computedValues',
          this.fb.array(
            result.map((v: ComputedValueModel) => this.fb.group({
              computedParameterType: [+v.computedParameterType, Validators.required],
              fromValue: [v.fromValue.toString()],
              toValue: [v.toValue.toString()],
              rangeType: [v.rangeType !== null ? +v.rangeType! : null],
              parameterExactValue: [v.parameterExactValue],
              computedValue: [v.computedValue, Validators.required]
            }))
          )
        );
      }
    });
  }

}






