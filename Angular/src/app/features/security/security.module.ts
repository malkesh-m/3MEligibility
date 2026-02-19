import { NgModule } from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { SecurityRoutingModule } from './security-routing.module';
import { SecurityComponent } from './security.component';
import { UserComponent } from './user/user.component';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { CoreModule } from '../../core/core.module';
import { MatTabsModule } from '@angular/material/tabs';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatMenuModule } from '@angular/material/menu';
import { MatSelectModule } from '@angular/material/select';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { QuillModule } from 'ngx-quill';
import { RoleComponent } from './role/role.component';
import { PermissionComponent } from './permission/permission.component';
import { UserprofileComponent } from './userprofile/userprofile.component';
import { TranslateModule } from '@ngx-translate/core';
import { MatTooltipModule } from '@angular/material/tooltip';
import { SharedModule } from '../../shared/shared.module';
import { MatProgressSpinner } from '@angular/material/progress-spinner';

@NgModule({
  declarations: [UserComponent, SecurityComponent, RoleComponent, PermissionComponent, UserprofileComponent],
  imports: [
    CommonModule,
    SecurityRoutingModule,
    FormsModule,
    MatIconModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatTableModule, // Make sure this is here
    MatTabsModule,
    ReactiveFormsModule,
    MatInputModule,
    MatTooltipModule,
    MatPaginatorModule,
    MatSortModule, // Make sure this is here
    MatMenuModule,
    MatSelectModule,
    MatDialogModule,
    MatButtonModule,
    QuillModule.forRoot(),
    TranslateModule,
    SharedModule,
    MatProgressSpinner,
    CoreModule
  ],
})
export class SecurityModule { }
