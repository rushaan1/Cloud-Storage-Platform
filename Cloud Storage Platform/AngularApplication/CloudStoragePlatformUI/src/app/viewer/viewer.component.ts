import { AfterViewChecked, Component, ViewChild } from '@angular/core';
import { ItemSelectionService } from '../services/item-selection.service';

@Component({
  selector: 'viewer',
  templateUrl: './viewer.component.html',
  styleUrl: './viewer.component.css'
})
export class ViewerComponent implements AfterViewChecked {
  constructor(public itemSelectionService:ItemSelectionService){}
  previouslySelected = false;

  ngAfterViewChecked(): void {
    const infoPanelComponent = document.getElementById("infoPanel") as HTMLElement;
    let isSelected = this.anyItemsSelected();
    if (isSelected){
      this.previouslySelected = isSelected;
      infoPanelComponent.style.display = "inline";
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
}
