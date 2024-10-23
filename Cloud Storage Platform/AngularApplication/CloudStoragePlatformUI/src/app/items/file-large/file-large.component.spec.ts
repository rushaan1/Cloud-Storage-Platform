import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FileLargeComponent } from './file-large.component';

describe('FileLargeComponent', () => {
  let component: FileLargeComponent;
  let fixture: ComponentFixture<FileLargeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [FileLargeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FileLargeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
