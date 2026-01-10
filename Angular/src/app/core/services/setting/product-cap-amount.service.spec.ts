import { TestBed } from '@angular/core/testing';

import { ProductCapAmountService } from './product-cap-amount.service';

describe('ProductCapAmountService', () => {
  let service: ProductCapAmountService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ProductCapAmountService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
