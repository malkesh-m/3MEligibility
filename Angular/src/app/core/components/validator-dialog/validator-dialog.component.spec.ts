import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ValidatorDialogComponent } from './validator-dialog.component';

describe('ValidatorDialogComponent', () => {
  let component: ValidatorDialogComponent;
  let fixture: ComponentFixture<ValidatorDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ValidatorDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ValidatorDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
