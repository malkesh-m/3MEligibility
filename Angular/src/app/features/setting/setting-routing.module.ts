import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SettingComponent } from './setting.component';
import { EntityComponent } from './entity/entity.component';
//import { FactorComponent } from './factor/factor.component';

import { ProductComponent } from './product/product.component';
import { ProductListComponent } from './product/product-list/product-list.component';
import { ParametersComponent } from './parameters/parameters.component';
import { RulesComponent } from './conditions/rules/rules.component';
import { ProductCardsComponent } from './conditions/product-cards/product-cards.component';
import { CardsComponent } from './conditions/cards/cards.component';
import { ListsComponent } from './lists/lists.component';
import { FactorsComponent } from './factor/factor.component';
import { MakerCheckerComponent } from './maker-checker/maker-checker.component';
import { MakerCheckerConfigComponent } from './maker-checker-config/maker-checker-config.component';
import { BulkImportComponent } from './bulk-import/bulk-import.component';
import { MakerCheckerHistoryComponent } from './maker-checker/maker-checker-history/maker-checker-history.component';
import { ExceptionManagementComponent } from './conditions/exception-management/exception-management.component';
import { roleGuard } from '../../core/guards/role/role.guard';
import { UnauthorizedComponent } from './unauthorized/unauthorized.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { AuditLogComponent } from './Audit/audit-log/audit-log.component';
import { LogsComponent } from './Audit/logs/logs.component';

const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: '', component: SettingComponent },
  { path: 'entity', component: EntityComponent,canActivate: [roleGuard], data: { requiredRoleId: 9 } },
  { path: 'factors', component: FactorsComponent, canActivate: [roleGuard], data: { requiredRoleId: 8 } },
  { path: 'parameters', component: ParametersComponent, canActivate: [roleGuard], data: { requiredRoleId: 10 } },
  { path: 'lists', component: ListsComponent },

  {
    path: 'products', component: ProductComponent,
    canActivate: [roleGuard], data: { requiredRoleId: 51 },
    children: [
      { path: '', redirectTo: 'list', pathMatch: 'full' },
      { path: 'list', component: ProductListComponent, canActivate: [roleGuard], data: { requiredRoleId: 51 } },
      { path: 'info', component: ProductListComponent, canActivate: [roleGuard], data: { requiredRoleId: 51 } },
      { path: 'details', component: ProductListComponent, canActivate: [roleGuard], data: { requiredRoleId: 51 } },
    ],
  },
  { path: 'rules', component: RulesComponent, canActivate: [roleGuard], data: { requiredRoleId: 52 } },
  { path: 'product-cards', component: ProductCardsComponent, canActivate: [roleGuard], data: { requiredRoleId: 54 } },
  { path: 'cards', component: CardsComponent, canActivate: [roleGuard], data: { requiredRoleId: 53 } },

  { path: 'checker', component: MakerCheckerComponent },
  { path: 'history', component: MakerCheckerHistoryComponent },
  { path: 'maker-checker-config', component: MakerCheckerConfigComponent },
  { path: 'bulk-import', component: BulkImportComponent },
  { path: 'exception', component: ExceptionManagementComponent },
  { path: 'unauthorized', component: UnauthorizedComponent }, // Unauthorized Page Route
  { path: 'dashboard', component: DashboardComponent },
  { path: 'audit-log', component: AuditLogComponent },
  { path: 'log', component: LogsComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SettingRoutingModule { }   
