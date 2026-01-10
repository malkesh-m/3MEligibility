import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MakerCheckerConfigComponent } from './maker-checker-config.component';

describe('MakerCheckerConfigComponent', () => {
  let component: MakerCheckerConfigComponent;
  let fixture: ComponentFixture<MakerCheckerConfigComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MakerCheckerConfigComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MakerCheckerConfigComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
