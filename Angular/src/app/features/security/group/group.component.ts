import { AfterViewInit, ChangeDetectorRef, Component, HostListener, inject, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { GroupService } from '../../../core/services/security/group.service';
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
export interface GroupRecord {
  groupId: number | null;
  groupName: string;
  groupDesc: string;
  userCount?: number;
}

export interface AssignedUserRecord {
  id: number | null;
  groupId: number | null;
  userName: string;
  loginId:number;
  email: string;
  entityName:string;
}

export interface UserRecord{
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
  groupId: string | null;
}

@Component({
  selector: 'app-group',
  standalone: false,
  templateUrl: './group.component.html',
  styleUrl: './group.component.scss'
})
export class GroupComponent implements OnInit,AfterViewInit {
  private readonly superAdminGroupName = 'Super Admin';
  private readonly adminGroupName = 'Admin';
  private readonly userGroupName = 'User';

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  searchTerm: string = '';
  searchGroup: string = '';
  menuVisible = false;
  formVisible = false;
  isEditMode = false;
  displayedColumns: string[] = ['groupName', 'groupDesc', 'actions'];
  assignedUserColumns: string[] = ['userName', 'email', 'mobileNo', 'actions'];
Rank: any = Rank;
  records: GroupRecord[] = [];
  userRecords: UserRecord[] = [];
  assignedUserData: AssignedUserRecord[] = [];

  dataSource = new MatTableDataSource<GroupRecord>(this.records);
  userdataSource = new MatTableDataSource<UserRecord>(this.userRecords);
  assignedUserDataSource = new MatTableDataSource<AssignedUserRecord>(this.assignedUserData);

  formData: GroupRecord = {
    groupId: 0,
    groupName: '',
    groupDesc: '',
  };
  activeTab?: string = 'group';
  selectedGroupName: string | null = null;
  filteredGroupNames: UserRecord[] = [];
  requestBody: { id: number | null; groupId: string | null } = { id: null, groupId: null };
  private _snackBar = inject(MatSnackBar);
  UnassignedUser: boolean = false;
  recordMessage: string = 'Please select group to view assigned user list';
  isLoading: boolean = false;
  isDownloading: boolean = false
  isUploading: boolean = false
  message: string = "Loading data, please wait...";
  currentUserGroups: string[] = [];
  currentUserRank: number = 0;
  currentUserId: number | null = null;

  constructor(
    private groupService: GroupService,
    private utilityService: UtilityService,
    private cdr: ChangeDetectorRef,
    private userService: UserService,
    private PermissionsService: PermissionsService,
    private authService: AuthService,
    private userprofileService: UserprogileService,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.fetchGroupList();
    this.fetchUsersList();
    this.loadCurrentUserGroups();
  }
  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  getAddPermission(): boolean {
    return (this.activeTab === 'group' && this.hasPermission('49')) ||
      (this.activeTab === 'assignedUser' && this.hasPermission('45'));
  }

  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }

  canEditGroup(record: GroupRecord): boolean {
    return this.hasPermission('Permissions.Group.Edit');
  }

  canDeleteGroup(record: GroupRecord): boolean {
    if (!this.hasPermission('Permissions.Group.Delete')) {
      return false;
    }
    if (this.isSuperAdminGroup(record.groupName)) {
      return this.currentUserRank === 3 && (record.userCount ?? 0) > 1;
    }
    return true;
  }

  canDeleteAssignedUser(): boolean {
    return this.hasPermission('Permissions.UserGroup.Delete');
  }

  private isSuperAdminGroup(groupName: string): boolean {
    return groupName?.toLowerCase() === this.superAdminGroupName.toLowerCase();
  }

  private canManageSelectedGroup(): boolean {
    if (!this.selectedGroupName) {
      return false;
    }
    const selectedGroupId = Number(this.selectedGroupName);
    const groupName = this.records.find(g => g.groupId === selectedGroupId)?.groupName ?? '';
    if (this.isSuperAdminGroup(groupName)) {
      if (this.hasPermission('Permissions.UserGroup.Delete')) {
        return true;
      }
      return this.currentUserRank === 3;
    }
    return true;
  }

  private loadCurrentUserGroups(): void {
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
                  const groups = response?.data?.groups ?? [];
                  this.currentUserGroups = groups.map((g: any) => g.groupName).filter(Boolean);
                  this.currentUserRank = this.getHighestRank(this.currentUserGroups);
                },
                error: () => {
                  this.currentUserGroups = [];
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
          const groups = response?.data?.groups ?? [];
          this.currentUserGroups = groups.map((g: any) => g.groupName).filter(Boolean);
          this.currentUserRank = this.getHighestRank(this.currentUserGroups);
        },
        error: () => {
          this.currentUserGroups = [];
          this.currentUserRank = 0;
        }
      });
    });
  }

  private getHighestRank(groupNames: string[]): number {
    let highest = 0;
    for (const name of groupNames) {
      const rank = this.getRank(name);
      if (rank > highest) {
        highest = rank;
      }
    }
    return highest;
  }

  private getRank(groupName: string): number {
    if (groupName?.toLowerCase() === this.superAdminGroupName.toLowerCase()) {
      return 3;
    }
    if (groupName?.toLowerCase() === this.adminGroupName.toLowerCase()) {
      return 2;
    }
    if (groupName?.toLowerCase() === this.userGroupName.toLowerCase()) {
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

      this._snackBar.open('Please fill in the required fields.', 'Close', {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000
      });
      return;
    }

    if (this.activeTab === 'group') {

      if (this.isEditMode) {
        this.groupService.updateUserDetails(this.formData).subscribe({
          next: (response) => {
            this.fetchGroupList();
            this.closeForm();
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top',
              duration: 3000
            });
          }
        });

      } else {
        this.groupService.addGroup(this.formData).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.fetchGroupList();
              this.closeForm();
            }
            this._snackBar.open(response.message, 'Okay', {
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

      if (!this.selectedGroupName) {
        this._snackBar.open('Please select Group', 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 3000
        });
        return;
      }

      const selectedUsers = this.filteredGroupNames.filter(u => u.selected);

      if (selectedUsers.length === 0) {
        this._snackBar.open('Please select at least one user.', 'Okay', {
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
          item.id === user.id && item.groupId === Number(this.selectedGroupName)
        );
        if (isAssigned) {
          alreadyAssigned.push(user.displayName ?? user.email);
          return false;
        }
        return true;
      });

      if (alreadyAssigned.length > 0) {
        const selectedGroupId = Number(this.selectedGroupName);
        const groupName =
          this.records.find(g => g.groupId === selectedGroupId)?.groupName ??
          this.selectedGroupName ??
          'selected group';

        this._snackBar.open(
          `User already present in group: ${groupName}`,
          'Okay',
          { horizontalPosition: 'right', verticalPosition: 'top', duration: 4000 }
        );
      }

      if (usersToAssign.length === 0) {
        return;
      }

      usersToAssign.forEach(user => {
        const payload = {
          groupId: this.selectedGroupName,
          userId: user.id
        };
        console.log('Assigning user with payload:', payload);

        this.groupService.addUsers(payload).subscribe({
          next: (response) => {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top',
              duration: 3000
            });

            completedCount++;

            if (completedCount === selectedUsers.length) {
              this.fetchAssignedUserbyId(String(this.selectedGroupName));
              this.closeForm();
              this.searchGroup = '';
              this.filteredGroupNames = [];
              this.userRecords.forEach(u => u.selected = false);
            }
          },
          error: (err) => console.error('Error assigning user:', err)
        });
      });

      return;
    }

    this.filteredGroupNames = [];
  }
  toggleMenu() {
    this.menuVisible = !this.menuVisible;
  }

  closeMenu() {
    this.menuVisible = false;
  }

  openForm(): void {
    if (this.activeTab == 'group') {
      this.formVisible = true;
      this.isEditMode = false;
      this.formData = {
        groupId: 0,
        groupName: '',
        groupDesc:''
      };
    }else {
      this.UnassignedUser = true;
      this.isEditMode = false;
    }
  }

  addRecord() {
    if (this.activeTab === 'group') {
      if (this.isEditMode) {
        this.groupService.updateUserDetails(this.formData).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.fetchGroupList();
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
        this.groupService.addGroup(this.formData).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.fetchGroupList();
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
    } else if (this.activeTab === 'assignedUser') {
      const isUserAssigned = this.assignedUserDataSource.data.some((item: any) =>
        item.id === this.requestBody.id && item.groupId === parseInt(this.requestBody.groupId!)
      );
      if (isUserAssigned) {
        this._snackBar.open('User is already assigned to this group.', 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
        return;
      }

      if(this.selectedGroupName === null){
        this._snackBar.open('Please select Group', 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
        this.closeForm();
        return;
      }

      this.groupService.addUsers(this.requestBody).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.fetchAssignedUserbyId(this.selectedGroupName!.toString());
            this.closeForm();
            this.searchGroup = '';
            this.filteredGroupNames = []; // Clear filtered list
            this.userRecords.forEach(user => user.selected = false); // Reset selected state
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000,
            });
            //this.fetchAssignedUserbyId(this.selectedGroupName!.toString());
          } else {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000,
            });
          }
        },
        error: (error) => console.error('Error adding assigned user:', error),
      });
    }
    this.filteredGroupNames = [];
  }

  trackByUserId(index:number,group:any): number {
    return group.userId;
  }

  filterGroupNames() {
    if (!this.userRecords || this.userRecords.length === 0) {
      return;
    }
    const query = this.searchGroup.toLowerCase().trim();
    if (query) {
      this.filteredGroupNames = this.userRecords.filter(group =>
        group.displayName.toLowerCase().startsWith(query)
      );
    } else {
      this.filteredGroupNames = [];
    }
    this.cdr.detectChanges();
  }

  onGroupSelectionChange(id: number | null) {
    this.requestBody.id = id;
    this.requestBody.groupId = this.selectedGroupName;
  }

  closeForm() {
    this.formVisible = false;
    if (this.activeTab === 'group') {
      this.formData = { groupId: 0, groupName: '', groupDesc: '' };
    }
    this.searchGroup = '';
    this.filteredGroupNames = []; // Clear filtered list
    this.userRecords.forEach(user => user.selected = false); // Reset selected state
    this.isEditMode = false;
    this.UnassignedUser=false;
  }

  editRecord(record: GroupRecord) {
    if (!this.hasPermission('Permissions.Group.Edit')) {
      this._snackBar.open('You do not have permission to edit groups.', 'Okay', {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000
      });
      return;
    }
    if (this.isSuperAdminGroup(record.groupName)) {
      this._snackBar.open('You can not edit this group.', 'Okay', {
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

  deleteRecord(record: GroupRecord) {
    if (!this.hasPermission('Permissions.Group.Delete')) {
      this._snackBar.open('You do not have permission to delete groups.', 'Okay', {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000
      });
      return;
    }
    if (this.isSuperAdminGroup(record.groupName)) {
      this._snackBar.open('You cannot delete Super Admin group.', 'Okay', {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000
      });
      return;
    }
    const dialogRef = this.dialog.open(DeleteDialogComponent, {
      data: {
        title: 'Confirm',
        message: `Are you sure you want to delete Group: "${record.groupName}"?`,
        confirmText: 'Confirm',
        cancelText: 'Cancel'
      }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (!result?.delete) {
        return;
      }
      this.groupService.deleteGroupWithId(Number(record.groupId)).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            console.log(`Group with ID ${record.groupId} deleted successfully.`);
            this.fetchGroupList();
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
    });
  }

  onSelect(event: any): void {
    this.selectedGroupName = event.target.value; // Set selected group name
    if (this.selectedGroupName) {
      this.requestBody.groupId = this.selectedGroupName;
      this.fetchAssignedUserbyId(this.selectedGroupName); // Fetch assigned users based on selected group
    }
  }

  deleteAssignedUserRecord(record: AssignedUserRecord) {
    if (!this.canManageSelectedGroup()) {
      this._snackBar.open('You can not delete users from this group.', 'Okay', {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000
      });
      return;
    }
    const selectedGroupId = Number(this.selectedGroupName);
    const selectedGroup = this.records.find(g => g.groupId === selectedGroupId);
    if (selectedGroup && this.isSuperAdminGroup(selectedGroup.groupName) && this.assignedUserDataSource.data.length <= 1) {
      this._snackBar.open('You cannot delete the last Super Admin user.', 'Okay', {
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
    this.groupService.getUserGroupCount(record.id).subscribe({
      next: (response) => {
        const groupCount = response?.data ?? 0;
        let warningMessage = 'You will remove this user from the group. Do you want to continue?';
        if (isSelf) {
          warningMessage = 'You will lose access to this group. Do you want to continue?';
        } else if (groupCount <= 1) {
          warningMessage = 'This user will have no groups and will lose access. Do you want to continue?';
        }
        const dialogRef = this.dialog.open(DeleteDialogComponent, {
          data: {
            title: 'Confirm',
            message: warningMessage,
            confirmText: 'Confirm',
            cancelText: 'Cancel'
          }
        });
        dialogRef.afterClosed().subscribe(result => {
          if (!result?.delete) {
            return;
          }
          console.log('Deleting assigned user record:', record);
          this.groupService.deleteAssignedUser(record.id,record.groupId).subscribe({
            next: (deleteResponse) => {
              if (deleteResponse.isSuccess) {
                this.fetchAssignedUserbyId(this.selectedGroupName!.toString());
              } else {
                this._snackBar.open(deleteResponse.message, 'Okay', {
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
            }
          });
        });
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
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
    if (!this.selectedGroupName) {
      return 'Delete';
    }
    const selectedGroupId = Number(this.selectedGroupName);
    const selectedGroup = this.records.find(g => g.groupId === selectedGroupId);
    if (selectedGroup && this.isSuperAdminGroup(selectedGroup.groupName) && this.assignedUserDataSource.data.length <= 1) {
      return 'You cannot delete the last Super Admin user';
    }
    return 'Delete';
  }

  fetchAssignedUserbyId(groupId: string) {
    this.isLoading = true;
    this.groupService.getAssignedUserbyId(parseInt(groupId)).subscribe({
      next: (response) => {
        this.assignedUserDataSource.data = response.data.map((item: any) => ({
          id:item.userId,
          groupId:item.groupId,
          displayName: item.userName,
          loginId: item.loginId,
          email: item.email,
          entityName: item.entityName,
          mobileNo: item.mobileNo
        }));
        this.assignedUserData = this.assignedUserDataSource.data;
        if(this.assignedUserDataSource.data.length === 0){
          this.recordMessage = "No User Assigned to this group";
        }
     
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

  @HostListener('document:click', ['$event.target'!])
  onClickOutside(targetElement: HTMLElement) {
    const dropdown = document.querySelector('.dropdown-toggle');
    const menu = document.querySelector('.dropdown-menu');

    if (!dropdown?.contains(targetElement) && !menu?.contains(targetElement)) {
      this.closeMenu();
    }
  }

  fetchGroupList() {
    this.isLoading = true;
    this.groupService.getGroupList().subscribe({
      next: (response) => {
        this.dataSource.data = response.data.map((item: any) => ({
          groupId: item.groupId,
          groupName: item.groupName,
          groupDesc: item.groupDesc,
          userCount: item.userCount
        }));
        this.records = response.data;
        if (this.records.some(g => this.isSuperAdminGroup(g.groupName))) {
          this.currentUserRank = Math.max(this.currentUserRank, 3);
        }
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
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
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      },
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value.trim().toLowerCase();
    if (this.activeTab === 'group') {
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
    if (tab === 'group') {
      this.fetchGroupList();
    }
    if (tab === 'assignedUser') {
      if(this.selectedGroupName!){
        this.fetchAssignedUserbyId(this.selectedGroupName!);
      }
    }
  }
}



