import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StorageBarComponent } from './storage-bar.component';

describe('StorageBarComponent', () => {
  let component: StorageBarComponent;
  let fixture: ComponentFixture<StorageBarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [StorageBarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StorageBarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
