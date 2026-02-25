import { AfterViewInit, ChangeDetectorRef, Component, HostListener, inject, OnInit, ViewChild } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatTableDataSource } from '@angular/material/table';
import { HeaderTitleService } from '../../../core/services/header-title.service';
import { RoleService } from '../../../core/services/security/role.service';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { UserService } from '../../../core/services/security/user.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { PermissionsService } from '../../../core/services/setting/permission.service';
import { UtilityService } from '../../../core/services/utility/utils';
import { NgForm } from '@angular/forms';
import { AuthService } from '../../../core/services/auth/auth.service';
import { UserprogileService } from '../../../core/services/security/userprofile.service';
import { DeleteDialogComponent } from '../../../core/components/delete-dialog/delete-dialog.component';
import { Rank } from '../../../core/enum/rank.enum';
import { TranslateService } from '@ngx-translate/core';
export interface RoleRecord {
  roleId: number | null;
  roleName: string;
  roleDesc: string;
  userCount?: number;
}

export interface AssignedUserRecord {
  id: number | null;
  roleId: number | null;
  userName: string;
  loginId: number;
  email: string;
  entityName: string;
}

export interface UserRecord {
  id: number | null;
  firstName: string;
  lastName: string;
  email: string;
  mobileNo: string;
  userPicture?: string;
  userProfileFile?: string;
  tenantId: number;
  entitylocation?: string;
  selected: boolean;
  displayName: string;
}

export interface requestBody {
  userId: number | null;
  roleId: string | null;
}

@Component({
  selector: 'app-role',
  standalone: false,
  templateUrl: './role.component.html',
  styleUrl: './role.component.scss'
})
export class RoleComponent implements OnInit, AfterViewInit {
  private readonly superAdminRoleName = 'Super Admin';
  private readonly adminRoleName = 'Admin';
  private readonly userRoleName = 'User';

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  searchTerm: string = '';
  searchRole: string = '';
  menuVisible = false;
  formVisible = false;
  isEditMode = false;
  displayedColumns: string[] = ['roleName', 'roleDesc', 'actions'];
  assignedUserColumns: string[] = ['userName', 'email', 'mobileNo', 'actions'];
  Rank: any = Rank;
  records: RoleRecord[] = [];
  userRecords: UserRecord[] = [];
  assignedUserData: AssignedUserRecord[] = [];

  dataSource = new MatTableDataSource<RoleRecord>(this.records);
  userdataSource = new MatTableDataSource<UserRecord>(this.userRecords);
  assignedUserDataSource = new MatTableDataSource<AssignedUserRecord>(this.assignedUserData);

  formData: RoleRecord = {
    roleId: 0,
    roleName: '',
    roleDesc: '',
  };
  activeTab?: string = 'role';
  selectedRoleName: string | null = null;
  filteredRoleNames: UserRecord[] = [];
  requestBody: { id: number | null; roleId: string | null } = { id: null, roleId: null };
  private _snackBar = inject(MatSnackBar);
  UnassignedUser: boolean = false;
  recordMessage: string = '';
  isLoading: boolean = false;
  isDownloading: boolean = false
  isUploading: boolean = false
  message: string = "Loading data, please wait...";
  currentUserRoles: string[] = [];
  currentUserRank: number = 0;
  currentUserId: number | null = null;

