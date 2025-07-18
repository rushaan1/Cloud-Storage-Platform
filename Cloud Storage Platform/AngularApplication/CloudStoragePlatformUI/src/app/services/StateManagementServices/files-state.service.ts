import {ChangeDetectorRef, Injectable} from '@angular/core';
import {BehaviorSubject} from "rxjs";
import {File} from "../../models/File";

@Injectable({
  providedIn: 'root'
})
export class FilesStateService {

  constructor() { }

  private itemsBeingMoved = new BehaviorSubject<File[]>([]);
  itemsBeingMoved$ = this.itemsBeingMoved.asObservable();

  private selectedItems = new BehaviorSubject<File[]>([]);
  selectedItems$ = this.selectedItems.asObservable();

  private isRenaming = new BehaviorSubject<boolean>(false);
  isRenaming$ = this.isRenaming.asObservable();

  private uncreatedFolderExists = new BehaviorSubject<boolean>(false);
  uncreatedFolderExists$ = this.uncreatedFolderExists.asObservable();

  private filesInViewer = new BehaviorSubject<File[]>([]);
  filesInViewer$ = this.filesInViewer.asObservable();

  fileOpened = false;
  outsideFilesAndFoldersMode = false;
  showSpaceUtilizedInNavDrawer = true;

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

  setRenaming(val:boolean){
    this.isRenaming.next(val);
  }

  setUncreatedFolderExists(val:boolean){
    this.uncreatedFolderExists.next(val);
  }

  setItemsBeingMoved(items:File[]){
    this.itemsBeingMoved.next(items);
  }

  getItemsBeingMoved():File[]{
    return this.itemsBeingMoved.value;
  }

  setFilesInViewer(files:File[]){
    this.filesInViewer.next(files);
  }

  getFilesInViewer():File[]{
    return this.filesInViewer.value;
  }
}
