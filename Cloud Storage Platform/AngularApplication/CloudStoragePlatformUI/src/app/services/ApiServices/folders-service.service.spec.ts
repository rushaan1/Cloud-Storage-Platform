import { TestBed } from '@angular/core/testing';

import { FoldersServiceService } from './folders-service.service';

describe('FoldersServiceService', () => {
  let service: FoldersServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FoldersServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
