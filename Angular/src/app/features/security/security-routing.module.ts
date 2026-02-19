import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SecurityComponent } from './security.component';
import { UserComponent } from './user/user.component';
import { RoleComponent } from './role/role.component';
import { PermissionComponent } from './permission/permission.component';
import { UserprofileComponent } from './userprofile/userprofile.component';
import { permissionGuard } from '../../core/guards/permission/permission.guard';

const routes: Routes = [
  { path: '', redirectTo: 'user', pathMatch: 'full', data: { title: 'Security' } },
  { path: '', component: SecurityComponent },
  { path: 'user', component: UserComponent, data: { title: 'User Management' } },
  { path: 'role', component: RoleComponent, data: { title: 'Roles' } },
  { path: 'permission', component: PermissionComponent, data: { title: 'Permissions' } },
  { path: 'userprofile', component: UserprofileComponent, data: { title: 'User Profile' } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SecurityRoutingModule { }


