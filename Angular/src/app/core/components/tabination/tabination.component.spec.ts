import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TabinationComponent } from './tabination.component';

describe('TabinationComponent', () => {
  let component: TabinationComponent;
  let fixture: ComponentFixture<TabinationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TabinationComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TabinationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
