import { Component, HostListener, inject, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MappingService } from '../../../core/services/connections/mapping.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { IntegrationService } from '../../../core/services/connections/integration.service';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { RolesService } from '../../../core/services/setting/role.service';

export interface mappingFormData {
  nodeId: number | null,
  apiid: number | null,
  parameterId: number | null,
  mapFunctionId: number | null,
}

export interface MappingRecord {
  nodeId: number,
  apiid: number,
  parameterId: number,
  mapFunctionId: number,
  apiname: string | undefined,
  parameter: string | undefined,
  xmlNode: string,
  xmlParent: string,
  isChecked: boolean
}

export interface MappingTreeDatasourceRecord {
  xmlNode: string,
  xmlParent: string,
  isChecked: boolean
}

export interface MappingTreeRecord {
  nodeId: number,
  apiid: number,
  parameterId: number,
  mapFunctionId: number,
  apiname: string,
  parameter: string,
  xmlNode: string,
  xmlParent: string,
  isChecked: boolean
}
@Component({
  selector: 'app-mapping',
  standalone: false,

  templateUrl: './mapping.component.html',
  styleUrl: './mapping.component.scss'
})

export class MappingComponent {
  private _snackBar = inject(MatSnackBar);
  menuVisible: boolean = false;
  formVisible: boolean = true;
  isEditMode: boolean = false;
  mappingForm!: FormGroup;
  mappingFormData: mappingFormData = {
    nodeId: null,
    apiid: null,
    parameterId: null,
    mapFunctionId: null,
  }
  webServicesRecords: { nodeId: number, nodeName: string }[] = []
  mappingFunctionRecords: { mapFunctionId: number, mapFunctionValue: string }[] = [];
  methodRecords: { apiid: number, apiname: string, nodeId: number }[] = [];
  filteredMethodRecords: { apiid: number, apiname: string }[] = [];
  parameterRecords: { parameterId: number, parameterName: string }[] = [];
  displayedMappingColumns: string[] = ['API', 'Parameter', 'XMLNode', 'XMLParent', 'Actions'];
  displayedMappingTreeColumns: string[] = ['Select', 'XMLNode', 'XMLParent'];
  mappingRecords: MappingRecord[] = [];
  mappingTreeRecords: MappingTreeRecord[] = [];
  activeTab: string = 'mappingTree';
  mappingDatasource = new MatTableDataSource<MappingRecord>(this.mappingRecords);
  mappingTreeDatasource = new MatTableDataSource<MappingTreeRecord>([]);
  selectedRow!: any;
  selectedMethod: string = '';
  isDisabled: boolean = true;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  pageSize: number = 5;
  currentPage: number = 0;
  paginatedRows: any[] = [];
  searchTerms: { [key: string]: string } = {
    mappingTree: '',
    mapping: ''
  };
  constructor(private fb: FormBuilder, private mappingService: MappingService,private rolesService:RolesService, private integrationService: IntegrationService) {
    this.mappingForm = this.fb.group({
      nodeId: [null, [Validators.required]],
      apiid: [null, [Validators.required]],
      parameterId: [{ value: null }, [Validators.required]],
      mapFunctionId: [{ value: null }, [Validators.required]],
    
    });
  }

  hasPermission(roleId: number): boolean {
    return this.rolesService.hasPermission(roleId);
  }

  async ngOnInit(): Promise<void> {
    await this.fetchNodeAPIList();
    await this.fetchAllParameters();
    await this.fetchMappingList();
    this.fetchAllMappingFunctions();
    this.fetchNodeList();
  }

  ngAfterViewInit() {
    this.mappingDatasource.paginator = this.paginator;
    this.mappingDatasource.sort = this.sort;
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value.trim().toLowerCase();
    this.mappingDatasource.filter = filterValue;
    this.mappingDatasource.paginator = this.paginator;
    this.mappingDatasource.sort = this.sort;
  }

