import { TestBed } from '@angular/core/testing';

import { ProductCapService } from './product-cap.service';

describe('ProductCapService', () => {
  let service: ProductCapService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ProductCapService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
