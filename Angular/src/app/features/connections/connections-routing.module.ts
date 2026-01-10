import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ConnectionsComponent } from './connections.component';
import { IntegrationComponent } from './integration/integration.component';
import { MappingComponent } from './mapping/mapping.component';
import { roleGuard } from '../../core/guards/role/role.guard';

const routes: Routes = [
  { path: '', redirectTo: 'integration', pathMatch: 'full' },
  { path: '', component: ConnectionsComponent },
  { path: 'integration', component: IntegrationComponent,canActivate: [roleGuard], data: { requiredRoleId: 7 } },
  { path: 'mapping', component: MappingComponent,canActivate: [roleGuard], data: { requiredRoleId: 6 } }
];


@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ConnectionsRoutingModule { }
