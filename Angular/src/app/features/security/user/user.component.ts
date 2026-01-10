import {
  Component,
  OnInit,
  HostListener,
  ViewChild,
  AfterViewInit,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common'; // Required for *ngIf, *ngFor
import { FormsModule } from '@angular/forms'; // If you need two-way data binding
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { UserService } from '../../../core/services/security/user.service';
import { MatIcon } from '@angular/material/icon';
import { MatCheckbox, MatCheckboxChange } from '@angular/material/checkbox';
import { TranslateModule } from '@ngx-translate/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RolesService } from '../../../core/services/setting/role.service';
import { EntityService } from '../../../core/services/setting/entity.service';


export interface UserRecord {
  userId: number | null;
  userName: string;
  loginId: string;
  userPassword?: string;
  email: string;
  phone: string;
  userPicture?: string;
  userProfileFile?: string;
  entityId: null|number;
  entitylocation?: string;
  selected: boolean;
  statusName: any;
  entityName: string
  statusId: number;

}

@Component({
  selector: 'app-user',
  standalone: false,
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.scss'],
})
export class UserComponent implements OnInit {
  private _snackBar = inject(MatSnackBar);
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  users: UserRecord[] = [];
  displayedColumns: string[] = [
    'select',
    'userName',
    'loginId',
    'email',
    'phone',
    'statusName',
    'entityName',
    'isSuspended',
    'actions',
  ];
  records: UserRecord[] = [];
  dataSource = new MatTableDataSource<UserRecord>(this.records);
  selectedUser: UserRecord | null = null;
  selectedRows: Set<number> = new Set();
  selectedRowsItem: Set<number> = new Set();
  pageSize: number = 5;
  formVisible = false;
  menuVisible = false;
  isEditMode = false;
  searchTerm: string = '';
  formData: UserRecord = {
    userId: 0,
    userName: '',
    loginId: '',
    userPassword: '',
    email: '',
    phone: '',
    userPicture: '',
    entityId: null,
    entitylocation: '',
    selected: false,
    statusName: '',
    entityName: '',
    statusId: 1
  };
    toastr: any;

  constructor(private userService: UserService,private rolesService:RolesService,private entityService:EntityService) { }

  hasPermission(roleId: number): boolean {
    return this.rolesService.hasPermission(roleId);
  }
  passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{6,}$/;
  ngOnInit() {
    this.fetchUsersList();
    this.fetchEntityList()

  }
  reActivateUser(user: any) {
    const model = {
      userId: user.userId,
      isSuspended: false
      // make sure name matches your ReActivationModel
    };

    this.userService.reActivateUser(model).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this._snackBar.open(res.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
          this.fetchUsersList(); // refresh table
        } else {
          this._snackBar.open(res.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
        }
      },
      error: (res) => {
        this._snackBar.open(res.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    });
  }

  isPasswordValid(): boolean {
    // If in edit mode and password field is disabled, consider it valid
    if (this.isEditMode) {
      return true;
    }

    // For new users, check if password meets all requirements
    const password = this.formData.userPassword;

    if (!password || password.length < 6) {
      return false;
    }

    // Check against your regex pattern
    return this.passwordRegex.test(password);
  }
  toggleMenu() {
    this.menuVisible = !this.menuVisible;
  }

  closeMenu() {
    this.menuVisible = false;
  }

  @HostListener('document:click', ['$event.target'])
  onClickOutside(targetElement: HTMLElement) {
    const dropdown = document.querySelector('.dropdown-toggle');
    const menu = document.querySelector('.dropdown-menu');

    if (!dropdown?.contains(targetElement) && !menu?.contains(targetElement)) {
      this.closeMenu();
    }
  }

  fetchUsersList() {
    this.userService.getUsersList().subscribe({
      next: (response) => {
        console.log('Users Data List: ', response.data);
        this.dataSource.data = response.data.map((item: any) => ({
          userId: item.userId,
          userName: item.userName,
          loginId: item.loginId,
          userPassword: item.userPassword,
          email: item.email,
          phone: item.phone,
          userPicture: item.userPicture,
          userProfileFile: item.userProfileFile,
          entityId: item.entityId,
          selected: false,
          entitylocation: item.entitylocation,
          statusName: item.statusName,
          statusId: item.statusId,
          entityName: item.entityName,
          isSuspended: item.issuspended
        }));
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        console.log(this.dataSource.sort)
        console.log("this.dataSource.sort")

      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      
        console.error('Error fetching users:', error);
      },
    });
  }
   EntityList: any[] = [];
  fetchEntityList() {
    this.entityService.getEntitiesList().subscribe({
      next: (response) => {
        this.EntityList = response.data;

      },
      error: (error) => {
        console.error('Error fetching users:', error);
      },
    });
  }


  getCurrentPageData(): any[] {
    if (!this.paginator) return []; // Return empty array if paginator is undefined
    const startIndex = this.paginator.pageIndex * this.paginator.pageSize;
    const endIndex = startIndex + this.paginator.pageSize;
    return this.dataSource.filteredData.slice(startIndex, endIndex);
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value.trim().toLowerCase();
    this.dataSource.filter = filterValue;
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  toggleSelection(event: MatCheckboxChange, userId: number) {
    if (event.checked) {
      this.selectedRows.add(userId);
    } else {
      this.selectedRows.delete(userId);
    }
  }

  toggleSelectAll(event: MatCheckboxChange) {
    const currentPageData = this.getCurrentPageData(); // Paginated rows
    if (event.checked) {
      currentPageData.forEach((row: any) => this.selectedRows.add(row.userId));
    } else {
      currentPageData.forEach((row: any) => this.selectedRows.delete(row.userId));
    }
  }

  isAllPageSelected(): boolean {
    const currentPageData = this.getCurrentPageData();
    return currentPageData.every((row: any) => this.selectedRows.has(row.userId));
  }

  isSomePageSelected(): boolean {
    const currentPageData = this.getCurrentPageData();
    return currentPageData.some((row: any) => this.selectedRows.has(row.userId)) && !this.isAllPageSelected();
  }

  refreshTable() {
    this.dataSource.data = [...this.dataSource.data];
    this.fetchEntityList()
  }

  addRecord() {
    if (this.isEditMode) {
      // Call update API
      const requestBody = {
        userId: this.formData.userId,
        userName: this.formData.userName,
        loginId: this.formData.loginId,
        userPassword: this.formData.userPassword,
        email: this.formData.email,
        phone: this.formData.phone ?? "",
        statusId: this.formData.statusId??2,
        entityId: this.formData.entityId,
      };
      console.log(requestBody)
      console.log("requestBodyfor edit")

      this.userService.updateUserDetails(requestBody).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
            this.fetchUsersList(); // Refresh the table
            this.closeForm(); // Close the form
          } else {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        },
        error: (error) => {
          this._snackBar.open(error.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
        },
      });
    } else {
      // Add new record

      const requestBody = {
        UserName: this.formData.userName,
        LoginId: this.formData.loginId,
        UserPassword: this.formData.userPassword,
        Email: this.formData.email,
        Phone: this.formData.phone,
        StatusId: this.formData.statusId,
        EntityId: this.formData.entityId,


      };
      console.log(requestBody)
      console.log("requestBodyfor add")
      this.userService.addUsers(requestBody).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
            this.fetchUsersList(); // Refresh the table after adding
            this.closeForm(); // Close the form
          } else {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        },
        error: (error) => {
          this._snackBar.open(error.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
        },
      });
    }
  }

  openForm() {
    this.formVisible = true;
    this.isEditMode = false;
    this.formData = {
      userId: null,
      userName: '',
      loginId: '',
      userPassword: '',
      email: '',
      phone: '',
      entityId: null,
      userPicture: '',
      selected: false,
      entitylocation: '',
      statusName: '',
      entityName: '',
      statusId: 1,

    };
  }

  closeForm() {
    this.formVisible = false;
    this.formData = {
      userId: null,
      userName: '',
      loginId: '',
      userPassword: '',
      email: '',
      phone: '',
      entityId: null,
      userPicture: '',
      selected: false,
      entitylocation: '',
      statusName: '',
      entityName: '',
      statusId: 1,

    };
    this.isEditMode = false;
  }

  editRecord(record: UserRecord) {
    this.isEditMode = true;
    this.formVisible = true;
    this.formData = { ...record };
    console.log(this.formData)
  }

  DeActiveUser() {
    if (this.selectedRows.size === 0) {
      alert('No records selected for DeActivateUser.');
      return;
    }

    if (confirm(`Are you sure you want to DeActivate User ${this.selectedRows.size} records?`)) {
      const userIdsToDelete = Array.from(this.selectedRows);

      this.userService.deActiveMultipleUsers(userIdsToDelete).subscribe({
        next: (response) => {
          this._snackBar.open(response.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
          this.selectedRows.clear(); // Clear selected rows
          this.fetchUsersList()
        },
        error: (error) => {
          this._snackBar.open(error, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
        }
      });
    }
  }
  getCheckboxStatus(): boolean {
    return this.formData?.statusId === 1;
  }

  updateStatus(event: Event): void {
    const isChecked = (event.target as HTMLInputElement)?.checked;
    if (this.formData && isChecked !== undefined && isChecked !== null) {
      this.formData.statusId = isChecked ? 1 : 2;
    }
  }

  deleteRecord(record: UserRecord) {
    if (confirm(`Are you sure you want to delete User: "${record.userName}"?`)) {
      // Call the delete API
      this.userService.deleteUserWithId(Number(record.userId)).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          this.  closeForm() 

            this.fetchUsersList(); // Refresh the table after deletion
          } else {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        },
        error: (error) => {
          this._snackBar.open(error, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
        }
      });
    }
  }
}
