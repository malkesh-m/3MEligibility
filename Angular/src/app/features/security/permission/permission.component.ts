import { Component, inject, OnInit, ViewChild } from "@angular/core";
import { PermissionService } from "../../../core/services/security/permission.service";
import { MatSnackBar } from "@angular/material/snack-bar";
import { MatTableDataSource } from "@angular/material/table";
import { MatCheckboxChange } from "@angular/material/checkbox";
import { MatSort } from "@angular/material/sort";
import { MatPaginator } from "@angular/material/paginator";
import { PermissionsService } from "../../../core/services/setting/permission.service";

export interface GroupRecord {
    groupId: number | null;
    groupName: string;
    groupDesc: string;
}

export interface PermissionRecord {
    permissionId: number;
    groupId: number;
    permissionAction: string;
    selected: boolean;
}

@Component({
  selector: 'app-permission',
  standalone: false,
  templateUrl: './permission.component.html',
  styleUrl: './permission.component.scss'
})

export class PermissionComponent implements OnInit {
  records: GroupRecord[] = [];
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
  selectedGroupId: number | null = null;
  requestBody: { groupId: number; permissionIds: number[] } = {
    groupId: 0,
    permissionIds: [],
  };
  selectedRows: Set<number> = new Set();
  selectedRowsItem: Set<number> = new Set();
  displayedColumns: string[] = ['permissionAction']; // Original columns
  combinedColumns: string[] = []; // To include 'select' column
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  isLoading: boolean = false;
  message: string = "Loading data, please wait...";
  isUploading: boolean = false;
  isDownloading: boolean = false;
  constructor(private permissionService: PermissionService, private PermissionsService: PermissionsService) { }

  ngOnInit(): void {
    this.combinedColumns = ['select', ...this.displayedColumns];
    this.fetchGroupList();
  }

  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }

  switchTab(tab: string): void {
    this.activeTab = tab;
    if (this.selectedGroupId !== null) {
      if (tab === 'AssignedPermissions') {
        this.getAssignedPermissionsByGroupId(this.selectedGroupId!);
      } else {
        this.getUnAssignedPermissionsByGroupId(this.selectedGroupId!);
      }
    }
  }

  fetchGroupList() {
    this.isLoading = true;
    this.permissionService.getGroupList().subscribe({
      next: (response) => {
        this.records = response.data.map((item: any) => ({
          groupId: item.groupId,
          groupName: item.groupName,
          groupDesc: item.groupDesc
        }));
        this.isLoading = false;
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
        this.isLoading = false;
      },
    });
  }

  onSelect(event: any): void {
    this.selectedGroupId = event.target.value;
    this.getAssignedPermissionsByGroupId(this.selectedGroupId!);
  }

  onSelectPermission(event: Event): void {
    const selectedOptions = (event.target as HTMLSelectElement).selectedOptions;
    this.selectedPermissionIds = Array.from(selectedOptions).map((option) => +option.value);
  }

  deletePermission() {
    this.requestBody.groupId = this.selectedGroupId!;
    this.requestBody.permissionIds = this.selectedPermissionIds;
    this.permissionService.deletePermission(this.requestBody).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.selectedPermissionIds = []
          //this.getUnAssignedPermissionsByGroupId(this.requestBody.groupId);
          this.getAssignedPermissionsByGroupId(this.requestBody.groupId);
          this._snackBar.open("Permission removed successfully.", 'Okay', {
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

  addPermission() {
    this.requestBody.groupId = this.selectedGroupId!;
    this.requestBody.permissionIds = this.selectedPermissionIds;
    if (!this.requestBody.permissionIds || this.requestBody.permissionIds.length === 0) {
      this._snackBar.open("Please select at least one permission.", 'Okay', {
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
          this.getUnAssignedPermissionsByGroupId(this.requestBody.groupId);
          this.getAssignedPermissionsByGroupId(this.requestBody.groupId);
          this._snackBar.open("Permission added successfully.", 'Okay', {
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
        console.error('Error adding assigned user:', error)
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      }
    })};

     
    

    getUnAssignedPermissionsByGroupId(groupId: number) {
      this.isLoading = true;
        this.permissionService.getUnAssignedPermissionsByGroupId(groupId).subscribe({
            next: (response) => {
                this.permissionUnassignedDataSource.data = response.data.map((item: any) => ({
                    permissionId: item.permissionId,
                    groupId: item.groupId,
                    permissionAction: item.permissionAction
                }));
                this.permissionUnassignedDataSource.paginator = this.paginator;
                this.permissionUnassignedDataSource.sort = this.sort;
                this.isLoading = false;
            },
            error: (error) => {
                this.permissionAssignedDataSource.data = [];
                this.permissionUnassignedDataSource.data = [];
                this._snackBar.open(error.message, 'Okay', {
                    horizontalPosition: 'right',
                    verticalPosition: 'top', duration: 3000,
                });
                this.isLoading = false;
            },
        });
    }

    getAssignedPermissionsByGroupId(groupId: number) {
      this.isLoading = true;
        this.permissionService.getAssignedPermissionsByGroupId(groupId).subscribe({
            next: (response) => {
                this.permissionAssignedDataSource.data = response.data.map((item: any) => ({
                    permissionId: item.permissionId,
                    groupId: item.groupId,
                    permissionAction: item.permissionAction
                }));
                this.permissionAssignedDataSource.paginator = this.paginator;
                this.permissionAssignedDataSource.sort = this.sort;
                this.isLoading = false;
            },
            error: (error) => {
                this.permissionAssignedDataSource.data = [];
                this.permissionUnassignedDataSource.data = [];

                this._snackBar.open(error.message, 'Okay', {
                    horizontalPosition: 'right',
                    verticalPosition: 'top', duration: 3000,
                });
                this.isLoading = false;
            },
        });
    }

    onCheckboxChange(entity: PermissionRecord): void {
        if (entity.selected) {
            this.selectedPermissionIds.push(entity.permissionId);
        } else {
            this.selectedPermissionIds = this.selectedPermissionIds.filter(id => id !== entity.permissionId);
        }
    }

    applyFilter(event: Event) {
        const filterValue = (event.target as HTMLInputElement).value.trim().toLowerCase();
        if (this.activeTab === 'AvailablePermissions') {
            this.permissionUnassignedDataSource.filter = filterValue;
            this.permissionUnassignedDataSource.paginator = this.paginator;
            this.permissionUnassignedDataSource.sort = this.sort;
        }
    }
}




