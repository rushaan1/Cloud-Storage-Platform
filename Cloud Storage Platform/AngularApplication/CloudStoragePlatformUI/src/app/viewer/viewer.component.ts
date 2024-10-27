import { Component } from '@angular/core';
import { ItemSelectionService } from '../services/item-selection.service';

@Component({
  selector: 'viewer',
  templateUrl: './viewer.component.html',
  styleUrl: './viewer.component.css'
})
export class ViewerComponent {
  constructor(public itemSelectionService:ItemSelectionService){}

  anyItemsSelected():boolean{
    return (this.itemSelectionService.selectedItems.length>0);
  }
}
