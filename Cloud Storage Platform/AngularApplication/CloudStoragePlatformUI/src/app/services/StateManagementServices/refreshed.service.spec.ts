import { TestBed } from '@angular/core/testing';

import { RefreshedService } from './refreshed.service';

describe('RefreshedService', () => {
  let service: RefreshedService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RefreshedService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
