import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ComputedValuesDialogComponent } from './computed-values-dialog.component';

describe('ComputedValuesDialogComponent', () => {
  let component: ComputedValuesDialogComponent;
  let fixture: ComponentFixture<ComputedValuesDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ComputedValuesDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ComputedValuesDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
