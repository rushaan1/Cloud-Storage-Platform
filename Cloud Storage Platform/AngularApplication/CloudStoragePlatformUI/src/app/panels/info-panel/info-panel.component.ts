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
  @ViewChild("infoPanel") infoPanel!:ElementRef<HTMLDivElement>;
  @ContentChild(TemplateRef) projectedContent?: TemplateRef<any>;
  previouslySelected = false;
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
    let itemSelected = this.anyItemsSelected();
    // IMPORTANT would likely need to use different logic INSTEAD of checking if any item is selected for displaying or not displaying menu based info panels
    if (itemSelected){
      if (this.previouslySelected != itemSelected){
        setTimeout(() => {
          this.infoPanel.nativeElement.classList.add("visible-info-panel");
        },50); 
        this.previouslySelected = itemSelected;
      }
      else{
        this.previouslySelected = itemSelected;
      }
    }
    else if (this.hasMenuContent){
      // ONLY for displaying/hiding the panel
      this.infoPanel.nativeElement.classList.remove("visible-info-panel");
      this.previouslySelected = itemSelected;
    }
  }

  anyItemsSelected():boolean{
    return (this.itemSelectionService.selectedItems.length>0);
  }
}
