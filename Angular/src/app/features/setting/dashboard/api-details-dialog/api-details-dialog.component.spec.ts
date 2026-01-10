import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ApiDetailsDialogComponent } from './api-details-dialog.component';

describe('ApiDetailsDialogComponent', () => {
  let component: ApiDetailsDialogComponent;
  let fixture: ComponentFixture<ApiDetailsDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ApiDetailsDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ApiDetailsDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
