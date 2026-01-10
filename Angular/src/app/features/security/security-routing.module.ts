import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SecurityComponent } from './security.component';
import { UserComponent } from './user/user.component';
import { GroupComponent } from './group/group.component';
import { RoleComponent } from './role/role.component';
import { UserprofileComponent } from './userprofile/userprofile.component';
import { roleGuard } from '../../core/guards/role/role.guard';

const routes: Routes = [
  {path: '', redirectTo: 'user', pathMatch: 'full' },
  { path: '', component: SecurityComponent },
  { path: 'user', component: UserComponent,canActivate: [roleGuard], data: { requiredRoleId: 70 } },
  { path: 'group', component: GroupComponent,canActivate: [roleGuard], data: { requiredRoleId: 71 }},
  { path: 'role', component: RoleComponent,canActivate: [roleGuard], data: { requiredRoleId: 72 }},
  { path: 'userprofile', component: UserprofileComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SecurityRoutingModule { }
