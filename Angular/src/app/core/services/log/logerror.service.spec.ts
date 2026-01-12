import { TestBed } from '@angular/core/testing';

import { LogerrorService } from './logerror.service';

describe('LogerrorService', () => {
  let service: LogerrorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LogerrorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
