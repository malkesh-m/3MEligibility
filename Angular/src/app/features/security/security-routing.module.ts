import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SecurityComponent } from './security.component';
import { UserComponent } from './user/user.component';
import { GroupComponent } from './group/group.component';
import { PermissionComponent } from './permission/permission.component';
import { UserprofileComponent } from './userprofile/userprofile.component';
import { permissionGuard } from '../../core/guards/permission/permission.guard';

const routes: Routes = [
  {path: '', redirectTo: 'user', pathMatch: 'full' },
  { path: '', component: SecurityComponent },
  { path: 'user', component: UserComponent},
  { path: 'group', component: GroupComponent},
  { path: 'permission', component: PermissionComponent},
  { path: 'userprofile', component: UserprofileComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SecurityRoutingModule { }


