import { ChangeDetectorRef, Component, HostListener, inject, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { GroupService } from '../../../core/services/security/group.service';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { UserService } from '../../../core/services/security/user.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RolesService } from '../../../core/services/setting/role.service';
import { UtilityService } from '../../../core/services/utility/utils';
import { NgForm } from '@angular/forms';

export interface GroupRecord {
  groupId: number | null;
  groupName: string;
  groupDesc: string;
}

export interface AssignedUserRecord {
  userId: number | null;
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
export class GroupComponent implements OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  searchTerm: string = '';
  searchGroup: string = '';
  menuVisible = false;
  formVisible = false;
  isEditMode = false;
  displayedColumns: string[] = ['groupName', 'groupDesc', 'actions'];
  assignedUserColumns: string[] = ['userName', 'email', 'mobileNo', 'actions'];

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
  activeTab: string = 'group';
  selectedGroupName: string | null = null;
  filteredGroupNames: UserRecord[] = [];
  requestBody: { id: number | null; groupId: string | null } = { id: null, groupId: null };
  private _snackBar = inject(MatSnackBar);
  UnassignedUser: boolean = false;
  recordMessage: string = 'Please select group to view assigned user list';
  constructor(private groupService: GroupService, private utilityService: UtilityService, private cdr: ChangeDetectorRef, private userService: UserService, private rolesService: RolesService) { }

  ngOnInit(): void {
    this.fetchGroupList();
    this.fetchUsersList();
  }

  getAddPermission(): boolean {
    return (this.activeTab === 'group' && this.hasPermission(49)) ||
      (this.activeTab === 'assignedUser' && this.hasPermission(45));
  }

  hasPermission(roleId: number): boolean {
    return this.rolesService.hasPermission(roleId);
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
            this.fetchGroupList();
            this.closeForm();
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

      selectedUsers.forEach(user => {
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
    this.isEditMode = true;
    this.formVisible = true;
    this.formData = { ...record };
  }

  deleteRecord(record: GroupRecord) {
    if (confirm(`Are you sure you want to delete Group: "${record.groupName}"?`)) {
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
    }
  }

  onSelect(event: any): void {
    this.selectedGroupName = event.target.value; // Set selected group name
    if (this.selectedGroupName) {
      this.requestBody.groupId = this.selectedGroupName;
      this.fetchAssignedUserbyId(this.selectedGroupName); // Fetch assigned users based on selected group
    }
  }

  deleteAssignedUserRecord(record: AssignedUserRecord) {
    if (confirm(`Are you sure you want to delete Assigned User: "${record.userName}"?`)) {
      this.groupService.deleteAssignedUser(record.userId,record.groupId).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.fetchAssignedUserbyId(this.selectedGroupName!.toString());
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
        }
      });
    }
  }

  fetchAssignedUserbyId(groupId: string) {
    this.groupService.getAssignedUserbyId(parseInt(groupId)).subscribe({
      next: (response) => {
        this.assignedUserDataSource.data = response.data.map((item: any) => ({
          id:item.id,
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
        setTimeout(() => {
          this.assignedUserDataSource.paginator = this.paginator;
          this.assignedUserDataSource.sort = this.sort;  
        }, 700);
       
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
      },
    });
  }

  @HostListener('document:click', ['$event.target'])
  onClickOutside(targetElement: HTMLElement) {
    const dropdown = document.querySelector('.dropdown-toggle');
    const menu = document.querySelector('.dropdown-menu');

    if (!dropdown?.contains(targetElement) && !menu?.contains(targetElement)) {
      this.closeMenu();
    }
  }

  fetchGroupList() {
    this.groupService.getGroupList().subscribe({
      next: (response) => {
        this.dataSource.data = response.data.map((item: any) => ({
          groupId: item.groupId,
          groupName: item.groupName,
          groupDesc: item.groupDesc
        }));
        this.records = response.data;
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000,
        });
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
