import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductCapAmountComponent } from './product-cap-amount.component';

describe('ProductCapAmountComponent', () => {
  let component: ProductCapAmountComponent;
  let fixture: ComponentFixture<ProductCapAmountComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ProductCapAmountComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductCapAmountComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
