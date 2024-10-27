import { AfterContentInit, AfterViewInit, Component, ContentChild, ElementRef, Input, OnInit, TemplateRef, ViewChild } from '@angular/core';

@Component({
  selector: 'info-panel',
  templateUrl: './info-panel.component.html',
  styleUrl: './info-panel.component.css'
})
export class InfoPanelComponent implements AfterViewInit, AfterContentInit {
  @Input() infoText:string = "";
  @ViewChild("infoPanel") infoPanel!:ElementRef<HTMLDivElement>;
  @ContentChild(TemplateRef) projectedContent?: TemplateRef<any>;
  hasMenuContent = false;

  ngAfterViewInit(): void {
    this.infoPanel.nativeElement.style.transform = "translateY(0%)";
  }

  ngAfterContentInit(): void {
    this.hasMenuContent = !!this.projectedContent;
  }
}
