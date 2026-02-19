import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatMenuModule } from '@angular/material/menu';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { QuillModule } from 'ngx-quill';
import { TranslateModule } from '@ngx-translate/core';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ConfigurationComponent } from './configuration.component';
import { ConfigurationRoutingModule } from './configuration-routing.module';
import { ExceptionConfigComponent } from './exception-config/exception-config.component';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { ProductCapComponent } from './product-cap/product-cap.component';
import { ProductCapAmountComponent } from './product-cap-amount/product-cap-amount.component';
import { SharedModule } from '../../shared/shared.module';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CoreModule } from '../../core/core.module';


@NgModule({
  declarations: [
    ConfigurationComponent,
    ExceptionConfigComponent,
    ProductCapComponent,
    ProductCapAmountComponent,
  ],
  imports: [
    CommonModule,
    ConfigurationRoutingModule,
    FormsModule,
    MatTooltipModule,
    MatIconModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatTableModule, // Make sure this is here
    MatTabsModule,
    ReactiveFormsModule,
    MatInputModule,
    MatPaginatorModule,
    MatSortModule, // Make sure this is here
    MatMenuModule,
    MatSelectModule,
    MatDialogModule,
    MatButtonModule,
    QuillModule,
    TranslateModule,
    MatSlideToggleModule,
    SharedModule,
    MatProgressSpinnerModule,
    CoreModule
  ]
})
export class ConfigurationModule { }
