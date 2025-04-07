import { TestBed } from '@angular/core/testing';

import { NetworkStatusServiceService } from './network-status-service.service';

describe('NetworkStatusServiceService', () => {
  let service: NetworkStatusServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NetworkStatusServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
