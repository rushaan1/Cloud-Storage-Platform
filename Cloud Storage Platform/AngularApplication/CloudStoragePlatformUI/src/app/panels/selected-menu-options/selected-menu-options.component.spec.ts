import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SelectedMenuOptionsComponent } from './selected-menu-options.component';

describe('SelectedMenuOptionsComponent', () => {
  let component: SelectedMenuOptionsComponent;
  let fixture: ComponentFixture<SelectedMenuOptionsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SelectedMenuOptionsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SelectedMenuOptionsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
