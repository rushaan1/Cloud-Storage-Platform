import { TestBed } from '@angular/core/testing';

import { FilesStateService } from './files-state.service';

describe('ItemSelectionService', () => {
  let service: FilesStateService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FilesStateService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
