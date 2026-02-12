import { Component, HostListener, inject, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { IntegrationService } from '../../../core/services/connections/integration.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ParameterService } from '../../../core/services/setting/parameter.service';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { DeleteDialogComponent } from '../../../core/components/delete-dialog/delete-dialog.component';
import { FactorsService } from '../../../core/services/setting/factors.service';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { PermissionsService } from '../../../core/services/setting/permission.service';
import { HttpClient, HttpHeaders }  from '@angular/common/http';
import { Observable } from 'rxjs';
import { UtilityService } from '../../../core/services/utility/utils';
export interface NodeRecord {
  nodeId: number | null;
  nodeName: string,
  code: string,
  nodeDesc: string,
  nodeUrl: string,
  urlType: string,
  apiuserName: string,
  apipassword: string,
  entityId: number | null;
}

export interface APIRecord {
  apiid: number | null;
  apiname: string | null,
  apidesc: string,
  nodeId: number | null;
  httpMethodType: string,
  binaryXml: string,
  xmlfileName: string,
  header: string
  requestParameters: string ,
  requestBody: string,
  responseFormate: string,
  isActive: boolean | false;
  executionOrder: number |null;

}
export interface ApiParameterRecord {
  apiParameterId: number | null;
  parameterDirection: 'Input' | 'Output';
  parameterName: string;
  isRequired: boolean;
  defaultValue: string | null;
  apiId: number | null;
  parameterType: string;
}
export interface ApiListModel {
  apiid: number;
  apiname: string;
  endpointPath: string;
  requestBody: string;
  requestParameters: string;
  responseRootPath: string;
  targetTable: string;
  isActive: boolean;
  apidesc: string;
  nodeId: number;
  nodeUrl: string;
  httpMethodType: string;
  binaryXml?: string;
  xmlfileName?: string;
  header?: string;
  createdBy?: string;
  createdByDateTime?: string;
  updatedBy?: string;
  updatedByDateTime?: string;
}

export interface APIDetailsRecord {
  apidetailsId: number | null;
  fromApiid: number | null;
  callingParamName: string,
  apiid: number | null;
  sourceApiparam: string,
  dataTypeId: number | null;
}

@Component({
  selector: 'app-integration',
  standalone: false,
  templateUrl: './integration.component.html',
  styleUrl: './integration.component.scss'
})

export class IntegrationComponent implements OnInit {
  private _snackBar = inject(MatSnackBar);
  apiParameterForm!: FormGroup;
  apiParameterRecords: ApiParameterRecord[] = [];
  apiParameterDataSource = new MatTableDataSource<ApiParameterRecord>(this.apiParameterRecords);
  apiListForDropdown: any[] = [];
  searchTerm: string = '';
  searchTerms: { [key: string]: string } = {
    nodes: '',
    api: '',
    apiDetails: ''
  };
  selectedApi: any = null;
  showApiParameterSection: boolean = false;

  searchGroup: string = '';
  menuVisible = false;
  formVisible = false;
  NodeAPIVisible = false;
  NodeAPIDetailsVisible = false;
  isEditMode = false;
  activeTab: string = 'nodes';
  nodeformData: NodeRecord = {
    nodeId: 0,
    nodeName: '',
    code: '',
    nodeDesc: '',
    nodeUrl: '',
    urlType: '',
    apiuserName: '',
    apipassword: '',
    entityId: 0
  };
  nodeAPIformData: APIRecord = {
    apiid: null,
    apiname: '',
    apidesc: '',
    nodeId: null,
    httpMethodType: '',
    binaryXml: '',
    xmlfileName: '',
    header: '',
    requestParameters: '',
    requestBody: '',
    responseFormate: '',
    isActive: false,
    executionOrder:null
  };
  nodeAPIDetailsformData: APIDetailsRecord = {
    apidetailsId: 0,
    fromApiid: null,
    callingParamName: '',
    apiid: null,
    sourceApiparam: '',
    dataTypeId: null
  };
  @ViewChild('nodeSort') nodeSort!: MatSort;
  @ViewChild('nodePaginator') nodePaginator!: MatPaginator;

  @ViewChild('apiSort') apiSort!: MatSort;
  @ViewChild('apiPaginator') apiPaginator!: MatPaginator;

  @ViewChild('apiDetailsPaginator') apiDetailsPaginator!: MatPaginator;
  @ViewChild('apiDetailsSort') apiDetailsSort!: MatSort;

  @ViewChild('apiParamsPaginator') apiParamsPaginator!: MatPaginator;
  @ViewChild('apiParamsSort') apiParamsSort!: MatSort;
  noderecords: NodeRecord[] = [];
  nodeAPIs: any[] = [];
  nodeDataSource = new MatTableDataSource<NodeRecord>(this.noderecords);
  apirecords: APIRecord[] = [];
  nodeAPIDataSource = new MatTableDataSource<APIRecord>(this.apirecords);
  apiDetailsrecords: APIDetailsRecord[] = [];
  apiDetailsDataSource = new MatTableDataSource<APIDetailsRecord>(this.apiDetailsrecords);
  displayedNodeColumns: string[] = ['select', 'code', 'nodeName', 'nodeDesc', 'Actions'];
  displayedAPIColumns: string[] = ['select', 'nodeName', 'apiname', 'apidesc', 'httpMethodType', 'executionOrder','isActive', 'Actions'];
  displayedAPIDetailColumns: string[] = ['select', 'API', 'Parameter', 'DataType', 'From API', 'Actions'];
  displayedAPIParamsColumns: string[] = ['select',
    'apiName',
    'parameterDirection',
    'parameterName',
    'parameterType',
    
    'Actions'
  ];
  displayedMappingColumns: string[] = [ 'apiParameterName', 'parameterName', 'actions'];
  addParameterVisible = false;

  apiParameterMappingsDataSource!: MatTableDataSource<any>;
  apiList: ApiListModel[] = [];
  selectedNodeIds: number = 0;
  selectNodeName: string = '';
  selectApiName: string = '';
  fileDetails: { httpMethodType: string; binaryXml: string; xmlfileName: string } | null = null;
  selectedNodeId: number = 0;
  nodeForm!: FormGroup;
  nodeAPIForm!: FormGroup;
  apidetailsForm!: FormGroup;
  isDropdown: boolean = false;
  dataTypes: { dataTypeId: number; dataTypeName: string }[] = [];
  selectedXMLFile: any = null;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  pageSize: number = 5;
  currentPage: number = 0;
  paginatedRows: any[] = [];
  selection = new Set<any>();
  urlTypes = [
    { value: '', label: 'URL Type' },  // Default option
    { value: 'Soap', label: 'Soap' },
    { value: 'API', label: 'API' }
  ];

  parametersList: any = [];
  selectedRows: Set<number> = new Set();
  selectedRowsItem: Set<number> = new Set();

  selectedApiForMapping: any = null;
  apiParameters: any[] = [];
  internalParameters: any[] = [];
  selectedApiId: number = 0;
  selectedApiParameterId: number | null = null;
  selectedInternalParameterId: number | null = null;
  isLoadingTestApi = false;
  showMappingSection = false;
  apiParameterMappings: any[] = [];
isLoading: boolean = false;
isDownloading: boolean = false;
isUploading: boolean = false;
message: string = "Loading data, please wait...";

  loadApiParameterMappings() {
    this.isLoading = true;
    this.integrationService.getApiParameterMapping().subscribe({
      next: (res: any) => {
        this.apiParameterMappings = res.data || res; // store all mappings
        if (this.selectedApi) {
          this.applyApiFilter(); // filter if an API is selected
          
        }
            this.isLoading = false;

      },
      error: (err) => {
        this.isLoading = false;
        console.error('Error loading API parameter mappings', err)
      }
    });
  }
deleteMapping(id: number) {
  if (!confirm('Are you sure you want to delete this mapping?')) return;

  this.integrationService.deleteMapping(id).subscribe({
    next: (response: any) => {
      // Remove from local array
      this.apiParameterMappings = this.apiParameterMappings.filter(m => m.id !== id);
      this.apiParameterMappingsDataSource.data = this.apiParameterMappings;

      // Show confirmation
      this._snackBar.open(response.message || 'Mapping deleted successfully', 'Okay', {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000
      });
      this.isEditMode = false;
      //  Reset selected API to force clearing fields
      this.selectedApi = null;
      // Refresh API parameters & mappings (will clear them because selectedApi is null)
      this.onApiChange();
    },
    error: (err) => {
      console.error('Error deleting mapping:', err);
      this._snackBar.open('Failed to delete mapping.', 'Okay', {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000
      });
    }
  });
}

  selectedMappingId: number | null = null;

  saveMapping() {
    if (!this.selectedApiParameterId || !this.selectedInternalParameterId) {
      alert('Please select both API and internal parameter');
      return;
    }

    const mapping = {
      id: this.selectedMappingId??0, // important for update
      apiId: this.selectedApi, // include api id
      apiParameterId: this.selectedApiParameterId,
      parameterId: this.selectedInternalParameterId
    };

    if (this.isEditMode) {
      // 🔹 UPDATE existing mapping
      this.integrationService.updateApiParameterMapping(mapping).subscribe({
        next: (res) => {
          this._snackBar.open(res.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          }),
            this.resetMappingForm();
          this.onApiChange(); // reload table
        },
        error: (err) => alert('Error updating mapping: ' + err.message)
      });
    } else {
      // 🔹 ADD new mapping
      mapping.id = 0; // ensure id=0 for new
      this.integrationService.saveApiParameterMapping(mapping).subscribe({
        next: (res) => {
          this._snackBar.open(res.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          }),
          this.resetMappingForm();
          this.onApiChange();
        },
        error: (err) => alert('Error saving mapping: ' + err.message)
      });
    }
  }
  editMapping(mapping: any) {
    this.isEditMode = true;
    this.selectedMappingId = mapping.id;
    this.selectedApiParameterId = mapping.apiParameterId;
    this.selectedInternalParameterId = mapping.parameterId;
    this.selectedApi = Number(mapping.apiId); // ← Correct property
  }
  // Function to reset after save/update
  resetMappingForm() {
    this.selectedApiParameterId = null;
    this.selectedInternalParameterId = null;
    this.selectedMappingId = null;
    this.isEditMode = false;
  }

  //onApiChange() {
  //  if (!this.selectedApi) {
  //    this.apiParameters = [];
  //    return;
  //  }

  //  const apiId = this.selectedApi; // because you used [ngValue]="api.apiid" in dropdown

  //  this.integrationService.getAllApiParameters(apiId).subscribe({
  //    next: (res) => {
  //      this.apiParameters = res.data || res; // handle both {data:[]} or direct []
  //      console.log("Loaded API parameters:", this.apiParameters);
  //    },
  //    error: (err) => {
  //      console.error("Error loading API parameters:", err);
  //      this.apiParameters = [];
  //    }
  //  });
  //}
  filteredApiMappings: any[] = [];
  apiParameterMap: { [key: number]: string } = {};

  onApiChange() {
  if (!this.selectedApi) {
    this.apiParameters = [];
    this.apiParameterMappings = [];
    return;
  }

  // Load API parameters for the dropdown
  this.integrationService.getAllApiParameters(this.selectedApi).subscribe({
    next: (res) => {
      this.apiParameters = res.data || res; // store parameters
    },
    error: (err) => {
      console.error('Error loading API parameters:', err);
    }
  });

  // Load API parameter mappings for this API
  this.integrationService.getApiParamsByApiId(this.selectedApi).subscribe({
    next: (res) => {
      // Add apiId to each mapping so editMapping works correctly
      this.apiParameterMappings = (res.data || res).map((row: ApiParameterRecord) => ({
        ...row,
        apiId: this.selectedApi
      }));

      this.apiParameterMappingsDataSource = new MatTableDataSource(this.apiParameterMappings);

      setTimeout(() => {
        this.apiParameterMappingsDataSource.paginator = this.paginator;
        this.apiParameterMappingsDataSource.sort = this.sort;
      });
    },
    error: (err) => {
      console.error(err);
      this._snackBar.open(err.message, 'Okay', {
        horizontalPosition: 'right',
        verticalPosition: 'top', duration: 3000,
      }); this.selectedRows.clear();

    }
  });
}
  sanitizeCode(event: any) {
    event.target.value = this.utilityService.sanitizeCode(event.target.value);
  }
  onApiChangeForTest() {
    if (!this.selectedApi) {
      this.apiParameters = [];
      this.apiParameterMappings = [];
      return;
    }
    this.testQuery = this.selectedApi.requestParameters || '';
    this.testInput = this.selectedApi.requestBody || '';
    this.testOutput = this.selectedApi.responseFormate || '';


  }
  getApiParameterName(apiParameterId: number) {
    const param = this.apiParameters.find(p => p.apiParameterId === apiParameterId);
    return param ? param.parameterName : apiParameterId;
  }
  constructor(private integrationService: IntegrationService, private fb: FormBuilder, private PermissionsService: PermissionsService, private parameterService: ParameterService, private dialog: MatDialog, private factorsService: FactorsService, private http: HttpClient, private utilityService: UtilityService) {
    this.nodeForm = this.fb.group({
      code: ['', [Validators.required]],
      description: [''],
      nodeName: ['', [Validators.required]],
      url: ['', [Validators.required]],
      urlType: ['', [Validators.required]],
      username: [''],
      password: ['']
    });
    this.nodeAPIForm = this.fb.group({
      nodeId: ['', [Validators.required]],
      apiid: [[Validators.required]],
      apidesc: [''],
      apiName: ['', [Validators.required]],
      httpMethodType: ['', [Validators.required]],
      description: [''],
      header: [''],
      xmlFile: [null],
      requestParameters: [''],
      requestBody: [''],
      responseFormate: [''],
      isActive: [false],
      executionOrder: [null, [Validators.required]],  


    });
    this.apidetailsForm = this.fb.group({
      fromApiid: [null, [Validators.required]],
      callingParamName: ['', [Validators.required]],
      apiid: [null, [Validators.required]],
      sourceApiparam: ['', [Validators.required]],
      dataTypeId: [null, [Validators.required]],
    })
    this.apiParameterForm = this.fb.group({
      apiId: [''],
      apiParameterId: [0],
      parameterDirection: ['Input', Validators.required],
      parameterName: ['', Validators.required],
      parameterType: ['', Validators.required],
      isRequired: [false],
      defaultValue: ['']
    });

  }
  selectedApis: any = null;
  fullApiUrl: string = '';

  testQuery: string = '';
  testInput: string = '';
  testOutput: any;


  formatJson(value: any): string {
    try {
      // If it's already an object, format it
      if (typeof value === 'object') {
        return JSON.stringify(value, null, 2);
      }

      // If it's a string, try parsing it as JSON
      return JSON.stringify(JSON.parse(value), null, 2);
    } catch (e) {
      // If not JSON, just return as-is
      return String(value);
    }
  }
  onApiSelect() {
    if (!this.selectedApi) {
      this.fullApiUrl = '';
      return;
    }
    this.fullApiUrl = this.getFullApiUrl(this.selectedApi);
  }
  loadApiList() {
    this.integrationService.getAllApiList().subscribe({
      next: (data) => {

        //  Parse headers from backend
        this.apiList = data.data.map((api: any) => {
          try {
            api.header = api.header ? JSON.parse(api.header) : {};
          } catch {
            api.header = {};
          }
          return api;
        });

      },
      error: (err) => console.error(err)
    });
  }

  getCleanApiPath(api: any): string {
    if (!api.apiname) return '';
    return api.apiname.startsWith('/') ? api.apiname.substring(1) : api.apiname;
  }
  getFullApiUrl(api: any): string {
    if (!api) return '';

    let nodeUrl = api.nodeUrl || '';
    if (nodeUrl.endsWith('/')) nodeUrl = nodeUrl.slice(0, -1);

    let path = (api.apiname || '').trim();  // <- trim spaces
    if (path.startsWith('/')) path = path.substring(1);

    // remove duplicate 'api/' if already included in nodeUrl
    if (nodeUrl.endsWith('/api') && path.startsWith('api/')) {
      path = path.substring(4);
    }

    return `${nodeUrl}/${path}`;
  }
  filteredMappings: any[] = [];

  applyApiFilter() {
    if (!this.selectedApi) {
      this.filteredMappings = [];
      return;
    }

    // Filter mappings for selected API
    this.filteredMappings = this.apiParameterMappings
      .filter((map: { apiId: any; }) => map.apiId === this.selectedApi)
      .map((map: { apiParameterId: any; parameterId: any; }) => {
        const apiParam = this.apiParameters.find(p => p.apiParameterId === map.apiParameterId);
        const internalParam = this.parametersList.find((p: { parameterId: any; }) => p.parameterId === map.parameterId);

        return {
          ...map,
          apiParameterName: apiParam ? apiParam.parameterName : 'Unknown',
          internalParameterName: internalParam ? internalParam.parameterName : 'Unknown'
        };
      });
  }
  testApi() {
    if (!this.selectedApi) {
      alert('Please select an API first.');
      return;
    }

    this.isLoadingTestApi = true;

    const method = (this.selectedApi.httpMethodType || 'POST').toUpperCase();
    let queryParams: any = {};
    let bodyData: any = {};

    try {
      if (this.testQuery) queryParams = JSON.parse(this.testQuery);
      if (this.testInput) bodyData = JSON.parse(this.testInput);
    } catch (e) {
      alert('Invalid JSON format in input.');
      this.isLoadingTestApi = false;
      return;
    }


    const mergedPayload = { ...queryParams, ...bodyData };

    const backendRequest = {
      url: this.getFullApiUrl(this.selectedApi),
      httpMethod: method,
      payload: mergedPayload,
      headers: this.selectedApi.header || {}
    };


    this.integrationService.callApi(backendRequest).subscribe({
      next: (res: any) => {
        try {
          this.testOutput = JSON.stringify(res, null, 2);
        } catch {
          this.testOutput = res;
        }
        this.isLoadingTestApi = false;
      },
      error: (err: any) => {
        console.error(' Backend API Error:', err);
        this.testOutput = err.error || err.message || 'Unknown error';
        this.isLoadingTestApi = false;
      }
    });
  }
  
 // Dynamic API request preparation
  prepareApiRequest(bodyData: any, queryParams: any, method: string, url: string) {
    let finalPayload = { ...bodyData };
    let finalQueryParams = { ...queryParams };

    // Check if this is the BREIntegrationAlignment endpoint
    //const isBREIntegration = url.includes('breintegrationalignment');

    //if (isBREIntegration) {
    //  // For BREIntegrationAlignment, ApiId should be in query params, not body
    //  finalQueryParams.ApiId = this.selectedApi.apiid;

    //  // Handle EntityId - can be in query params or body
    //  if (bodyData.EntityId && !finalQueryParams.EntityId) {
    //    finalQueryParams.EntityId = bodyData.EntityId;
    //    // Remove EntityId from body to avoid duplication
    //    const { EntityId, ...cleanBody } = finalPayload;
    //    finalPayload = cleanBody;
    //  }

    //  // Ensure EntityId exists (set default if not provided)
    //  if (!finalQueryParams.EntityId) {
    //    finalQueryParams.EntityId = 0; // Default value
    //  }
    //} else {
    {
      // For other APIs, include apiId in the body (your existing logic)
      finalPayload = {
        ...bodyData,
        apiId: this.selectedApi.apiid
      };
    }

    // For GET requests, move all data to query params
    if (method === 'GET') {
      finalQueryParams = { ...finalQueryParams, ...finalPayload };
      finalPayload = {};
    }

    //console.log('API Request Prepared:', {
    //  url: url,
    //  method: method,
    //  isBREIntegration: isBREIntegration,
    //  payload: finalPayload,
    //  queryParams: finalQueryParams
    //});

    return { finalPayload, finalQueryParams };
  }

  getAddPermission(): boolean {
    return (this.activeTab === 'nodes' && this.hasPermission('Permissions.Node.Create')) ||
      (this.activeTab === 'api' && this.hasPermission('Permissions.NodeApi.Create')) ||
      (this.activeTab === 'apiDetails' && this.hasPermission('Permissions.ApiDetails.Create'))||
      (this.activeTab === 'addParameters' && this.hasPermission('Permissions.ApiParameters.Create')) || (this.activeTab === 'mapApiParams' && this.hasPermission('Permissions.ApiParameterMaps.Create'));
  }

  getDeletePermission(): boolean {
    return (this.activeTab === 'nodes' && this.hasPermission('Permissions.Node.Delete')) ||
      (this.activeTab === 'api' && this.hasPermission('Permissions.NodeApi.Delete')) ||
      (this.activeTab === 'apiDetails' && this.hasPermission('Permissions.ApiDetails.Delete'));
  }
  openApiDetails(api: any) {
    this.NodeAPIVisible = false;
    this.fetchParameterListOnProduct();
    this.selectedApi = api.apiid;
    this.activeTab = 'addParameters'; // Switch to API Details tab
    this.showApiParameterSection = true;

    // Optional: Fetch existing parameters from backend if available
    //  this.loadApiParameters(api.apiid);
  }
  CancelAddParameter() {
    this.fetchParameterListOnProduct();
    this.activeTab = 'addParameters'; // Switch to API Details tab
    this.showApiParameterSection = false;

    // Optional: Fetch existing parameters from backend if available
    //  this.loadApiParameters(api.apiid);
  }


  //loadApiParameters(apiId: number) {
  //  // Example API call (you can replace with real service)
  //  this.integrationService.p(apiId).subscribe((res: any[]) => {
  //    this.apiParameterRecords = res;
  //    this.apiParameterDataSource.data = [...this.apiParameterRecords];
  //  });
  //}
  onDeleteApiParameter(row: any) {
    if (confirm(`Are you sure you want to delete parameter: ${row.parameterName}?`)) {
      this.integrationService.deleteApiParameter(row.apiParameterId).subscribe({
        next: (res) => {
          if (res.isSuccess) {
            this._snackBar.open(res.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000,
            });
            this.loadApiParameters(); // refresh table after delete
            this.selectedRows.clear();

          }
        },
        error: (err) => {
          console.error('Error deleting API parameter:', err);
          this._snackBar.open(err.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000,
          });          this.selectedRows.clear();

        }
      });
    }
  }
  editApiParameter(row: ApiParameterRecord) {
    this.showApiParameterSection = true; // show the Add/Edit form
    this.isEditMode = true; // mark as edit
    this.addParameterVisible = true;
    // Patch values according to your form controls
    this.apiParameterForm.patchValue({
      apiId: row.apiId,
      apiParameterId: row.apiParameterId,
      parameterDirection: row.parameterDirection,
      parameterName: row.parameterName,
      parameterType: row.parameterType,
      isRequired: row.isRequired,
      defaultValue: row.defaultValue
    });
  }
  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }
  addApiParameter() {
    if (!this.apiParameterForm.value.apiId && this.selectedApi?.apiid) {
      this.apiParameterForm.patchValue({ apiId: this.selectedApi.apiid });
    }
    if (this.apiParameterForm.invalid) {
      this._snackBar.open('Please fill all required fields', 'Okay', {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000,
      });
      return;
    }

    const formValues = this.apiParameterForm.value;

    // Map frontend apiParameterId to backend ApiParamterId
    const paramPayload = {
      ApiParamterId: formValues.apiParameterId, // map to backend typo
      ApiId: formValues.apiId,
      ParameterDirection: formValues.parameterDirection,
      ParameterName: formValues.parameterName,
      ParameterType: formValues.parameterType,
      IsRequired: formValues.isRequired,
      DefaultValue: formValues.defaultValue
    };

    if (this.isEditMode) {
      // Update existing parameter
      this.integrationService.updateApiParameter(paramPayload).subscribe({
        next: (res: any) => {
          if (res.isSuccess) {
            // Update local data source
            const index = this.apiParameterRecords.findIndex(
              p => p.apiParameterId === formValues.apiParameterId
            );
            if (index !== -1) {
              this.apiParameterRecords[index] = {
                ...formValues // keep frontend property names
              };
            }
            this.apiParameterDataSource.data = [...this.apiParameterRecords];
            this.closeForm();

            this._snackBar.open('Parameter updated successfully', 'Okay', { duration: 3000 });
            this.CancelAddParameter();
            this.loadApiParameters();  // refresh table
            this.closeForm();
          } else {
            this._snackBar.open(res.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000 });
          }
        },
        error: (err) => {
          console.error(err);
          this._snackBar.open(err.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000 });

        }
      });
    } else {
      // Add new parameter
      paramPayload.ApiParamterId = 0

      this.integrationService.addAPIParams(paramPayload).subscribe({
        next: (res: any) => {
          if (res.isSuccess) {
            // Map backend ApiParamterId to frontend apiParameterId
            this.apiParameterRecords.push({
              ...formValues,
              apiParameterId: res.data.apiParamterId
            });
            this.apiParameterDataSource.data = [...this.apiParameterRecords];

            this._snackBar.open('Parameter added successfully', 'Okay', { duration: 3000 });
            this.loadApiParameters();
            this.closeForm();
// refresh table
            this.CancelAddParameter();
          } else {
            this._snackBar.open(res.message, 'Okay', { duration: 3000 });
          }
        },
        error: (err) => {
          console.error(err);
          this._snackBar.open(err.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });        }
      });
    }
  }



  deleteApiParameter(row: ApiParameterRecord) {
    this.apiParameterRecords = this.apiParameterRecords.filter(p => p !== row);
    this.apiParameterDataSource.data = [...this.apiParameterRecords];
  }
  selectedApiParameter: string = "";
  originalApiParameterRecords: any[] = [];
  loadApiParameters() {
        this.isLoading = true;

    this.integrationService.getApiParameters().subscribe({
      next: (response) => {
        this.originalApiParameterRecords = response.data.map((p: any) => ({
          ...p,
          apiParameterId: p.apiParamterId,
          apiName: this.apiList.find(a => a.apiid === p.apiId)?.apiname || ''
        }));

        this.apiParameterRecords = [...this.originalApiParameterRecords];
        this.apiParameterDataSource.data = this.apiParameterRecords;
        this.apiParameterDataSource.paginator = this.paginator;
        this.apiParameterDataSource.sort = this.sort;
              this.isLoading = false;

      },
      error: (err) => {
        this.isLoading = false;
        console.error(err);
      }
    });
  }
  filterByApi() {
    if (!this.selectedApiParameter) {
      // If user selects "All APIs"
      this.apiParameterDataSource.data = [...this.originalApiParameterRecords];
      return;
    }

    this.apiParameterDataSource.data = this.originalApiParameterRecords.filter(
      x => x.apiName.toLowerCase() === this.selectedApiParameter.toLowerCase()
    );
  }
  ngOnInit(): void {
    this.fetchNodeList();
    this.fetchNodeAPIList();
    this.fetchAPIDetailsList();
    this.fetchDataTypesList();
    this.fetchParameterListOnProduct();
    this.loadApiList();
    this.loadApiParameters();

  }


  private initializeTableControls(): void {
    // Node table
    if (this.nodePaginator && this.nodeSort) {
      this.nodeDataSource.paginator = this.nodePaginator;
      this.nodeDataSource.sort = this.nodeSort;
    }

    // Node API table
    if (this.apiPaginator && this.apiSort) {
      this.nodeAPIDataSource.paginator = this.apiPaginator;
      this.nodeAPIDataSource.sort = this.apiSort;
    }

    // API Details table
    if (this.apiDetailsPaginator && this.apiDetailsSort) {
      this.apiDetailsDataSource.paginator = this.apiDetailsPaginator;
      this.apiDetailsDataSource.sort = this.apiDetailsSort;
    }

    // API Parameters table
    if (this.apiParamsPaginator && this.apiParamsSort) {
      this.apiParameterDataSource.paginator = this.apiParamsPaginator;
      this.apiParameterDataSource.sort = this.apiParamsSort;
    }
  }
  fetchNodeList() {
        this.isLoading = true;

    this.integrationService.getNodeList().subscribe({
      next: (response) => {
        this.nodeDataSource.data = response.data.map((item: any) => ({
          nodeId: item.nodeId,
          nodeName: item.nodeName,
          code: item.code,
          nodeDesc: item.nodeDesc,
          nodeUrl: item.nodeUrl,
          urlType: item.urlType,
          apiuserName: item.apiuserName,
          apipassword: item.apipassword,
          entityId: item.entityId
        }));
        this.noderecords = response.data;

        // Reconnect controls after data update
        setTimeout(() => {
          this.nodeDataSource.paginator = this.nodePaginator;
          this.nodeDataSource.sort = this.nodeSort;
        });
      this.isLoading = false;

      },
      error: (error) => {
        console.error('Error fetching node list:', error);
        this.nodeDataSource.data = [];
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 3000,
        });
      this.isLoading = false;
      },
    });
  }

  fetchNodeAPIList() {
        this.isLoading = true;
   this.integrationService.getNodeAPIList().subscribe({
      next: (response) => {
        this.nodeAPIDataSource.data = response.data.map((item: any) => {
          const matchedNode = this.noderecords.find(node => node.nodeId === item.nodeId);
          return {
            apiid: item.apiid,
            apiname: item.apiname,
            apidesc: item.apidesc,
            nodeId: item.nodeId,
            nodeName: matchedNode ? matchedNode.nodeName : '',
            httpMethodType: item.httpMethodType,
            binaryXml: item.binaryXml,
            xmlfileName: item.xmlfileName,
            header: item.header,
            requestParameters: item.requestParameters,
            requestBody: item.requestBody,
            responseFormate: item.responseFormate,
            isActive: item.isActive,
            executionOrder: item.executionOrder
          };
        });
        this.apirecords = this.nodeAPIDataSource.data;

        // Reconnect controls after data update
        setTimeout(() => {
          this.nodeAPIDataSource.paginator = this.apiPaginator;
          this.nodeAPIDataSource.sort = this.apiSort;
        });
      this.isLoading = false;
      },
      error: (error) => {
        console.error('Error fetching node API list:', error);
        this.nodeAPIDataSource.data = [];
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 3000,
        });
      this.isLoading = false;
      },
    });
  }


  fetchAPIDetailsList() {
    this.integrationService.getAPIDetailsList().subscribe({
      next: (response) => {
        this.apiDetailsDataSource.data = response.data.map((item: any) => {
          const matchedNode = this.nodeAPIDataSource.data.find(node => node.apiid === item.apiid);
          const machedAPI = this.nodeAPIDataSource.data.find(node => node.apiid === item.fromApiid);
          const matchedDataType = this.dataTypes.find(datatype => datatype.dataTypeId === item.dataTypeId);
          return {
            apidetailsId: item.apidetailsId,
            fromApiid: item.fromApiid,
            fromAPIName: machedAPI ? machedAPI?.apiname : '',
            callingParamName: item.callingParamName,
            apiid: item.apiid,
            apiname: matchedNode ? matchedNode.apiname : '',
            sourceApiparam: item.sourceApiparam,
            binaryXml: item.binaryXml,
            dataTypeId: item.dataTypeId,
            dataTypeName: matchedDataType ? matchedDataType?.dataTypeName : ''
          }
        });
        this.apiDetailsrecords = response.data;
        this.apiDetailsDataSource.paginator = this.paginator;
        this.apiDetailsDataSource.sort = this.sort;
      },
      error: (error) => {
        setTimeout(() => {
          this.apiDetailsDataSource.data = [];
          this.apiDetailsDataSource.paginator = this.paginator;
          this.apiDetailsDataSource.sort = this.sort;
        }, 100);
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      },
    });
  }

  fetchParameterListOnProduct() {
    this.factorsService.getParametersList().subscribe({
      next: (response) => {
        if (response.data && response.data.length > 0) {
          this.parametersList = response.data
        }
      },
      error: (error) => {
        this._snackBar.open(error, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      }
    })
  }

  toggleRowSelection(row: any): void {
    if (this.selection.has(row)) {
      this.selection.delete(row);
    } else {
      this.selection.add(row);
    }
  }

  toggleAllSelection(checked: boolean): void {
    if (checked) {
      this.selection = new Set(this.nodeDataSource.data);
    } else {
      this.selection.clear();
    }
  }

  isAllSelected(): boolean {
    return this.selection.size === this.nodeDataSource.data.length;
  }

  isRowSelected(row: any): boolean {
    return this.selection.has(row);
  }

  async fetchDataTypesList() {
    this.parameterService.getDataTypes().subscribe({
      next: (data) => {
        this.dataTypes = data;
      },
      error: (error) => {
        console.error('Error fetching data types:', error);
      },
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value.trim().toLowerCase();
    if (this.activeTab === 'nodes') {
      this.nodeDataSource.filter = filterValue;
      this.nodeDataSource.paginator = this.paginator;
      this.nodeDataSource.sort = this.sort;
    } else if (this.activeTab === 'api') {
      this.nodeAPIDataSource.filter = filterValue;
      this.nodeAPIDataSource.paginator = this.paginator;
      this.nodeAPIDataSource.sort = this.sort;
    } else if (this.activeTab === 'apiDetails') {
      this.apiDetailsDataSource.filter = filterValue;
      this.apiDetailsDataSource.paginator = this.paginator;
      this.apiDetailsDataSource.sort = this.sort;
    }
    else if (this.activeTab === 'addParameters') {
      this.apiParameterDataSource.filter = filterValue;
      this.apiParameterDataSource.paginator = this.paginator;
      this.apiParameterDataSource.sort = this.sort;
    }

  }

  addRecord() {
    if (this.activeTab === 'nodes') {
      if (this.isEditMode) {

        this.integrationService.updateNode(this.nodeformData).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.fetchNodeList();
              this.closeForm();
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
          error: (error) => console.error('Error updating group:', error),
        });
      } else {
        const requestBody = {
          nodeId: this.nodeformData.nodeId,
          nodeName: this.nodeformData.nodeName,
          code: this.nodeformData.code,
          nodeDesc: this.nodeformData.nodeDesc,
          nodeUrl: this.nodeformData.nodeUrl,
          urlType: this.nodeformData.urlType,
          apiuserName: this.nodeformData.apiuserName,
          apipassword: this.nodeformData.apipassword,
          entityId: this.nodeformData.entityId
        };
        this.integrationService.addNode(requestBody).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.fetchNodeList();
              this.closeForm();
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
          error: (error) => console.error('Error adding group:', error),
        });
      }
    } else if (this.activeTab === 'api') {
      if (this.isEditMode) {

        var apid = this.nodeAPIForm.value.apiid;
        const requestBody = {
          apiid: this.nodeAPIForm.value.apiid,
          apiName: this.nodeAPIForm.value.apiName,
          apidesc: this.nodeAPIForm.value.apidesc,
          nodeId: this.nodeAPIForm.value.nodeId,
          httpMethodType: this.nodeAPIForm.value.httpMethodType,
          binaryXml: this.nodeAPIformData.binaryXml,
          xmlfileName: this.nodeAPIformData.xmlfileName,
          header: this.nodeAPIForm.value.header,
          requestBody: this.nodeAPIForm.value.requestBody??"",
          requestParameters: this.nodeAPIForm.value.requestParameters,
          responseFormate: this.nodeAPIForm.value.responseFormate,
          isActive: this.nodeAPIForm.get('isActive')?.value === true,
          executionOrder: this.nodeAPIForm.value.executionOrder
        };
        this.integrationService.updateNodeAPI(requestBody).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.fetchNodeAPIList();
              this.closeForm();
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
          error: (error) => console.error('Error updating group:', error),
        });
      } else {
        if (this.nodeAPIForm.valid) {
          const requestBody = {
            apiid: 0,
            apiName: this.nodeAPIForm.value.apiName ? this.nodeAPIForm.value.apiName : this.selectApiName,
            apidesc: this.nodeAPIForm.value.apidesc,
            nodeId: this.nodeAPIForm.value.nodeId,
            httpMethodType: this.nodeAPIForm.value.httpMethodType,
            binaryXml: this.nodeAPIformData.binaryXml,
            xmlfileName: this.nodeAPIformData.xmlfileName,
            header: this.nodeAPIForm.value.header,
            isActive: this.nodeAPIForm.value.isActive,
            requestBody: this.nodeAPIForm.value.requestBody??"",
            executionOrder: this.nodeAPIForm.value.executionOrder,
            requestParameters: this.nodeAPIForm.value.requestParameters??"",
            responseFormate: this.nodeAPIForm.value.responseFormate??"",

          };

          this.integrationService.addNodeAPI(requestBody).subscribe({
            next: (response) => {
              if (response.isSuccess) {
                this.fetchNodeAPIList();
                this.closeForm();
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
            error: (error) => console.error('Error adding group:', error),
          });
        }
      }
    } else if (this.activeTab === 'apiDetails') {
      if (this.isEditMode) {
        this.integrationService.updateNodeAPIDetails(this.nodeAPIDetailsformData).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.fetchAPIDetailsList();
              this.closeForm();
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
          error: (error) => console.error('Error updating group:', error),
        });
      } else {
        const requestBody =
        {
          "apidetailsId": this.nodeAPIDetailsformData.apidetailsId,
          "fromApiid": this.nodeAPIDetailsformData.fromApiid,
          "callingParamName": this.nodeAPIDetailsformData.callingParamName,
          "apiid": this.nodeAPIDetailsformData.apiid,
          "sourceApiparam": this.nodeAPIDetailsformData.sourceApiparam,
          "dataTypeId": this.nodeAPIDetailsformData.dataTypeId
        }
        this.integrationService.addNodeAPIDetails(requestBody).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.fetchAPIDetailsList();
              this.closeForm();
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
          error: (error) => console.error('Error adding group:', error),
        });
      }
    }
  }

  closeForm() {
    if (this.activeTab === 'nodes') {
      this.formVisible = false;
      this.nodeformData = { nodeId: 0, nodeName: '', code: '', nodeDesc: '', nodeUrl: '', urlType: '', apiuserName: '', apipassword: '', entityId: 0 };
    } else if (this.activeTab === 'api') {
      this.NodeAPIVisible = false;
      this.nodeAPIformData = {
        apiid: 0, apiname: '', apidesc: '', nodeId: 0, httpMethodType: '', binaryXml: '', xmlfileName: '', header: '', requestBody: '', requestParameters: '', responseFormate: '', isActive: false, executionOrder:null
      };
    } else if (this.activeTab === 'apiDetails') {
      this.NodeAPIDetailsVisible = false;
      this.nodeAPIDetailsformData = { apidetailsId: 0, fromApiid: 0, callingParamName: '', apiid: 0, sourceApiparam: '', dataTypeId: 0 };
    }
    else if (this.activeTab === 'addParameters') {
      this.showApiParameterSection = false;
    this. addParameterVisible = false;
      this.loadApiParameters();
      // Reset API Parameter form
      this.apiParameterForm.reset({
        apiId: null,
        apiParameterId: null,
        parameterDirection: 'Input',
        parameterName: '',
        parameterType: '',
        isRequired: false,
        defaultValue: ''
      });
      this.searchGroup = '';
      this.isDropdown = false;
      this.isEditMode = false;
    }
    else if (this.activeTab === 'mapApiParams') {
      // Hide mapping section
      
      this.addParameterVisible = false;

      // Reset selected mapping
      this.selectedMappingId = null;
      this.selectedApi = null;
      this.selectedApiParameterId = null;
      this.selectedInternalParameterId = null;
      this.isEditMode = false;

      // Optionally reset the data source
    //  this.apiParameterMappingsDataSource = new MatTableDataSource([]);
    }
  }


  getCurrentPageData(): any[] {
    if (!this.paginator) return []; // Return empty array if paginator is undefined
    const startIndex = this.paginator.pageIndex * this.paginator.pageSize;
    const endIndex = startIndex + this.paginator.pageSize;
    if (this.activeTab === 'nodes') {
      return this.nodeDataSource.filteredData.slice(startIndex, endIndex);
    } else if(this.activeTab === 'api') {
      return this.nodeAPIDataSource.filteredData.slice(startIndex, endIndex);
    } else if (this.activeTab === 'addParameters') {
      return this.apiParameterDataSource.filteredData.slice(startIndex, endIndex);
    } else{
      return this.apiDetailsDataSource.filteredData.slice(startIndex, endIndex);
    }
  }

  isAllPageSelected(): boolean {
    const currentPageData = this.getCurrentPageData();
    if (this.activeTab === 'nodes') {
      return currentPageData.every((row: any) => this.selectedRows.has(row.nodeId));
    } else if(this.activeTab === 'api') {
      return currentPageData.every((row: any) => this.selectedRows.has(row.apiid));
    } else if (this.activeTab === 'addParameters') {
      return currentPageData.every((row: any) => this.selectedRows.has(row.apiParameterId));
    } else{
      return currentPageData.every((row: any) => this.selectedRows.has(row.apidetailsId));
    }
  }

  isSomePageSelected(): boolean {
    const currentPageData = this.getCurrentPageData();
    if (this.activeTab === 'nodes') {
      return currentPageData.some((row: any) => this.selectedRows.has(row.nodeId)) && !this.isAllPageSelected();
    } else if(this.activeTab === 'api') {
      return currentPageData.some((row: any) => this.selectedRows.has(row.apiid)) && !this.isAllPageSelected();
    } else if (this.activeTab === 'addParameters') {
      return currentPageData.some((row: any) => this.selectedRows.has(row.apiParameterId)) && !this.isAllPageSelected();
    } else{
      return currentPageData.some((row: any) => this.selectedRows.has(row.apidetailsId)) && !this.isAllPageSelected();
    }
  }

  toggleSelection(event: MatCheckboxChange, ids: number) {
    if (this.activeTab === 'nodes') {
      if (event.checked) {
        this.selectedRows.add(ids);
      } else {
        this.selectedRows.delete(ids);
      }
    } else if (this.activeTab === 'api') {
      if (event.checked) {
        this.selectedRows.add(ids);
      } else {
        this.selectedRows.delete(ids);
      }
    }
    else if (this.activeTab === 'addParameters') {
      if (event.checked) {
        this.selectedRows.add(ids);
      } else {
        this.selectedRows.delete(ids);
      }
    }
    else {
      if (event.checked) {
        this.selectedRows.add(ids);
      } else {
        this.selectedRows.delete(ids);
      }
    }
  }

  toggleSelectAll(event: MatCheckboxChange) {
    const currentPageData = this.getCurrentPageData(); // Paginated rows
    if (this.activeTab === 'nodes') {
      if (event.checked) {
        currentPageData.forEach((row: any) => this.selectedRows.add(row.nodeId));
      } else {
        currentPageData.forEach((row: any) => this.selectedRows.delete(row.nodeId));
      }
    } else if (this.activeTab === 'api') {
      if (event.checked) {
        currentPageData.forEach((row: any) => this.selectedRows.add(row.apiid));
      } else {
        currentPageData.forEach((row: any) => this.selectedRows.delete(row.apiid));
      }
    }
    else if (this.activeTab === 'addParameters') {
      if (event.checked) {
        currentPageData.forEach((row: any) => this.selectedRows.add(row.apiParameterId));
      } else {
        currentPageData.forEach((row: any) => this.selectedRows.delete(row.apiParameterId));
      }
    }
     else {
      if (event.checked) {
        currentPageData.forEach((row: any) => this.selectedRows.add(row.apidetailsId));
      } else {
        currentPageData.forEach((row: any) => this.selectedRows.delete(row.apidetailsId));
      }
    }
  }

  onSelect(data: any): void {
    var value = data.target.value;
    this.selectedNodeIds = value;
  }



  onApiNameChange(event: any) {
    const apiName = event.target.value;
    this.nodeAPIForm.patchValue({ apiName });
  }

  onNodeSelect(data: any) : void {
    const value = data.target.value;
    this.selectedNodeIds = value;
    const selectedNode = this.noderecords.find(node => node.nodeId == this.selectedNodeIds)
    if(selectedNode && selectedNode.urlType == "API") {
      this.isDropdown = false;
      this.nodeAPIformData.apiname = '';
      this.nodeAPIForm.controls['apiName'].setValidators([Validators.required]);
      this.nodeAPIForm.controls['apiid'].clearValidators();
    }
    else if(selectedNode && selectedNode.urlType == "Soap") {
      this.fetchSoapAPIList(selectedNode!.nodeId);
      this.isDropdown = true;
      this.nodeAPIformData.apiname = null;
      this.nodeAPIForm.controls['apiName'].setValidators([Validators.required]);
      // this.nodeAPIForm.controls['apiid'].setValidators([Validators.required]);
      // this.nodeAPIForm.controls['apiName'].clearValidators();
    }else{
      this.isDropdown = false;
    }
    this.nodeAPIForm.controls['apiName'].updateValueAndValidity();
    this.nodeAPIForm.controls['apiid'].updateValueAndValidity();
    if(this.activeTab === 'api'){
    // this.nodeAPIs = [{
    //   apiId:12,
    //   apiName: "abc"
    // },
    // {
    //   apiId:13,
    //   apiName: "xyz"
    // }]
  }
  }

  fetchSoapAPIList(nodeId: number | null): void {
    this.integrationService.getSoapApi(nodeId).subscribe({
      next: (response) => {
        this.nodeAPIs = response.data;
        if (this.isEditMode && this.nodeAPIformData.apiname) {
          const matchingApi = this.nodeAPIformData.apiname = this.nodeAPIs.find(api => api.name === this.nodeAPIformData.apiname);
          if (matchingApi) {
            this.nodeAPIformData.apiname = matchingApi.name;
          }else{
            this.nodeAPIformData.apiname = '';
          }
        }
      },
      error: (error) => console.error('Error fetching Soap API list:', error),
    });
  }

  onSampleInputClick(): void {
    const fileInput: HTMLInputElement | null = document.querySelector('input[type="file"]');
    if (fileInput) {
      fileInput.click();
    }
  }

  openFileInput() : void {
    const fileInput: HTMLInputElement | null = document.querySelector('input[type="file"]');
    if (fileInput) {
      fileInput.click();
    }
  } 
  
  @HostListener('document:click', ['$event.target'!])
  onClickOutside(targetElement: HTMLElement) {
    const dropdown = document.querySelector('.dropdown-toggle');
    const menu = document.querySelector('.dropdown-menu');

    if (!dropdown?.contains(targetElement) && !menu?.contains(targetElement)) {
      this.closeMenu();
    }
  }
    
  toggleMenu() {
    this.menuVisible = !this.menuVisible;
  }

  closeMenu() {
    this.menuVisible = false;
  }
  openApiParameterForm() {
    this.isEditMode = false;         // always add mode
    this.showApiParameterSection = true;
    this.apiParameterForm.reset();   // reset form values
  }

  closeApiParameterForm() {
    this.isEditMode = false;
    this.showApiParameterSection = false;
    this.apiParameterForm.reset();
  }
  openForm() {

    if (this.activeTab === 'nodes') {
      this.formVisible = true;
      this.isEditMode = false;
      // this.nodeformData = { nodeId: 0, nodeName: '', code: '', nodeDesc: '', nodeUrl: '', urlType: '', apiuserName: '', apipassword: '', entityId: 0 };
      this.nodeForm.reset();
    } else if (this.activeTab === 'api') {
      this.showApiParameterSection = false;
      this.NodeAPIVisible = true;
      this.isEditMode = false;
      this.selectedXMLFile = null;
      this.nodeAPIformData = {
        apiid: null,
        apiname: '',
        apidesc: '',
        nodeId: null,
        httpMethodType: '',
        binaryXml: '',
        xmlfileName: '',
        header: ''
        , requestBody: '', requestParameters: '', responseFormate: '', isActive: false, executionOrder: null
      };
      // this.nodeAPIformData = { apiid: 0, apiname: '', apidesc: '', nodeId: 0, httpMethodType: '', binaryXml: '', xmlfileName: '', header: '' };
      this.nodeAPIForm.reset();
    } else if (this.activeTab === 'apiDetails') {
      this.NodeAPIDetailsVisible = true;
      this.isEditMode = false;
      this.nodeAPIDetailsformData = { apidetailsId: 0, fromApiid: null, callingParamName: '', apiid: null, sourceApiparam: '', dataTypeId: null };
      this.apidetailsForm.reset();
    } else if (this.activeTab === 'addParameters') {
      this.isEditMode = false;

      this.addParameterVisible = true;
      if (!this.isEditMode) {
        this.apiParameterForm.patchValue({ apiId: this.selectedApi?.apiId });
      }
      this.apiParameterForm.reset({
        apiId: null,
        apiParameterId: null,
        parameterDirection: '',
        parameterName: '',
        parameterType: '',
        isRequired: false,
        defaultValue: ''
      });
    
    
    } else if (this.activeTab === 'mapApiParams') { 
      // Show mapping section
      this.showMappingSection = true;
    this.isEditMode = false;

    // Reset mapping form or related state
    this.selectedApiParameterId = null;
    this.selectedInternalParameterId = null;
    this.loadApiParameterMappings();
  } 
  
    else {

      this.addParameterVisible = false;
      this.isEditMode = false;

    }

  
  } 

  deleteMulSelectedRows(): void {
    if (this.selectedRows.size === 0) {
      alert('Please select at least one row to delete');
      return;
    }

    const dialogRef = this.dialog.open(DeleteDialogComponent);

    dialogRef.afterClosed().subscribe(result => {
      if (result?.delete) {
        if (this.activeTab === 'nodes') {
          this.performMultipleDeleteNode();
        } else if (this.activeTab === 'api') {
          this.performMultipleDeleteAPI();
        }
        else if (this.activeTab === 'addParameters') {
          this.performMultipleDeleteApiParam();
        } else {
          this.performMultipleDeleteAPIDetails();
        }
      }
    });
  }


  performMultipleDeleteNode(): void {
    const selectedIds = Array.from(this.selectedRows);

    this.integrationService.deleteMultipleNodes(selectedIds).subscribe({
      next: (response) => {
        this.fetchNodeList();
        this.selectedRows.clear();
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      },
    });
  }
  performMultipleDeleteApiParam(): void {
    const selectedIds = Array.from(this.selectedRows);
    this.integrationService.deleteMultipleApiParams(selectedIds).subscribe({
      next: (response) => {
        this.loadApiParameters();
        this.selectedRows.clear();
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      },
    });
  }

  performMultipleDeleteAPI(): void {
    const selectedIds = Array.from(this.selectedRows);
    this.integrationService.deleteMultipleAPI(selectedIds).subscribe({
      next: (response) => {
        this.fetchNodeAPIList();
        this.selectedRows.clear();
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      },
    });
  }

  performMultipleDeleteAPIDetails(): void {
    const selectedIds = Array.from(this.selectedRows);
    this.integrationService.deleteMultipleDetails(selectedIds).subscribe({
      next: (response) => {
        this.fetchAPIDetailsList();
        this.selectedRows.clear();
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      },
    });
  }

  showMappingForm: boolean = false;
  currentMappingId: number | null = null;

  switchTab(tab: string): void {
    // Reset all visibility flags
    this.formVisible = false;
    this.NodeAPIVisible = false;
    this.NodeAPIDetailsVisible = false;
    this.showApiParameterSection = false;
    this.showMappingSection = false;
    this.showMappingForm = false;  // Reset mapping form visibility
    this.addParameterVisible = false;
    this.selectedApiParameter = 'All APIs';
    this.isEditMode = false;
    this.currentMappingId = null;

    this.selectedRows.clear();
    this.activeTab = tab;

    // Tab-specific logic
    if (this.activeTab === 'nodes') {
      this.fetchNodeList();
    } else if (this.activeTab === 'api') {
      this.fetchNodeAPIList();
    } else if (this.activeTab === 'apiDetails') {
      this.fetchAPIDetailsList();
    } else if (tab === 'mapApiParams') {
      this.showMappingSection = true;  // Show the mapping tab content
      this.showMappingForm = false;    // But hide the form initially
      this.loadApiParameterMappings();
      this.apiParameterForm.reset();
      this.selectedApi=null;
      this.resetMappingForm();
    } else if (tab === 'testApi') {
      this.testQuery = '';
      this.testInput = this.selectedApi;
      this.testOutput = '';
      this.loadApiList();
    } else if (tab === 'addParameters') {
      this.showApiParameterSection = true;
      this.loadApiParameters();
      this.loadApiList();
    }

    this.searchTerm = this.searchTerms[this.activeTab] || '';
  }

  editRecord(record: NodeRecord) {
    this.isEditMode = true;
    this.formVisible = true;
    const validUrlType = this.urlTypes.some(type => type.value === record.urlType) ? record.urlType : '';
    this.nodeformData = {
      ...record,
      urlType: validUrlType
    };
  }

  editNodeAPIRecord(record: APIRecord) {
    this.isEditMode = true;
    this.NodeAPIVisible = true;
    this.selectedNodeId = record.nodeId!;

    const selectedNode = this.noderecords.find(x => x.nodeId == record.nodeId);
    if (selectedNode?.urlType === 'API') {
      this.isDropdown = false;
    } else if (selectedNode?.urlType === 'Soap') {
      this.isDropdown = true;
      this.fetchSoapAPIList(selectedNode.nodeId);
    }

    this.nodeAPIformData = { ...record }; // keep a copy if needed
    this.nodeAPIForm.patchValue({
      nodeId: record.nodeId,
      apiid: record.apiid,
      apiName: record.apiname,
      httpMethodType: record.httpMethodType,
      apidesc: record.apidesc,
      header: record.header,
      xmlfileName: record.xmlfileName,
      requestParameters: record.requestParameters || '',
      requestBody: record.requestBody || '',
      responseFormate: record.responseFormate || '',
      isActive: record.isActive || false,
      executionOrder: record.executionOrder || null
    });

    this.selectedXMLFile = record.binaryXml;
  }

  editAPIDetailsRecord(record: APIDetailsRecord) {
    this.isEditMode = true;
    this.NodeAPIDetailsVisible = true;
    this.nodeAPIDetailsformData = { ...record };
    this.apidetailsForm.patchValue(this.nodeAPIDetailsformData);
  }

  deleteNode(record: NodeRecord) {
    if (confirm(`Are you sure you want to delete Node: ${record.nodeName}?`)) {
      this.integrationService.deleteNode(Number(record.nodeId)).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.fetchNodeList();
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
          this.closeForm()
          this.selectedRows.clear();

        },
        error: (error) => {
          this._snackBar.open(error.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000,
          });
          this.selectedRows.clear();

        },
      });
    }
  }
  
  deleteNodeAPI(record: APIRecord) {
    if (confirm(`Are you sure you want to delete Node API: ${record.apiname}?`)) {
      this.integrationService.deleteNodeAPI(Number(record.apiid)).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.fetchNodeAPIList();
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
          this.closeForm()
          this.selectedRows.clear();

        },
        error: (error) => {
          this._snackBar.open(error.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000,
          });
          this.selectedRows.clear();

        },
      });
    }
  }

  deleteNodeDetails(record: APIDetailsRecord) {
    if (confirm(`Are you sure you want to delete Node Api Details: "${record.callingParamName}"?`)) {
      this.integrationService.deleteNodeDetails(Number(record.apidetailsId)).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.fetchAPIDetailsList();
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
  
  onFileSelect(event: Event) {
    const input = event.target as HTMLInputElement;

    if (input.files?.length) {
      const file = input.files[0];
      this.nodeAPIformData.xmlfileName = file.name;

      this.nodeAPIForm.patchValue({ xmlFile: file });

      const reader = new FileReader();
      reader.onload = () => {
        const base64String = reader.result as string;
        const base64WithoutPrefix = base64String.split(',')[1];
        this.nodeAPIformData.binaryXml = base64WithoutPrefix;
        this.selectedXMLFile = base64WithoutPrefix;
      };
      reader.readAsDataURL(file);
    }
  }

}



