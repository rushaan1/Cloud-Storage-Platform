import { AfterViewChecked, Component, ViewChild } from '@angular/core';
import { ItemSelectionService } from '../services/item-selection.service';
import { EventService } from '../services/event-service.service';

@Component({
  selector: 'viewer',
  templateUrl: './viewer.component.html',
  styleUrl: './viewer.component.css'
})
export class ViewerComponent implements AfterViewChecked {
  constructor(public itemSelectionService:ItemSelectionService, public eventService:EventService){}
  previouslySelected = false;
  itemsSelected = 0;
  files = ["file innit gawk gawk sigma porch innit", "file 2", "file 3","file 1", "file 2", "file 3","file 1", "file 2", "file 3","file 1", "file 2", "file 3","file 1", "file 2", "file 3","file 1", "file 2", "file 3","file 1", "file 2", "file 3","file 1", "file 2", "file 3","file 1", "file 2", "file 3"];

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
