import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UseractivityDirective } from './track-activity/useractivity.directive';
import { HttpClientModule } from '@angular/common/http';
import { TopBarComponent } from './components/top-bar/top-bar.component';
import { TabBarComponent } from './components/tab-bar/tab-bar.component';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';



@NgModule({
  declarations: [
    UseractivityDirective
  ],
  imports: [
    CommonModule,
    HttpClientModule,
    MatIconModule,
    FormsModule,
    TopBarComponent,
    TabBarComponent
  ],
  exports: [
    UseractivityDirective,
    TopBarComponent,
    TabBarComponent
  ]
})
export class SharedModule { }
