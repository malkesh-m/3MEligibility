import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductCapComponent } from './product-cap.component';

describe('ProductCapComponent', () => {
  let component: ProductCapComponent;
  let fixture: ComponentFixture<ProductCapComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ProductCapComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductCapComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
