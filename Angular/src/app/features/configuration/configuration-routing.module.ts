import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ConfigurationComponent } from './configuration.component';
import { ExceptionConfigComponent } from './exception-config/exception-config.component';
import { ProductCapComponent } from './product-cap/product-cap.component';
import { ProductCapAmountComponent } from './product-cap-amount/product-cap-amount.component';

const routes: Routes = [
  { path: '', redirectTo: 'exception', pathMatch: 'full' },
  { path: '', component: ConfigurationComponent },
  { path: 'exception', component: ExceptionConfigComponent },
  { path: 'Product-Cap', component: ProductCapComponent },
  { path: 'Product-Cap-Amount', component: ProductCapAmountComponent }
];


@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ConfigurationRoutingModule { }
