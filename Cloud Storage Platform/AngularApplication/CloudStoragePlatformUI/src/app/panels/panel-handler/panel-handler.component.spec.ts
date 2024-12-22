import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PanelHandlerComponent } from './panel-handler.component';

describe('PanelHandlerComponent', () => {
  let component: PanelHandlerComponent;
  let fixture: ComponentFixture<PanelHandlerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PanelHandlerComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PanelHandlerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
