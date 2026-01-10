import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExceptionFormComponent } from './exception-form.component';

describe('ExceptionFormComponent', () => {
  let component: ExceptionFormComponent;
  let fixture: ComponentFixture<ExceptionFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ExceptionFormComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ExceptionFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
