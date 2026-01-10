import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExceptionParameterFormComponent } from './exception-parameter-form.component';

describe('ExceptionParameterFormComponent', () => {
  let component: ExceptionParameterFormComponent;
  let fixture: ComponentFixture<ExceptionParameterFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ExceptionParameterFormComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ExceptionParameterFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
