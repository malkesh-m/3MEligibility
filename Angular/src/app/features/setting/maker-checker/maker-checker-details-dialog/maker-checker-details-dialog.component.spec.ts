import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MakerCheckerDetailsDialogComponent } from './maker-checker-details-dialog.component';

describe('MakerCheckerDetailsDialogComponent', () => {
  let component: MakerCheckerDetailsDialogComponent;
  let fixture: ComponentFixture<MakerCheckerDetailsDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MakerCheckerDetailsDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MakerCheckerDetailsDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
