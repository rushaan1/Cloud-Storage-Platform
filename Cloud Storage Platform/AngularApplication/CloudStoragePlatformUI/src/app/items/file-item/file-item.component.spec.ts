import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FileItemComponent } from './file-item.component';

describe('FileLargeComponent', () => {
  let component: FileItemComponent;
  let fixture: ComponentFixture<FileItemComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [FileItemComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FileItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