  toggleMenu() {
    this.menuVisible = !this.menuVisible;
  }

  closeMenu() {
    this.menuVisible = false;
  }

  openForm() {
    this.formVisible = true;
    this.isEditMode = false;
    this.mappingFormData = { nodeId: null, apiid: null, parameterId: null, mapFunctionId: null, };
    this.mappingForm.reset();
    this.mappingForm.get('parameterId')?.disable();
    this.mappingForm.get('mapFunctionId')?.disable();
    this.activeTab = 'mappingTree';
  }

  deleteMulSelectedRows() {

  }

  addRecord() {
    if (this.isEditMode) {
      const formValue = this.mappingForm.value;

      const requestBody = {
        apiid: formValue.apiid,
        nodeId: formValue.nodeId,
        parameterId: formValue.parameterId,
        mapFunctionId: formValue.mapFunctionId,
        xmlnode: this.selectedRow?.xmlNode,
        xmlparent: this.selectedRow?.xmlParent
      };

      this.mappingService.updateMap(requestBody).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.fetchMappingList();
            this.activeTab = 'mapping';
            this.closeForm();
            this.selectedRow = null;
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000,
            });
          } else {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000,
            });
          }
        },
        error: (error) => {
          this._snackBar.open(error, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000,
          });
        }
      });
    }
    else {
      const formValue = this.mappingForm.value;

      const requestBody = {
        apiid: formValue.apiid,
        nodeId: formValue.nodeId,
        parameterId: formValue.parameterId,
        mapFunctionId: formValue.mapFunctionId,
        xmlparent: formValue.xmlParent,
        xmlnode: formValue.xmlNode
      }
      this.mappingService.addMap(requestBody).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.fetchMappingList();
            this.activeTab = 'mapping';
            this.closeForm();
            this.selectedRow = null;
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000,
            });
          } else {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000,
            });
          }
        },
        error: (error) => {
          this._snackBar.open(error, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000,
          });
        }
      });
    }
  }

  onSelectMapTree(row: any): void {
    if (this.selectedRow === row) {
      row.isChecked = !row.isChecked;
      if (!row.isChecked) {
        this.selectedRow = null;
      }
      this.mappingForm.get('xmlNode')?.reset();
      this.mappingForm.get('xmlParent')?.reset();
    } else {
      if (this.selectedRow) {
        this.selectedRow.isChecked = false;
      }

      row.isChecked = true;
      this.selectedRow = row;
      this.mappingForm.controls['xmlNode'].setValue(this.selectedRow.xmlNode);
      this.mappingForm.controls['xmlParent'].setValue(this.selectedRow.xmlParent);
    }
  }


  onSelect(type: string, event: any) {
    const value = event.target.value;
    if (type === 'method') {
      this.mappingForm.get('apiid')?.setValue(value);
      if (this.mappingForm.get('apiid')?.value && this.mappingForm.get('nodeId')?.value) {
        this.mappingForm.get('parameterId')?.enable();
        this.mappingForm.get('mapFunctionId')?.enable();
      }
    }
  }


  onWebServiceSelect(event: any) {
    const nodeId = event.target.value;
    this.setFilteredMethodRecords(nodeId);
    this.mappingForm.get('apiid')?.reset();
  }

  setFilteredMethodRecords(nodeId: number | null) {
    this.filteredMethodRecords = this.methodRecords.filter(method => method.nodeId == nodeId);
  }

  closeForm() {
    this.formVisible = false;
    this.isEditMode = false;
    this.mappingDatasource.data = this.mappingDatasource.data.map((mapTree) => {
      mapTree.isChecked = false;
      return mapTree;
    });
    this.mappingFormData = { nodeId: null, apiid: null, parameterId: null, mapFunctionId: null};
    this.selectedRow = null;
    this.isDisabled = true;
  }

  @HostListener('document:click', ['$event.target'])
  onClickOutside(targetElement: HTMLElement) {
    const dropdown = document.querySelector('.dropdown-toggle');
    const menu = document.querySelector('.dropdown-menu');

    if (!dropdown?.contains(targetElement) && !menu?.contains(targetElement)) {
      this.closeMenu();
    }
  }

  fetchAllMappingFunctions() {
    this.mappingService.getAllMappingFunctions().subscribe({
      next: (response) => {
        this.mappingFunctionRecords = response.data;

      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      },
    });
  }

  fetchNodeList() {
    this.integrationService.getNodeList().subscribe({
      next: (response) => {
        this.webServicesRecords = response.data;
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      },
    });
  }

  async fetchNodeAPIList() {
    this.integrationService.getNodeAPIList().subscribe({
      next: (response) => {
        this.methodRecords = response.data;
        this.filteredMethodRecords = response.data;
        this.mapApiAndParameters();
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      },
    });
  }

  async fetchAllParameters() {
    this.mappingService.getAllParameters().subscribe({
      next: (response) => {
        this.parameterRecords = response.data;
        this.mapApiAndParameters();
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      },
    });
  }

  async fetchMappingList() {
    this.mappingService.getMappingTreeList().subscribe({
      next: (response) => {
        console.log('response.data--->',response.data);
        this.mappingDatasource.data = response.data.map((item: any) => {
          return {
            apiid: item.apiid,
            apiname: undefined,
            mapFunctionId: item.mapFunctionId,
            nodeId: item.nodeId,
            parameterId: item.parameterId,
            parameter: undefined,
            xmlNode: item.xmlnode,
            xmlParent: item.xmlparent
          }
        });
        this.mappingDatasource.paginator = this.paginator;
        this.mappingDatasource.sort = this.sort;
        this.mapApiAndParameters();
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      },
    });
  }

  mapApiAndParameters() {
    this.mappingDatasource.data = this.mappingDatasource.data.map((item: MappingRecord) => {
      const api = this.methodRecords.find(method => method.apiid == item.apiid);
      const parameter = this.parameterRecords.find(param => param.parameterId == item.parameterId)
      return {
        ...item,
        apiname: item.apiname ?? api?.apiname,
        parameter: item.parameter ?? parameter?.parameterName,
      }
    });
  }

  editMappingRecord(record: MappingRecord) {
    this.isEditMode = true;
    this.formVisible = true;
    this.isDisabled = false;
    this.mappingForm.get('parameterId')?.enable();
    this.mappingForm.get('mapFunctionId')?.enable();
    this.activeTab = 'mappingTree';
    this.setFilteredMethodRecords(record.nodeId);
    this.mappingFormData = { ...record }
    this.selectedRow = record;
    this.mappingForm.controls['xmlNode'].setValue(this.selectedRow.xmlNode);
    this.mappingForm.controls['xmlParent'].setValue(this.selectedRow.xmlParent);
    this.mappingDatasource.data = this.mappingDatasource.data.map((mapTree) => {
      if (mapTree == record) {
        mapTree.isChecked = true;
      }
      else {
        mapTree.isChecked = false;
      }
      return mapTree;
    })
  }

  deleteMappingrecord(record: any) {
    if (confirm(`Are you sure you want to delete Map?`)) {
      this.mappingService.deleteMap({ apiId: record.apiid, nodeId: record.nodeId, parameterId: record.parameterId }).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.fetchMappingList()
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000,
            });
          } else {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000,
            });
          }
        },
        error: (error) => {
          this._snackBar.open(error.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000,
          });
        },
      });
    }
  }

  switchTab(tab: string) {
    this.activeTab = tab;
    if(this.activeTab == "mappingTree") {
      this.formVisible = true;
    }
    else {
      this.formVisible = false;
    }
    this.mappingDatasource.paginator = this.paginator;
    this.mappingDatasource.sort = this.sort;
  }

}
