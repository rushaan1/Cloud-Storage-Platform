import { AfterViewInit, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';

@Component({
  selector: 'info-panel',
  templateUrl: './info-panel.component.html',
  styleUrl: './info-panel.component.css'
})
export class InfoPanelComponent implements AfterViewInit {
  @Input() infoText:string = "";
  @ViewChild("infoPanel") infoPanel!:ElementRef<HTMLDivElement>;

  ngAfterViewInit(): void {
    this.infoPanel.nativeElement.style.transform = "translateY(0%)";
  }
}
