import { Component, inject, ViewChild } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { UtilityService } from '../../../../core/services/utility/utils';
import { FactorsService } from '../../../../core/services/setting/factors.service';
import { ExceptionManagementService } from '../../../../core/services/setting/exception-management.service';
import { RolesService } from '../../../../core/services/setting/role.service';

export interface ExceptionRecord {
  [x: string]: any;
  amountType: string;
  exceptionManagementId: number | null;
  exceptionName: string;
  parameterID?: number | null;
  isTemporary: boolean;
  startDate: string | null;
  endDate: string | null;
  fixedPercentage: number;
  variationPercentage: number;
  scope: string[];
  description: string;
  isActive: boolean;
  productId: [];
  Products: [];
  expShown: [''],
  expressionComponents?: { type: string; value: any; id?: number }[]; // NEW

  // createdByDateTime: string;
  // updatedByDateTime: string;
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

type ExceptionManagementTabs = 'Exceptions' | 'Exception Rule';

@Component({
  selector: 'app-exception-management',
  standalone: false,

  templateUrl: './exception-management.component.html',
  styleUrl: './exception-management.component.scss'
})

export class ExceptionManagementComponent {
  private _snackBar = inject(MatSnackBar);
  parametersList: any[] = [];
  exceptionsTableData: any[] = [];
  formVisible:boolean=false;
  searchTerms: { [key: string]: string } = {
    exception: '',
    exceptionParameter: ''
  };
  searchTerm: string = '';
  activeTab: ExceptionManagementTabs = 'Exceptions';
  formData: any;
  isEditMode : boolean = false;
  limitAmountType: 'Fixed' | 'Variation' = 'Fixed';
  constructor(
    private utilityService: UtilityService,
    private factorsService: FactorsService,
    private exceptionService: ExceptionManagementService,private rolesService:RolesService) {
      this.fetchTableData()
  }

  isLoading: boolean = false;
  isDownloading: boolean = false
  isUploading: boolean = false
  message: string = "Loading data, please wait...";
  switchTab(tab: ExceptionManagementTabs): void {
    this.searchTerm = this.searchTerms[tab] || '';
    this.activeTab = tab;
    this.formVisible = false;
  }

  hasPermission(roleId: string): boolean {
    return this.rolesService.hasPermission(roleId);
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value.trim().toLowerCase();

    this.searchTerm = filterValue; 
  }

  openForm() {

    this.formVisible = true;
  }
  onnewClick(){
    this.formVisible = true;
    this.isEditMode = false;
  
    this.formData = {
      exceptionManagementId: 0,
      exceptionName: '',
      parameterID: 0,
      isTemporary: false,
      scope: [],
      description: '',
      isActive: true,
      startDate: '',
      endDate: '',
      fixedPercentage: 0,
      variationPercentage: 0,
      productId: [],
      limitAmountType: '',
      Products: [],
      expShown: ['']}
  }
  onSubmit() {
    this.formVisible = false;
    this.isEditMode = false;
    this.fetchTableData()
  }

  onFormClose() {
    this.formVisible = false;
    this.isEditMode = false;
    this.formData = {
      exceptionID:null,
      exceptionName: '',
      parameterID: null,
      isTemporary: false,
      scope: [],
      description: '',
      isActive: false,
      startDate: '',
      endDate: '',
      fixedPercentage: 0,  // For fixed limit percentage
      variationPercentage: 0,
      products:[]
    }
  }

  fetchParametersList(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.factorsService.getParametersList().subscribe({
        next: (response) => {
          if (response && response.data) {
            this.parametersList = response.data;
          } else {
            console.error('Invalid parameters response:', response);
          }
          resolve();
        },
        error: (err) => reject(err),
      });
    });
  }

  fetchExceptionList() {
    this.isLoading = true;
    this.exceptionService.getExceptionList().subscribe({
      next: (response) => {
        if (response && response.data) {
          this.exceptionsTableData = response.data.map((item: any) => {
            const parameter = this.parametersList.find(
              (param) => param.parameterId === item.parameterID
            );
           
            console.log("fetchExceptionList")
            return {
              exceptionManagementId: item.exceptionManagementId,
              ExceptionName: item.exceptionName,
              ParameterName: parameter ? parameter.parameterName : 'Unknown Parameter',
              TemporaryException: item.isTemporary,
              Scope: item.scope ? item.scope.split(',') : [],
              Description: item.description,
              Status: item.isActive == true ? "Active" : "inactive",
              CreatedByDateTime: item.createdByDateTime,
              UpdatedByDateTime: item.updatedByDateTime,
              FixedPercentage: item.fixedPercentage,
              VariationPercentage: item.variationPercentage,
              exceptionStartDate: item.startDate,
              exceptionEndDate: item.endDate,
              expShown: item.expShown,


            };
          });
          this.isLoading = false;
        }
       
      },
      error: (error: any) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        this.isLoading = false;
      },
    });
  }

  async fetchTableData() {
    //Make sure we get all the parameters before the exceptions
    await this.fetchParametersList();
    this.fetchExceptionList()
  }

  async fetchExceptionById(Id: number) {
    return new Promise(async (resolve, reject) => {
     
      await this.exceptionService.getExceptionById(Id).subscribe({
        next: (response) => {
          response.data.scope = response?.data?.scope?.split(',');
          response.data.startDate = this.formateDate(response?.data?.startDate);
          response.data.endDate = this.formateDate(response?.data?.endDate);


          resolve(response.data)
         
        
        },
        error: (err:any) => reject(err),
      });
    });
  }

  
  sanitizeCode(event: any) {
    event.target.value = this.utilityService.sanitizeCode(event.target.value);
  }

  async editForm(record: any) {

    const productIds = record.Products?.map((p: any) => p.productId) || [];

    record = {
      exceptionManagementId: record.exceptionManagementId,
      exceptionName: record.ExceptionName,
      parameterName: record.ParameterName,
      isTemporary: record.TemporaryException,
      scope: record.Scope,
      description: record.Description,
      isActive: record.Status == true ? "Active" : "inactive",
      fixedPercentage: record.FixedPercentage > 0 ? record.FixedPercentage : record.VariationPercentage,
      variationPercentage: record.VariationPercentage,
      startDate: this.formateDate(record.exceptionStartDate),
      endDate: this.formateDate(record.exceptionEndDate),
      expShown: record.expShown
      
    }
    //console.log(record.expShown)
    this.formData = await this.fetchExceptionById(record.exceptionManagementId);

    this.openForm();
    this.isEditMode = true;
  
    // this.formData = record;
  }

  
  deleteException(Id: any) {
    this.exceptionService.deleteException(Id).subscribe({
      next: (response) => {
        this.fetchTableData();
        this.onFormClose();
      },
      error: (err:any) => console.log(err),
    });
   
  }

  formateDate(date: any) {
    if (!date) return null;
    let newDate = new Date(date);
    let month: any = newDate.getMonth() + 1;
    let year = newDate.getFullYear();
    let day: any = newDate.getDate();
    if(month < 10) {
      month = `0${month}`;
    }
    if(day < 10) {
      day = `0${day}`;
    }
    return `${year}-${month}-${day}`
  }

}
