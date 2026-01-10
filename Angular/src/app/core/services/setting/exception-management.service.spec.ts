import { TestBed } from '@angular/core/testing';

import { ExceptionManagementService } from './exception-management.service';

describe('ExceptionManagementService', () => {
  let service: ExceptionManagementService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ExceptionManagementService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
