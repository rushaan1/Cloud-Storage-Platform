import { TestBed } from '@angular/core/testing';

import { FilesAndFoldersService } from './files-and-folders.service';

describe('FoldersServiceService', () => {
  let service: FilesAndFoldersService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FilesAndFoldersService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
