import { Component, inject, OnInit, ViewChild } from "@angular/core";
import { RoleService } from "../../../core/services/security/role.service";
import { MatSnackBar } from "@angular/material/snack-bar";
import { MatTableDataSource } from "@angular/material/table";
import { MatCheckboxChange } from "@angular/material/checkbox";
import { MatSort } from "@angular/material/sort";
import { MatPaginator } from "@angular/material/paginator";
import { RolesService } from "../../../core/services/setting/role.service";

export interface GroupRecord {
    groupId: number | null;
    groupName: string;
    groupDesc: string;
}

export interface RoleRecord {
    roleId: number;
    groupId: number;
    roleAction: string;
    selected: boolean;
}

@Component({
  selector: 'app-role',
  standalone: false,
  templateUrl: './role.component.html',
  styleUrl: './role.component.scss'
})

export class RoleComponent implements OnInit {
  records: GroupRecord[] = [];
  roleRecord: RoleRecord[] = [];
  roleAssigndataSource = new MatTableDataSource<RoleRecord>(this.roleRecord);
  roleUnAssigndataSource = new MatTableDataSource<RoleRecord>(this.roleRecord);
  private _snackBar = inject(MatSnackBar);
  activeTab: string = 'AssignedRoles';
  searchTerm: string = '';
  menuVisible = false;
  formVisible = false;
  selectedRoleId: number[] = [];
  availableRoles: any[] = [];
  assignedRoles: any[] = [];
  selectedRoleIds: number[] = [];
  selectedGroupId: number | null = null;
  requestBody: { groupId: number; roleIds: number[] } = {
    groupId: 0,
    roleIds: [],
  };
  selectedRows: Set<number> = new Set();
  selectedRowsItem: Set<number> = new Set();
  displayedColumns: string[] = ['roleAction']; // Original columns
  combinedColumns: string[] = []; // To include 'select' column
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  isLoading: boolean = false;
  message: string = "Loading data, please wait...";
  isUploading: boolean = false;
  isDownloading: boolean = false;
  constructor(private roleService: RoleService, private rolesService: RolesService) { }

  ngOnInit(): void {
    this.combinedColumns = ['select', ...this.displayedColumns];
    this.fetchGroupList();
  }

  hasPermission(roleId: string): boolean {
    return this.rolesService.hasPermission(roleId);
  }

  switchTab(tab: string): void {
    this.activeTab = tab;
    if (this.selectedGroupId !== null) {
      if (tab === 'AssignedRoles') {
        this.getAssignedRolesByGroupId(this.selectedGroupId!);
      } else {
        this.getUnAssignedRolesByGroupId(this.selectedGroupId!);
      }
    }
  }

  fetchGroupList() {
    this.isLoading = true;
    this.roleService.getGroupList().subscribe({
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
    this.getAssignedRolesByGroupId(this.selectedGroupId!);
  }

  onSelectRole(event: Event): void {
    const selectedOptions = (event.target as HTMLSelectElement).selectedOptions;
    this.selectedRoleIds = Array.from(selectedOptions).map((option) => +option.value);
  }

  deleteRole() {
    this.requestBody.groupId = this.selectedGroupId!;
    this.requestBody.roleIds = this.selectedRoleIds;
    this.roleService.deleteRole(this.requestBody).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.selectedRoleIds = []
          //this.getUnAssignedRolesByGroupId(this.requestBody.groupId);
          this.getAssignedRolesByGroupId(this.requestBody.groupId);
          this._snackBar.open("Role Removed successfully.", 'Okay', {
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

  addRole() {
    this.requestBody.groupId = this.selectedGroupId!;
    this.requestBody.roleIds = this.selectedRoleIds;
    if (!this.requestBody.roleIds || this.requestBody.roleIds.length === 0) {
      this._snackBar.open("Please select at least one role.", 'Okay', {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000,
      });
      return;
    }
    this.roleService.addRole(this.requestBody).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.selectedRoleIds = [];
          this.getUnAssignedRolesByGroupId(this.requestBody.groupId);
          this.getAssignedRolesByGroupId(this.requestBody.groupId);
          this._snackBar.open("Role Added successfully.", 'Okay', {
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

     
    

    getUnAssignedRolesByGroupId(groupId: number) {
      this.isLoading = true;
        this.roleService.getUnAssignedRolesByGroupId(groupId).subscribe({
            next: (response) => {
                this.roleUnAssigndataSource.data = response.data.map((item: any) => ({
                    roleId: item.roleId,
                    groupId: item.groupId,
                    roleAction: item.roleAction
                }));
                this.roleUnAssigndataSource.paginator = this.paginator;
                this.roleUnAssigndataSource.sort = this.sort;
                this.isLoading = false;
            },
            error: (error) => {
                this.roleAssigndataSource.data = [];
                this.roleUnAssigndataSource.data = [];
                this._snackBar.open(error.message, 'Okay', {
                    horizontalPosition: 'right',
                    verticalPosition: 'top', duration: 3000,
                });
                this.isLoading = false;
            },
        });
    }

    getAssignedRolesByGroupId(groupId: number) {
      this.isLoading = true;
        this.roleService.getAssignedRolesByGroupId(groupId).subscribe({
            next: (response) => {
                this.roleAssigndataSource.data = response.data.map((item: any) => ({
                    roleId: item.roleId,
                    groupId: item.groupId,
                    roleAction: item.roleAction
                }));
                this.roleAssigndataSource.paginator = this.paginator;
                this.roleAssigndataSource.sort = this.sort;
                this.isLoading = false;
            },
            error: (error) => {
                this.roleAssigndataSource.data = [];
                this.roleUnAssigndataSource.data = [];

                this._snackBar.open(error.message, 'Okay', {
                    horizontalPosition: 'right',
                    verticalPosition: 'top', duration: 3000,
                });
                this.isLoading = false;
            },
        });
    }

    onCheckboxChange(entity: RoleRecord): void {
        if (entity.selected) {
            this.selectedRoleIds.push(entity.roleId);
        } else {
            this.selectedRoleIds = this.selectedRoleIds.filter(id => id !== entity.roleId);
        }
    }

    applyFilter(event: Event) {
        const filterValue = (event.target as HTMLInputElement).value.trim().toLowerCase();
        if (this.activeTab === 'AvailableRoles') {
            this.roleUnAssigndataSource.filter = filterValue;
            this.roleUnAssigndataSource.paginator = this.paginator;
            this.roleUnAssigndataSource.sort = this.sort;
        }
    }
}
