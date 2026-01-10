import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MakerCheckerHistoryComponent } from './maker-checker-history.component';

describe('MakerCheckerHistoryComponent', () => {
  let component: MakerCheckerHistoryComponent;
  let fixture: ComponentFixture<MakerCheckerHistoryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MakerCheckerHistoryComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MakerCheckerHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
