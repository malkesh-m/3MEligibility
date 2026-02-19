import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDividerModule } from '@angular/material/divider';

import { SettingRoutingModule } from './setting-routing.module';
import { SettingComponent } from './setting.component';
import { EntityComponent } from './entity/entity.component';
//import { FactorComponent } from './factor/factor.component';
import { ProductComponent } from './product/product.component';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ProductListComponent } from './product/product-list/product-list.component';
import { CoreModule } from '../../core/core.module';
import { MatTabsModule } from '@angular/material/tabs';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatMenuModule } from '@angular/material/menu';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogModule } from '@angular/material/dialog';
import { DeleteDialogComponent } from '../../core/components/delete-dialog/delete-dialog.component';
import { ParametersComponent } from './parameters/parameters.component';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { QuillModule } from 'ngx-quill';
import { RulesComponent } from './conditions/rules/rules.component';
import { CardsComponent } from './conditions/cards/cards.component';
import { ProductCardsComponent } from './conditions/product-cards/product-cards.component';
import { ListsComponent } from './lists/lists.component';
import { TranslateModule } from '@ngx-translate/core';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FactorsComponent } from './factor/factor.component';
import { MakerCheckerComponent } from './maker-checker/maker-checker.component';
import { MakerCheckerConfigComponent } from './maker-checker-config/maker-checker-config.component';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { BulkImportComponent } from './bulk-import/bulk-import.component';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner'
import { MakerCheckerDetailsDialogComponent } from './maker-checker/maker-checker-details-dialog/maker-checker-details-dialog.component';
import { MakerCheckerHistoryComponent } from './maker-checker/maker-checker-history/maker-checker-history.component';
import { ExceptionManagementComponent } from './conditions/exception-management/exception-management.component';
import { UnauthorizedComponent } from './unauthorized/unauthorized.component';
import { ExceptionComponent } from './conditions/exception-management/exception/exception.component';
import { ExceptionFormComponent } from './conditions/exception-management/exception-form/exception-form.component';
import { ExceptionParameterComponent } from './conditions/exception-management/exception-parameter/exception-parameter.component';
import { ExceptionParameterFormComponent } from './conditions/exception-management/exception-parameter-form/exception-parameter-form.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ComputedValuesDialogComponent } from './parameters/computed-values-dialog/computed-values-dialog.component';
import { AuditLogComponent } from './Audit/audit-log/audit-log.component';
import { LogsComponent } from './Audit/logs/logs.component';
import { SharedModule } from '../../shared/shared.module';
import { ApiDetailsDialogComponent } from './dashboard/api-details-dialog/api-details-dialog.component';
import { ParameterBindingComponent } from './parameter-binding/parameter-binding.component';



@NgModule({
  declarations: [
    SettingComponent,
    EntityComponent,
    DashboardComponent,
    FactorsComponent,
    ProductComponent,
    ProductListComponent,
    ParametersComponent,
    RulesComponent,
    CardsComponent,
    ProductCardsComponent,
    ListsComponent,
    MakerCheckerComponent,
    MakerCheckerConfigComponent,
    MakerCheckerDetailsDialogComponent,
    MakerCheckerHistoryComponent,
    BulkImportComponent,
    ExceptionManagementComponent,
    UnauthorizedComponent,
    ExceptionComponent,
    ExceptionFormComponent,
    ExceptionParameterComponent,
    ExceptionParameterFormComponent,
    ComputedValuesDialogComponent,
    AuditLogComponent,
    LogsComponent,
    ApiDetailsDialogComponent,
    ParameterBindingComponent
  ],
  imports: [
    MatDividerModule
    ,
    CommonModule,
    CoreModule,
    MatTooltipModule,
    SettingRoutingModule,
    FormsModule,
    MatIconModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatTableModule,
    MatTabsModule,
    ReactiveFormsModule,
    MatInputModule,
    MatPaginatorModule,
    MatSortModule,
    MatMenuModule,
    MatSelectModule,
    MatDialogModule,
    MatButtonModule,
    QuillModule,
    TranslateModule, MatSlideToggleModule,
    MatProgressSpinnerModule,
    SharedModule
  ],

})
export class SettingModule { }
