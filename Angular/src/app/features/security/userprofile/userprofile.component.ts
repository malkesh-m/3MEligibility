import { Component, inject } from '@angular/core';
import { AuthService } from '../../../core/services/auth/auth.service';
import { UserprogileService } from '../../../core/services/security/userprofile.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PermissionsService } from '../../../core/services/setting/permission.service';
import { TranslateService } from '@ngx-translate/core';

export interface personalDetailRecord {
  entityName: string | null,
  statusName: string,
  groups: [],
  userId: number | null,
  userName: string,
  loginId: string,
  userPassword: string,
  email: string,
  phone: string,
  entityId: number | null,
  userPicture: string | null,
  userProfileFile: string | null,
  statusId: number | null,
}

export interface ChangePassRecord {
  userId: number | null,
  currentPassword: string,
  newPassword: string,
  confirmNewPassword: string
}

@Component({
  selector: 'app-userprofile',
  standalone: false,
  templateUrl: './userprofile.component.html',
  styleUrl: './userprofile.component.scss'
})
export class UserprofileComponent {
  private _snackBar = inject(MatSnackBar);
  activeTab: string = 'personalDetails';
  changeEmailformVisible = false;
  cngPassFormVisible = false;
  personalFormVisible = true;
  personalText: string = this.translate.instant('You can view and update some of your information with<br>fileds colored blue');
  changeEmailText: string = this.translate.instant('You can update your current email by write a new one<br>below and click update changes');
  changePasswordText: string = this.translate.instant('You can update your current password by enter your current<br>one then your new one below and click update changes');
  loggedInUser: any = null;
  personalformData: personalDetailRecord = {
    entityName: null,
    statusName: '',
    userId: 0,
    groups: [],
    userName: '',
    loginId: '',
    userPassword: '',
    email: '',
    phone: '',
    entityId: 0,
    userPicture: null,
    userProfileFile: null,
    statusId: 0,
  };
  changePassformData: ChangePassRecord = {
    userId: 0,
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: ''
  };
  groupId: number = 0;
  groupsRecord = [
    {
      groupId: 4,
      groupName: "Super Admin",
      groupDesc: null
    },
    {
      groupId: 5,
      groupName: "Admin",
      groupDesc: null
    }
  ];

  constructor(private authService: AuthService, private userprofileService: UserprogileService, private PermissionsService: PermissionsService, private translate: TranslateService) { }

  ngOnInit() {
    this.authService.currentUser$.subscribe((user) => {
      this.loggedInUser = user;
    });
    this.userprofileService.getUserProfileById(this.loggedInUser.user.userId).subscribe({
      next: (response) => {
        this.personalformData = ({
          entityName: response.data.entityName,
          statusName: response.data.statusName,
          groups: response.data.groups,
          userId: response.data.userId,
          userName: response.data.userName,
          loginId: response.data.loginId,
          userPassword: response.data.userPassword,
          email: response.data.email,
          phone: response.data.phone,
          entityId: response.data.entityId,
          userPicture: response.data.userPicture,
          userProfileFile: response.data.userProfileFile,
          statusId: response.data.statusId
        });
        this.groupsRecord = this.personalformData.groups;
      },
      error: (error) => {
        console.error('Error fetching users:', error);
      },
    });
  }

  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }

  switchTab(tab: string): void {
    this.activeTab = tab;
    this.personalFormVisible = false;
    this.changeEmailformVisible = false;
    this.cngPassFormVisible = false;
    if (this.activeTab === 'personalDetails') {
      this.personalFormVisible = true;
    } else if (this.activeTab === 'changeEmail') {
      this.changeEmailformVisible = true;
    } else if (this.activeTab === 'changePassword') {
      this.cngPassFormVisible = true;
    }
  }

  updateUser() {
    if (this.activeTab === 'personalDetails' || this.activeTab === 'changeEmail') {
      this.userprofileService.updateUsers(this.personalformData).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          } else {
            this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        },
        error: (error) => console.error('Error adding assigned user:', error),
      });

    }
    if (this.activeTab === 'changePassword') {
      this.changePassformData.userId = this.personalformData.userId;
      this.userprofileService.changePassword(this.changePassformData).subscribe({
        next: (response2) => {
          if (response2.isSuccess) {
            this._snackBar.open(this.translate.instant(response2.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          } else {
            this._snackBar.open(this.translate.instant(response2.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        },
        error: (error) => console.log(error),
      });
    }
  }
}



