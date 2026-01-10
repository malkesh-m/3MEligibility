import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExceptionParameterComponent } from './exception-parameter.component';

describe('ExceptionParameterComponent', () => {
  let component: ExceptionParameterComponent;
  let fixture: ComponentFixture<ExceptionParameterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ExceptionParameterComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ExceptionParameterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
