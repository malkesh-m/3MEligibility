import { TestBed } from '@angular/core/testing';

import { MakercheckerService } from './makerchecker.service';

describe('MakercheckerService', () => {
  let service: MakercheckerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MakercheckerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
