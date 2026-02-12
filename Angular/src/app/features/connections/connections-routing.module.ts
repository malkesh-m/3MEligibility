import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ConnectionsComponent } from './connections.component';
import { IntegrationComponent } from './integration/integration.component';
import { MappingComponent } from './mapping/mapping.component';
import { permissionGuard } from '../../core/guards/permission/permission.guard';

const routes: Routes = [
  { path: '', redirectTo: 'integration', pathMatch: 'full' },
  { path: '', component: ConnectionsComponent },
  { path: 'integration', component: IntegrationComponent,canActivate: [permissionGuard], data: { requiredPermissionId: "Permissions.Integration.View"} },
  { path: 'mapping', component: MappingComponent,canActivate: [permissionGuard], data: { requiredPermissionId: 6 } }
];


@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ConnectionsRoutingModule { }


