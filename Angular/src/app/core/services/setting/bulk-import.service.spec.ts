import { TestBed } from '@angular/core/testing';

import { BulkImportService } from './bulk-import.service';

describe('BulkImportService', () => {
  let service: BulkImportService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BulkImportService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
