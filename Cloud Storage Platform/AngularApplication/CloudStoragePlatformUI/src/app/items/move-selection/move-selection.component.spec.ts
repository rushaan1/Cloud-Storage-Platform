import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MoveSelectionComponent } from './move-selection.component';

describe('MoveSelectionComponent', () => {
  let component: MoveSelectionComponent;
  let fixture: ComponentFixture<MoveSelectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MoveSelectionComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MoveSelectionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
