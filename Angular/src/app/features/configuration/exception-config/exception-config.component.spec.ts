import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExceptionConfigComponent } from './exception-config.component';

describe('ExceptionConfigComponent', () => {
  let component: ExceptionConfigComponent;
  let fixture: ComponentFixture<ExceptionConfigComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ExceptionConfigComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ExceptionConfigComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
