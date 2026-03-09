import { Component, inject, OnInit, ViewChild } from "@angular/core";
import { Title } from "@angular/platform-browser";
import { Location } from "@angular/common";
import { ActivatedRoute, Router } from "@angular/router";
import { PermissionService } from "../../../core/services/security/permission.service";
import { MatSnackBar } from "@angular/material/snack-bar";
import { MatTableDataSource } from "@angular/material/table";
import { MatCheckboxChange } from "@angular/material/checkbox";
import { MatSort } from "@angular/material/sort";
import { MatPaginator } from "@angular/material/paginator";
import { PermissionsService } from "../../../core/services/setting/permission.service";
import { TranslateService } from "@ngx-translate/core";
import { HeaderTitleService } from "../../../core/services/header-title.service";

export interface RoleRecord {
  roleId: number | null;
  roleName: string;
  roleDesc: string;
}

export interface PermissionRecord {
  permissionId: number;
  roleId: number;
  permissionAction: string;
  permissionName: string;
  isMasterSwitch: boolean;
  moduleName: string;
  resourceName: string;
  selected: boolean;
}
@Component({
  selector: 'app-permission',
  standalone: false,
  templateUrl: './permission.component.html',
  styleUrl: './permission.component.scss'
})

export class PermissionComponent implements OnInit {
  // Permissions are filtered to those referenced in the frontend.
  records: RoleRecord[] = [];
  permissionRecord: PermissionRecord[] = [];
  permissionAssignedDataSource = new MatTableDataSource<PermissionRecord>(this.permissionRecord);
  permissionUnassignedDataSource = new MatTableDataSource<PermissionRecord>(this.permissionRecord);
  private _snackBar = inject(MatSnackBar);
  activeTab: string = 'AssignedPermissions';
  searchTerm: string = '';
  menuVisible = false;
  formVisible = false;
  selectedPermissionId: number[] = [];
  availablePermissions: any[] = [];
  assignedPermissions: any[] = [];
  selectedPermissionIds: number[] = [];
  selectedRoleId: number | null = null;
  requestBody: { roleId: number; permissionIds: number[] } = {
    roleId: 0,
    permissionIds: [],
  };
  selectedRows: Set<number> = new Set();
  selectedRowsItem: Set<number> = new Set();
  displayedColumns: string[] = ['permissionAction']; // Original columns
  combinedColumns: string[] = []; // To include 'select' column
  @ViewChild('assignedPaginator') assignedPaginator!: MatPaginator;
  @ViewChild('unassignedPaginator') unassignedPaginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  isLoading: boolean = false;
  message: string = this.translate.instant("Loading data, please wait...");
  isUploading: boolean = false;
  isDownloading: boolean = false;

  // Hierarchical Data
  groupedAssigned: any[] = [];
  groupedUnassigned: any[] = [];
  constructor(
    private permissionService: PermissionService,
    private PermissionsService: PermissionsService,
    private titleService: Title,
    private location: Location,
    private router: Router,
    private route: ActivatedRoute,
    private translate: TranslateService,
    private headerTitleService: HeaderTitleService
  ) { }

