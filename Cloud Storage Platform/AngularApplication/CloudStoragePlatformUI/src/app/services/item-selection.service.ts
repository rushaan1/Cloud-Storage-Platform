import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ItemSelectionService {
  public selectedItems:any[]=[];

  selectItem(/**TODO Parameter*/){
    //TODO Add actual item to selectedItems
    this.selectedItems.push("Sample Item");
  }

  deSelectItem(/**TODO Parameter*/){
    //TODO Pop the actualy iem from selectedItems
    this.selectedItems.pop();
  }

  deSelectAll(){
    this.selectedItems = [];
  }

  constructor() { }
}
