import { AfterViewChecked, AfterViewInit, Component } from '@angular/core';
import { ItemSelectionService } from '../../services/item-selection.service';
import { EventService } from '../../services/event-service.service';

@Component({
  selector: 'info-panel-handler',
  templateUrl: './panel-handler.component.html',
  styleUrl: './panel-handler.component.css'
})
export class PanelHandlerComponent implements AfterViewChecked, AfterViewInit {
  itemsSelected = 0;
  previouslySelected = false;
  sample1Visbility = false;

  constructor(public itemSelectionService:ItemSelectionService, public eventService:EventService){}
  
  ngAfterViewInit(): void {
    this.eventService.listen("checkbox selection change",()=>{
      this.itemsSelected = this.itemSelectionService.selectedItems.length; // had to do this because change was not being detected when checkbox was being selected/unselected until mouse moved
    })
  }

  ngAfterViewChecked(): void {
    const infoPanelComponent = document.getElementById("infoPanel") as HTMLElement;
    let isSelected = this.anyItemsSelected();
    if (isSelected){
      this.previouslySelected = isSelected;
      infoPanelComponent.style.display = "inline";
      this.itemsSelected = this.itemSelectionService.selectedItems.length;
    }
    else{
      if (this.previouslySelected!=isSelected){
        setTimeout(()=>{infoPanelComponent.style.display = "none";},600)
      }
      else{
        infoPanelComponent.style.display = "none";
      }
    }
  }

  anyItemsSelected():boolean{
    return (this.itemSelectionService.selectedItems.length>0);
  }

  unselect(){
    this.eventService.emit("unselector all", 0);
  }

}
