import { AfterContentChecked, AfterContentInit, AfterViewChecked, AfterViewInit, Component, ContentChild, ElementRef, Input, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { ItemSelectionService } from '../../services/item-selection.service';

@Component({
  selector: 'info-panel',
  templateUrl: './info-panel.component.html',
  styleUrl: './info-panel.component.css'
})
export class InfoPanelComponent implements AfterViewChecked, AfterViewInit {
  @Input() infoText:string = "";
  @Input() hasMenuContent!:boolean;
  @Input() shouldBeVisible!:boolean;
  @ViewChild("infoPanel") infoPanel!:ElementRef<HTMLDivElement>;
  @ContentChild(TemplateRef) projectedContent?: TemplateRef<any>;
  previouslyVisible = false;
  transitionDone = false;
  afterContentInit = false;

  constructor(public itemSelectionService:ItemSelectionService){}

  // ngAfterContentInit(): void {
  //   this.hasMenuContent = !!this.projectedContent;
  //   this.afterContentInit = true;
  //   // if (!this.hasMenuContent && !this.transitionDone){
  //   //   setTimeout(()=>{this.infoPanel.nativeElement.style.transform = "translate(0%)";
  //   //     this.transitionDone = true;},35)
  //   // }
  // }

  ngAfterViewInit(): void {
  }

  ngAfterViewChecked(){
    if (this.shouldBeVisible){
      if (this.previouslyVisible!=this.shouldBeVisible){
        setTimeout(() => {
          this.infoPanel.nativeElement.classList.add("visible-info-panel");
        },50);
      }
      this.previouslyVisible = this.shouldBeVisible;
    }
    else{
      this.infoPanel.nativeElement.classList.remove("visible-info-panel");
      this.previouslyVisible = this.shouldBeVisible;
    }
  }

}
