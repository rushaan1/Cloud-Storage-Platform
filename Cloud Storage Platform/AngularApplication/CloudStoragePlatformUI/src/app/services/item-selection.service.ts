import { Injectable } from '@angular/core';
import {BehaviorSubject} from "rxjs";
import {File} from "../models/File";

@Injectable({
  providedIn: 'root'
})
export class ItemSelectionService {
  private selectedItems = new BehaviorSubject<File[]>([]);
  selectedItems$ = this.selectedItems.asObservable();

  addSelectedItem(item: File){
    this.selectedItems.next([...this.selectedItems.value, item]);
  }

  deselectItem(item:File){
    let items = this.selectedItems.value;
    items = items.filter(f => f.fileId == item.fileId);
    let indexOfItemTobeRemoved = this.selectedItems.value.indexOf(items[0]);
    this.selectedItems.value.splice(indexOfItemTobeRemoved, 1)
    this.selectedItems.next(this.selectedItems.value);
  }

  deSelectAll(){
    this.selectedItems.next([]);
  }

  constructor() { }
}
