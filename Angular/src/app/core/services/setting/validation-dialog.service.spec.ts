import { TestBed } from '@angular/core/testing';

import { ValidationDialogService } from './validation-dialog.service';

describe('ValidationDialogService', () => {
  let service: ValidationDialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ValidationDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