  constructor(
    private roleService: RoleService,
    private utilityService: UtilityService,
    private cdr: ChangeDetectorRef,
    private userService: UserService,
    private PermissionsService: PermissionsService,
    private authService: AuthService,
    private userprofileService: UserprogileService,
    private dialog: MatDialog,
    private titleService: Title,
    private location: Location,
    private router: Router,
    private route: ActivatedRoute,
    private translate: TranslateService,
    private headerTitleService: HeaderTitleService
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['tab']) {
        this.activeTab = params['tab'];
      }
    });
    this.fetchRoleList();
    this.fetchUsersList();
    this.loadCurrentUserRoles();
    this.updateTitle();
  }

  get activeTabTitle(): string {
    switch (this.activeTab) {
      case 'role':
        return 'Role - Roles';
      case 'assignedUser':
        return 'Role - Assigned User';
      default:
        return 'Role - Roles';
    }
  }

  updateTitle() {
    this.titleService.setTitle(`${this.activeTabTitle} - 3M Eligibility`);
    this.headerTitleService.setTitle(this.activeTabTitle);
  }
  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  getAddPermission(): boolean {
    return (this.activeTab === 'role' && this.hasPermission('49')) ||
      (this.activeTab === 'assignedUser' && this.hasPermission('45'));
  }

  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }

  canEditRole(record: RoleRecord): boolean {
    return this.hasPermission('Permissions.Role.Edit');
  }

  canDeleteRole(record: RoleRecord): boolean {
    if (!this.hasPermission('Permissions.Role.Delete')) {
      return false;
    }
    if (this.isSuperAdminRole(record.roleName)) {
      return this.currentUserRank === 3 && (record.userCount ?? 0) > 1;
    }
    return true;
  }

  canDeleteAssignedUser(): boolean {
    return this.hasPermission('Permissions.UserRole.Delete');
  }

  private isSuperAdminRole(roleName: string): boolean {
    return roleName?.toLowerCase() === this.superAdminRoleName.toLowerCase();
  }

  private canManageSelectedRole(): boolean {
    if (!this.selectedRoleName) {
      return false;
    }
    const selectedRoleId = Number(this.selectedRoleName);
    const roleName = this.records.find(r => r.roleId === selectedRoleId)?.roleName ?? '';
    if (this.isSuperAdminRole(roleName)) {
      if (this.hasPermission('Permissions.UserRole.Delete')) {
        return true;
      }
      return this.currentUserRank === 3;
    }
    return true;
  }

  private loadCurrentUserRoles(): void {
    this.authService.currentUser$.subscribe((user) => {
      const userId = user?.user?.userId ?? user?.userId;
      if (!userId) {
        this.authService.loadUserPermissions().subscribe({
          next: (res: any) => {
            const resolvedId = res?.userId ?? null;
            if (resolvedId) {
              this.currentUserId = resolvedId;
              this.userprofileService.getUserProfileById(resolvedId).subscribe({
                next: (response) => {
                  const roles = response?.data?.roles ?? [];
                  this.currentUserRoles = roles.map((r: any) => r.roleName).filter(Boolean);
                  this.currentUserRank = this.getHighestRank(this.currentUserRoles);
                },
                error: () => {
                  this.currentUserRoles = [];
                  this.currentUserRank = 0;
                }
              });
            }
          }
        });
        return;
      }
      this.currentUserId = userId;
      this.userprofileService.getUserProfileById(userId).subscribe({
        next: (response) => {
          const roles = response?.data?.roles ?? [];
          this.currentUserRoles = roles.map((r: any) => r.roleName).filter(Boolean);
          this.currentUserRank = this.getHighestRank(this.currentUserRoles);
        },
        error: () => {
          this.currentUserRoles = [];
          this.currentUserRank = 0;
        }
      });
    });
  }

  private getHighestRank(roleNames: string[]): number {
    let highest = 0;
    for (const name of roleNames) {
      const rank = this.getRank(name);
      if (rank > highest) {
        highest = rank;
      }
    }
    return highest;
  }

  private getRank(roleName: string): number {
    if (roleName?.toLowerCase() === this.superAdminRoleName.toLowerCase()) {
      return 3;
    }
    if (roleName?.toLowerCase() === this.adminRoleName.toLowerCase()) {
      return 2;
    }
    if (roleName?.toLowerCase() === this.userRoleName.toLowerCase()) {
      return 1;
    }
    return 0;
  }

  sanitizeCode(event: any) {
    event.target.value = this.utilityService.sanitizeCode(event.target.value);
  }

  submitEditForm(form: NgForm): void {

    if (form.invalid) {
      Object.keys(form.controls).forEach((controlName) => {
        form.controls[controlName].markAsTouched();
      });

      this._snackBar.open(this.translate.instant('Please fill in the required fields.'), this.translate.instant('Okay'), {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000
      });
      return;
    }

    if (this.activeTab === 'role') {

      if (this.isEditMode) {
        this.roleService.updateRoleDetails(this.formData).subscribe({
          next: (response) => {
            this.fetchRoleList();
            this.closeForm();
            this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top',
              duration: 3000
            });
          }
        });

      } else {
        this.roleService.addRole(this.formData).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.fetchRoleList();
              this.closeForm();
            }
            this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top',
              duration: 3000
            });
          }
        });
      }

      return;
    }

    if (this.activeTab === 'assignedUser') {

      if (!this.selectedRoleName) {
        this._snackBar.open(this.translate.instant('Please select Role'), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 3000
        });
        return;
      }

      const selectedUsers = this.filteredRoleNames.filter(u => u.selected);

      if (selectedUsers.length === 0) {
        this._snackBar.open(this.translate.instant('Please select at least one user.'), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 3000
        });
        return;
      }

      let completedCount = 0;
      const alreadyAssigned: string[] = [];
      const usersToAssign = selectedUsers.filter(user => {
        const isAssigned = this.assignedUserDataSource.data.some((item: any) =>
          item.id === user.id && item.roleId === Number(this.selectedRoleName)
        );
        if (isAssigned) {
          alreadyAssigned.push(user.displayName ?? user.email);
          return false;
        }
        return true;
      });

      if (alreadyAssigned.length > 0) {
        const selectedRoleId = Number(this.selectedRoleName);
        const roleName =
          this.records.find(r => r.roleId === selectedRoleId)?.roleName ??
          this.selectedRoleName ??
          'selected role';

        this._snackBar.open(
          this.translate.instant('User already present in role: {{roleName}}', { roleName }),
          this.translate.instant('Okay'),
          { horizontalPosition: 'right', verticalPosition: 'top', duration: 4000 }
        );
      }

      if (usersToAssign.length === 0) {
        return;
      }

      usersToAssign.forEach(user => {
        const payload = {
          roleId: this.selectedRoleName,
          userId: user.id
        };
        console.log('Assigning user with payload:', payload);

        this.roleService.addUsers(payload).subscribe({
          next: (response) => {
            this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top',
              duration: 3000
            });

            completedCount++;

            if (completedCount === selectedUsers.length) {
              this.fetchAssignedUserbyId(String(this.selectedRoleName));
              this.closeForm();
              this.searchRole = '';
              this.filteredRoleNames = [];
              this.userRecords.forEach(u => u.selected = false);
            }
          },
          error: (err) => console.error('Error assigning user:', err)
        });
      });

      return;
    }

    this.filteredRoleNames = [];
  }
  toggleMenu() {
    this.menuVisible = !this.menuVisible;
  }

  closeMenu() {
    this.menuVisible = false;
  }

  openForm(): void {
    if (this.activeTab == 'role') {
      this.formVisible = true;
      this.isEditMode = false;
      this.formData = {
        roleId: 0,
        roleName: '',
        roleDesc: ''
      };
    } else {
      this.UnassignedUser = true;
      this.isEditMode = false;
    }
  }

  addRecord() {
    if (this.activeTab === 'role') {
      if (this.isEditMode) {
        this.roleService.updateRoleDetails(this.formData).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.fetchRoleList();
              this.closeForm();
              this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
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
          error: (error) => console.error('Error updating role:', error),
        });
      } else {
        this.roleService.addRole(this.formData).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.fetchRoleList();
              this.closeForm();
              this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
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
          error: (error) => console.error('Error adding role:', error),
        });
      }
    } else if (this.activeTab === 'assignedUser') {
      const isUserAssigned = this.assignedUserDataSource.data.some((item: any) =>
        item.id === this.requestBody.id && item.roleId === parseInt(this.requestBody.roleId!)
      );
      if (isUserAssigned) {
        this._snackBar.open(this.translate.instant('User is already assigned to this role.'), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
        return;
      }

      if (this.selectedRoleName === null) {
        this._snackBar.open(this.translate.instant('Please select Role'), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
        this.closeForm();
        return;
      }

      this.roleService.addUsers(this.requestBody).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.fetchAssignedUserbyId(this.selectedRoleName!.toString());
            this.closeForm();
            this.searchRole = '';
            this.filteredRoleNames = []; // Clear filtered list
            this.userRecords.forEach(user => user.selected = false); // Reset selected state
            this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
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
        error: (error) => console.error('Error adding assigned user:', error),
      });
    }
    this.filteredRoleNames = [];
  }

  trackByUserId(index: number, role: any): number {
    return role.userId;
  }

  filterRoleNames() {
    if (!this.userRecords || this.userRecords.length === 0) {
      return;
    }
    const query = this.searchRole.toLowerCase().trim();
    if (query) {
      this.filteredRoleNames = this.userRecords.filter(role =>
        role.displayName.toLowerCase().startsWith(query)
      );
    } else {
      this.filteredRoleNames = [];
    }
    this.cdr.detectChanges();
  }

  onRoleSelectionChange(id: number | null) {
    this.requestBody.id = id;
    this.requestBody.roleId = this.selectedRoleName;
  }

  closeForm() {
    this.formVisible = false;
    if (this.activeTab === 'role') {
      this.formData = { roleId: 0, roleName: '', roleDesc: '' };
    }
    this.searchRole = '';
    this.filteredRoleNames = []; // Clear filtered list
    this.userRecords.forEach(user => user.selected = false); // Reset selected state
    this.isEditMode = false;
    this.UnassignedUser = false;
  }

  editRecord(record: RoleRecord) {
    if (!this.hasPermission('Permissions.Role.Edit')) {
      this._snackBar.open(this.translate.instant('You do not have permission to edit roles.'), this.translate.instant('Okay'), {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000
      });
      return;
    }
    if (this.isSuperAdminRole(record.roleName)) {
      this._snackBar.open(this.translate.instant('You can not edit this role.'), this.translate.instant('Okay'), {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000
      });
      return;
    }
    this.isEditMode = true;
    this.formVisible = true;
    this.formData = { ...record };
  }

  deleteRecord(record: RoleRecord) {
    if (!this.hasPermission('Permissions.Role.Delete')) {
      this._snackBar.open(this.translate.instant('You do not have permission to delete roles.'), this.translate.instant('Okay'), {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000
      });
      return;
    }
    if (this.isSuperAdminRole(record.roleName)) {
      this._snackBar.open(this.translate.instant('You cannot delete Super Admin role.'), this.translate.instant('Okay'), {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000
      });
      return;
    }
    const dialogRef = this.dialog.open(DeleteDialogComponent, {
      data: {
        title: this.translate.instant('Confirm'),
        message: this.translate.instant('Are you sure you want to delete Role: "{{roleName}}"?', { roleName: record.roleName }),
        confirmText: this.translate.instant('Confirm'),
        cancelText: this.translate.instant('Cancel')
      }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (!result?.delete) {
        return;
      }
      this.roleService.deleteRoleWithId(Number(record.roleId)).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            console.log(`Role with ID ${record.roleId} deleted successfully.`);
            this.fetchRoleList();
          } else {
            this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000,
            });
          }
        },
        error: (error) => {
          this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000,
          });
        },
      });
    });
  }

  onSelect(event: any): void {
    this.selectedRoleName = event.target.value; // Set selected role name
    if (this.selectedRoleName) {
      this.requestBody.roleId = this.selectedRoleName;
      this.fetchAssignedUserbyId(this.selectedRoleName); // Fetch assigned users based on selected role
    }
  }

  deleteAssignedUserRecord(record: AssignedUserRecord) {
    if (!this.canManageSelectedRole()) {
      this._snackBar.open(this.translate.instant('You can not delete users from this role.'), this.translate.instant('Okay'), {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000
      });
      return;
    }
    const selectedRoleId = Number(this.selectedRoleName);
    const selectedRole = this.records.find(r => r.roleId === selectedRoleId);
    if (selectedRole && this.isSuperAdminRole(selectedRole.roleName) && this.assignedUserDataSource.data.length <= 1) {
      this._snackBar.open(this.translate.instant('You cannot delete the last Super Admin user.'), this.translate.instant('Okay'), {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000
      });
      return;
    }
    const isSelf = this.isCurrentUser(record.id);
    if (record.id === null) {
      return;
    }
    this.roleService.getUserRoleCount(record.id).subscribe({
      next: (response) => {
        const roleCount = response?.data ?? 0;
        let warningMessageKey = 'You will remove this user from the role. Do you want to continue?';
        if (isSelf) {
          warningMessageKey = 'You will lose access to this role. Do you want to continue?';
        } else if (roleCount <= 1) {
          warningMessageKey = 'This user will have no roles and will lose access. Do you want to continue?';
        }
        const dialogRef = this.dialog.open(DeleteDialogComponent, {
          data: {
            title: this.translate.instant('Confirm'),
            message: this.translate.instant(warningMessageKey),
            confirmText: this.translate.instant('Confirm'),
            cancelText: this.translate.instant('Cancel')
          }
        });
        dialogRef.afterClosed().subscribe(result => {
          if (!result?.delete) {
            return;
          }
          console.log('Deleting assigned user record:', record);
          this.roleService.deleteAssignedUser(record.id, record.roleId).subscribe({
            next: (deleteResponse) => {
              if (deleteResponse.isSuccess) {
                this.fetchAssignedUserbyId(this.selectedRoleName!.toString());
              } else {
                this._snackBar.open(this.translate.instant(deleteResponse.message), this.translate.instant('Okay'), {
                  horizontalPosition: 'right',
                  verticalPosition: 'top', duration: 3000,
                });
              }
            },
            error: (error) => {
              this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
                horizontalPosition: 'right',
                verticalPosition: 'top', duration: 3000,
              });
            }
          });
        });
      },
      error: (error) => {
        this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      }
    });
  }

  private isCurrentUser(userId: number | null): boolean {
    if (userId === null) {
      return false;
    }
    if (this.currentUserId !== null) {
      return Number(userId) === Number(this.currentUserId);
    }
    try {
      const saved = localStorage.getItem('currentUser');
      if (!saved) return false;
      const parsed = JSON.parse(saved);
      const storedId = parsed?.user?.userId ?? parsed?.userId ?? null;
      if (storedId === null || storedId === undefined) return false;
      return Number(userId) === Number(storedId);
    } catch {
      return false;
    }
  }

  getAssignedUserDeleteTooltip(): string {
    if (!this.selectedRoleName) {
      return this.translate.instant('Delete');
    }
    const selectedRoleId = Number(this.selectedRoleName);
    const selectedRole = this.records.find(r => r.roleId === selectedRoleId);
    if (selectedRole && this.isSuperAdminRole(selectedRole.roleName) && this.assignedUserDataSource.data.length <= 1) {
      return this.translate.instant('You cannot delete the last Super Admin user');
    }
    return this.translate.instant('Delete');
  }

  fetchAssignedUserbyId(roleId: string) {
    this.isLoading = true;
    this.roleService.getAssignedUserbyId(parseInt(roleId)).subscribe({
      next: (response) => {
        this.assignedUserDataSource.data = response.data.map((item: any) => ({
          id: item.userId,
          roleId: item.roleId,
          displayName: item.userName,
          loginId: item.loginId,
          email: item.email,
          entityName: item.entityName,
          mobileNo: item.mobileNo
        }));
        this.assignedUserData = this.assignedUserDataSource.data;
        if (this.assignedUserDataSource.data.length === 0) {
          this.recordMessage = this.translate.instant('No User Assigned to this role');
        }

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

  @HostListener('document:click', ['$event.target'!])
  onClickOutside(targetElement: HTMLElement) {
    const dropdown = document.querySelector('.dropdown-toggle');
    const menu = document.querySelector('.dropdown-menu');

    if (!dropdown?.contains(targetElement) && !menu?.contains(targetElement)) {
      this.closeMenu();
    }
  }

  fetchRoleList() {
    this.isLoading = true;
    this.roleService.getRoleList().subscribe({
      next: (response) => {
        this.dataSource.data = response.data.map((item: any) => ({
          roleId: item.roleId,
          roleName: item.roleName,
          roleDesc: item.roleDesc,
          userCount: item.userCount
        }));
        this.records = response.data;
        if (this.records.some(r => this.isSuperAdminRole(r.roleName))) {
          this.currentUserRank = Math.max(this.currentUserRank, 3);
        }
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
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

  fetchUsersList() {
    this.userService.getUsersList().subscribe({
      next: (response) => {
        this.userdataSource.data = response.data.map((item: any) => ({
          id: item.userId,
          firstName: item.firstName,
          lastName: item.lastName,
          email: item.email,
          mobileNo: item.mobileNo,
          userPicture: item.userPicture,
          userProfileFile: item.userProfileFile,
          tenantId: item.tenantId,
          selected: false,
          entitylocation: item.entitylocation,
          displayName: item.displayName,
        }));
        this.userRecords = response.data;
      },
      error: (error) => {
        this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      },
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value.trim().toLowerCase();
    if (this.activeTab === 'role') {
      this.dataSource.filter = filterValue;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    } else {
      this.assignedUserDataSource.filter = filterValue;
      this.assignedUserDataSource.paginator = this.paginator;
      this.assignedUserDataSource.sort = this.sort;
    }
  }

  switchTab(tab: string): void {
    this.activeTab = tab;
    const urlTree = this.router.createUrlTree([], {
      relativeTo: this.route,
      queryParams: { tab: tab },
      queryParamsHandling: 'merge'
    });
    this.location.go(urlTree.toString());
    this.updateTitle();

    if (tab === 'role') {
      this.fetchRoleList();
    }
    if (tab === 'assignedUser') {
      if (this.selectedRoleName!) {
        this.fetchAssignedUserbyId(this.selectedRoleName!);
      }
    }
  }
}



