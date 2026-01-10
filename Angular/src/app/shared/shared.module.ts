import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UseractivityDirective } from './track-activity/useractivity.directive';
import { HttpClientModule } from '@angular/common/http';



@NgModule({
  declarations: [
    UseractivityDirective
  ],
  imports: [
    CommonModule,
    HttpClientModule 
  ],
  exports: [UseractivityDirective]
})
export class SharedModule { }