  private readonly moduleOrder: string[] = [
    'Dashboard',
    'Setup',
    'Conditions',
    'Connections',
    'Security',
    'Maker Checker',
    'Logs',
    'Configuration',
    'Bulk Import'
  ];
  // Keep permission screen focused on permissions actually referenced in the frontend.
  // This prevents assigning backend-only or unused permissions.
  private readonly frontendPermissionActions = new Set<string>([
    'permissions.accesscontrol.access',
    'permissions.approvals.access',
    'permissions.audit.screen',
    'permissions.bulkimport.access',
    'permissions.bulkimport.screen',
    'permissions.bulkimport.view',
    'permissions.bulkimport.download',
    'permissions.bulkimport.import',
    'permissions.businesslogic.access',
    'permissions.connections.access',
    'permissions.dashboard.screen',
    'permissions.datatype.view',
    'permissions.ecard.screen',
    'permissions.ecard.create',
    'permissions.ecard.delete',
    'permissions.ecard.edit',
    'permissions.ecard.export',
    'permissions.ecard.import',
    'permissions.factor.screen',
    'permissions.factor.create',
    'permissions.factor.delete',
    'permissions.factor.edit',
    'permissions.factor.export',
    'permissions.factor.import',
    'permissions.integration.screen',
    'permissions.limitandcaps.access',
    'permissions.log.view',
    'permissions.logs.access',
    'permissions.makerchecker.screen',
    'permissions.makerchecker.view',
    'permissions.makerchecker.edit',
    'permissions.makercheckerconfig.screen',
    'permissions.makercheckerconfig.edit',
    'permissions.managedlist.screen',
    'permissions.managedlist.create',
    'permissions.managedlist.delete',
    'permissions.managedlist.edit',
    'permissions.managedlist.export',
    'permissions.managedlist.import',
    'permissions.masterdata.access',
    'permissions.parameter.screen',
    'permissions.parameter.create',
    'permissions.parameter.delete',
    'permissions.parameter.edit',
    'permissions.parameter.export',
    'permissions.parameter.import',
    'permissions.parameterbinding.screen',
    'permissions.pcard.screen',
    'permissions.pcard.create',
    'permissions.pcard.delete',
    'permissions.pcard.edit',
    'permissions.pcard.export',
    'permissions.pcard.import',
    'permissions.permission.screen',
    'permissions.product.screen',
    'permissions.product.create',
    'permissions.product.delete',
    'permissions.product.edit',
    'permissions.product.export',
    'permissions.product.import',
    'permissions.productcap.screen',
    'permissions.productcap.create',
    'permissions.productcap.delete',
    'permissions.productcap.edit',
    'permissions.productcapamount.screen',
    'permissions.productcapamount.create',
    'permissions.productcapamount.delete',
    'permissions.productcapamount.edit',
    'permissions.role.screen',
    'permissions.role.create',
    'permissions.role.delete',
    'permissions.role.edit',
    'permissions.rolepermission.view',
    'permissions.rolepermission.create',
    'permissions.rolepermission.delete',
    'permissions.rolepermission.edit',
    'permissions.rule.screen',
    'permissions.rule.create',
    'permissions.rule.delete',
    'permissions.rule.edit',
    'permissions.rule.export',
    'permissions.rule.import',
    'permissions.userrole.create',
    'permissions.userrole.delete',
    'permissions.validator.rule',
    'permissions.validator.ecard',
    'permissions.validator.pcard',
    'permissions.apiparametermaps.view',
    'permissions.apiparametermaps.create',
    'permissions.apiparametermaps.edit',
    'permissions.apiparametermaps.delete',
    'permissions.apiparameters.view',
    'permissions.apiparameters.create',
    'permissions.apiparameters.edit',
    'permissions.apiparameters.delete',
    'permissions.node.view',
    'permissions.node.create',
    'permissions.node.edit',
    'permissions.node.delete',
    'permissions.nodeapi.view',
    'permissions.nodeapi.create',
    'permissions.nodeapi.edit',
    'permissions.nodeapi.delete',
    'permissions.category.create',
    'permissions.category.delete',
    'permissions.category.edit',
    'permissions.category.export',
    'permissions.category.import',
    'permissions.listitem.create',
    'permissions.listitem.delete',
    'permissions.listitem.edit',
    'permissions.listitem.export',
    'permissions.listitem.import'
  ]);

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['tab']) {
        this.activeTab = params['tab'];
      }
    });

    this.combinedColumns = ['select', ...this.displayedColumns];
    this.fetchRoleList();
    this.updateTitle();
  }

  get activeTabTitle(): string {
    switch (this.activeTab) {
      case 'AssignedPermissions':
        return 'Assigned Permissions';
      case 'AvailablePermissions':
        return 'Available Permissions';
      default:
        return 'Permissions';
    }
  }

  updateTitle() {
    this.titleService.setTitle(`${this.activeTabTitle} - 3M Eligibility`);
    this.headerTitleService.setTitle(this.activeTabTitle);
  }
  isSuperAdminSelected(): boolean {
    const role = this.records.find(r => r.roleId == this.selectedRoleId);
    return role?.roleName?.toLowerCase() === 'super admin';
  }
  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }

  switchTab(tab: 'AssignedPermissions' | 'AvailablePermissions'): void {
    this.activeTab = tab;
    const urlTree = this.router.createUrlTree([], {
      relativeTo: this.route,
      queryParams: { tab: tab },
      queryParamsHandling: 'merge'
    });
    this.location.go(urlTree.toString());
    this.updateTitle();

    if (this.selectedRoleId !== null) {
      if (tab === 'AssignedPermissions') {
        this.getAssignedPermissionsByRoleId(this.selectedRoleId!);
      } else {
        this.getUnAssignedPermissionsByRoleId(this.selectedRoleId!);
      }
    }
  }

  fetchRoleList() {
    this.isLoading = true;
    this.permissionService.getRoleList().subscribe({
      next: (response) => {
        this.records = response.data.map((item: any) => ({
          roleId: item.roleId,
          roleName: item.roleName,
          roleDesc: item.roleDesc
        }));
        this.isLoading = false;
      },
      error: (error) => {
        this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
        this.isLoading = false;
      },
    });
  }

  onSelect(event: any): void {
    this.selectedRoleId = event.target.value;
    this.getAssignedPermissionsByRoleId(this.selectedRoleId!);
  }

  onSelectPermission(event: Event): void {
    const selectedOptions = (event.target as HTMLSelectElement).selectedOptions;
    this.selectedPermissionIds = Array.from(selectedOptions).map((option) => +option.value);
  }

  deletePermission() {

    if (this.isSuperAdminSelected()) {
      this._snackBar.open(
        this.translate.instant("You cannot remove permissions from Super Admin role."),
        this.translate.instant('Okay'),
        {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 3000,
        }
      );
      return;
    }

    this.requestBody.roleId = this.selectedRoleId!;
    this.requestBody.permissionIds = this.selectedPermissionIds;

    this.permissionService.deletePermission(this.requestBody).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.selectedPermissionIds = [];
          this.getAssignedPermissionsByRoleId(this.requestBody.roleId);

          this._snackBar.open(this.translate.instant("Permission removed successfully."), this.translate.instant('Okay'), {
            horizontalPosition: 'right',
            verticalPosition: 'top',
            duration: 3000,
          });
        } else {
          this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
            horizontalPosition: 'right',
            verticalPosition: 'top',
            duration: 3000,
          });
        }
      }
    });
  }

  addPermission() {
    this.requestBody.roleId = this.selectedRoleId!;
    this.requestBody.permissionIds = this.selectedPermissionIds;
    if (!this.requestBody.permissionIds || this.requestBody.permissionIds.length === 0) {
      this._snackBar.open(this.translate.instant("Please select at least one permission."), this.translate.instant('Okay'), {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000,
      });
      return;
    }
    this.permissionService.addPermission(this.requestBody).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.selectedPermissionIds = [];
          this.getUnAssignedPermissionsByRoleId(this.requestBody.roleId);
          this.getAssignedPermissionsByRoleId(this.requestBody.roleId);
          this._snackBar.open(this.translate.instant("Permission added successfully."), this.translate.instant('Okay'), {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000,
          });
        } else {
          this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000,
          });
        }
      },
      error: (error) => {
        console.error('Error adding assigned user:', error)
        this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      }
    })
  };

  getUnAssignedPermissionsByRoleId(roleId: number) {
    this.isLoading = true;
    this.permissionService.getUnAssignedPermissionsByRoleId(roleId).subscribe({
      next: (response) => {
        const raw = Array.isArray(response.data) ? response.data : [];
        const data = raw
          .filter((item: any) => this.isFrontendPermission(item.permissionAction))
          .map((item: any) => ({
          permissionId: item.permissionId,
          roleId: item.roleId,
          permissionAction: item.permissionAction || '',
          permissionName: item.permissionName || item.permissionAction || '',
          isMasterSwitch: !!item.isMasterSwitch || (item.permissionAction || '').endsWith('.Screen'),
          moduleName: item.moduleName || 'General',
          resourceName: item.resourceName || 'General',
          selected: false
        })).sort((a: any, b: any) => this.comparePermissions(a, b));

        this.permissionUnassignedDataSource.data = data;
        this.permissionUnassignedDataSource.paginator = this.unassignedPaginator;
        this.permissionUnassignedDataSource.sort = this.sort;
        this.isLoading = false;
      },
      error: (error) => {
        this.permissionUnassignedDataSource.data = [];
        this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
        this.isLoading = false;
      },
    });
  }

  getAssignedPermissionsByRoleId(roleId: number) {
    this.isLoading = true;
    this.permissionService.getAssignedPermissionsByRoleId(roleId).subscribe({
      next: (response) => {
        const raw = Array.isArray(response.data) ? response.data : [];
        const data = raw
          .filter((item: any) => this.isFrontendPermission(item.permissionAction))
          .map((item: any) => ({
          permissionId: item.permissionId,
          roleId: item.roleId,
          permissionAction: item.permissionAction || '',
          permissionName: item.permissionName || item.permissionAction || '',
          isMasterSwitch: !!item.isMasterSwitch || (item.permissionAction || '').endsWith('.Screen'),
          moduleName: item.moduleName || 'General',
          resourceName: item.resourceName || 'General',
          selected: false
        })).sort((a: any, b: any) => this.comparePermissions(a, b));

        this.permissionAssignedDataSource.data = data;
        this.permissionAssignedDataSource.paginator = this.assignedPaginator;
        this.permissionAssignedDataSource.sort = this.sort;
        this.isLoading = false;
      },
      error: (error) => {
        this.permissionAssignedDataSource.data = [];
        this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
        this.isLoading = false;
      },
    });
  }

  onCheckboxChange(entity: PermissionRecord): void {
    // Simple flat selection. No UI-level cascading cascades.
    // Backend handles all mapping logic (PermissionDependencies) on Save/Remove.
    this.updateSelectedList(entity);
  }

  private updateSelectedList(entity: PermissionRecord): void {
    if (entity.selected) {
      if (!this.selectedPermissionIds.includes(entity.permissionId)) {
        this.selectedPermissionIds.push(entity.permissionId);
      }
    } else {
      this.selectedPermissionIds = this.selectedPermissionIds.filter(id => id !== entity.permissionId);
    }
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value.trim().toLowerCase();
    if (this.activeTab === 'AvailablePermissions') {
      this.permissionUnassignedDataSource.filter = filterValue;
      this.permissionUnassignedDataSource.paginator = this.unassignedPaginator;
      this.permissionUnassignedDataSource.sort = this.sort;
    }
  }

  formatPermissionAction(action: string): string {
    if (!action) return '';
    // Simply remove "Permissions." and replace dots with spaces
    return action.replace('Permissions.', '').replace(/\./g, ' ');
  }
  private isFrontendPermission(action?: string | null): boolean {
    if (!action) return false;
    return this.frontendPermissionActions.has(action.toLowerCase().trim());
  }

  private toTitleWords(value: string): string {
    if (!value) return '';
    return value
      .replace(/([a-z])([A-Z])/g, '$1 $2')
      .replace(/_/g, ' ')
      .trim();
  }

  private normalizeModuleName(value: string): string {
    if (!value) return 'General';
    const name = this.toTitleWords(value);
    if (name.toLowerCase() === 'dashboard') return 'Dashboard';
    if (name.toLowerCase() === 'maker checker') return 'Maker Checker';
    if (name.toLowerCase() === 'bulk import') return 'Bulk Import';
    if (name.toLowerCase() === 'product cap amount') return 'Product Cap Amount';
    if (name.toLowerCase() === 'product cap') return 'Product Cap';
    if (name.toLowerCase() === 'api parameter maps') return 'API Parameter Maps';
    if (name.toLowerCase() === 'api parameters') return 'API Parameters';
    if (name.toLowerCase() === 'api details') return 'API Details';
    if (name.toLowerCase() === 'pcard') return 'P Card';
    return name;
  }

  private comparePermissions(a: PermissionRecord, b: PermissionRecord): number {
    // 1. Global Priority: .Access (Module Headers) and .Screen permissions come first
    const aIsAccess = a.permissionAction.endsWith('.Access');
    const bIsAccess = b.permissionAction.endsWith('.Access');
    const aIsScreen = a.permissionAction.endsWith('.Screen');
    const bIsScreen = b.permissionAction.endsWith('.Screen');

    // .Access always absolute first
    if (aIsAccess !== bIsAccess) {
      return aIsAccess ? -1 : 1;
    }

    // .Screen always absolute second (before other actions)
    if (aIsScreen !== bIsScreen) {
      return aIsScreen ? -1 : 1;
    }

    // 2. Secondary sort: Module Order
    const aModuleIndex = this.moduleOrder.indexOf(this.normalizeModuleName(a.moduleName));
    const bModuleIndex = this.moduleOrder.indexOf(this.normalizeModuleName(b.moduleName));

    const aModIdx = aModuleIndex === -1 ? 99 : aModuleIndex;
    const bModIdx = bModuleIndex === -1 ? 99 : bModuleIndex;

    if (aModIdx !== bModIdx) {
      return aModIdx - bModIdx;
    }

    // 3. Tertiary sort: Alphabetical by name
    return a.permissionName.localeCompare(b.permissionName);
  }

}
