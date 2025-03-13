import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnInit,
  Output,
  ViewChild
} from '@angular/core';
import { FilesStateService } from '../../services/StateManagementServices/files-state.service';
import { EventService } from '../../services/event-service.service';
import { v4 as uuidv4 } from 'uuid';
import {FormControl, Validators} from "@angular/forms";
import {invalidCharacter, invalidFileNameChars} from "../../CustomValidators";
import {FoldersService} from "../../services/ApiServices/folders.service";
import {File} from "../../models/File";
import {ActivatedRoute, Router, RouterLink} from "@angular/router";
import {Utils} from "../../Utils";
import {BreadcrumbService} from "../../services/StateManagementServices/breadcrumb.service";

@Component({
  selector: 'file-large-item',
  templateUrl: './file-large.component.html',
  styleUrl: './file-large.component.css'
})
export class FileLargeComponent implements OnInit, AfterViewInit {
  /*
  * Important: BaseFileInterface contains all properties contained by Folders and every single one of those properties are also contained by File but File contains additional properties.
  *            File term is sometimes used to refer to Folder, especially in Frontend code (questionable practice)
  *            IF APPLICABLE: Difference between / and // handled by service
  * */
  @Input() type: string = "";
  @Input() FileFolder!:File;
  @Output() destroy = new EventEmitter();
  hoveringOverDestroy = false;

  @ViewChild("fileNameInput", { static: false }) fileNameInput?: ElementRef<HTMLInputElement>;
  @ViewChild("selectFile") selectFileCheckbox!: ElementRef<HTMLInputElement>;
  @ViewChild("fileOptionsMenu") fileOptionsMenu!: ElementRef<HTMLDivElement>;
  @ViewChild("file") file!: ElementRef<HTMLDivElement>;
  @ViewChild("ellipsis") ellipsis!: ElementRef<HTMLElement>;
  @ViewChild("renamingFileOptionDiv") renamingFileOptionDiv?: ElementRef<HTMLDivElement>;
  @ViewChild("abortCreationCross", {static:true}) abortCreationCross?: ElementRef;

  renameFormControl = new FormControl("", [Validators.required, Validators.pattern(/\S/), invalidCharacter]);

  uniqueComponentIdentifierUUID:string = "";
  name = "";
  originalName = "";
  fileOptionsVisible = false;
  renaming = false;
  selected = false;
  appIsInSelectionState = false;
  hoveringOver: boolean = false;
  crumbs: string[] = [];
  anyFileIsRenaming = false;
  anyUncreatedFolderExists = false;

  constructor(protected filesState: FilesStateService, protected router:Router, protected cdRef: ChangeDetectorRef, protected foldersService:FoldersService, protected eventService: EventService, protected breadcrumbService:BreadcrumbService) { }

  ngOnInit(): void {
    this.uniqueComponentIdentifierUUID = uuidv4();
    this.originalName = this.FileFolder.fileName;
    this.name = this.originalName;
    this.nameResizing();

    this.eventService.listen("file options expanded", (uuid:string)=>{
      if (uuid != this.uniqueComponentIdentifierUUID && this.fileOptionsVisible == true)
        this.expandOrCollapseOptions();
    });

    this.breadcrumbService.breadcrumbs$.subscribe((crumbs)=>{
      this.crumbs = crumbs;
    });

    this.filesState.isRenaming$.subscribe((isRenaming)=>{
      this.anyFileIsRenaming = isRenaming;
    });

    this.filesState.uncreatedFolderExists$.subscribe((uncreatedFolderExists)=> {
      this.anyUncreatedFolderExists = uncreatedFolderExists;
    });
  }

  ngAfterViewInit() {
    if (this.FileFolder.uncreated) {
      this.setupInput(false);
    }

    this.filesState.selectedItems$.subscribe(items=>{
      let containsFileId = false;
      this.appIsInSelectionState = items.length > 0;
      if (this.appIsInSelectionState){
        this.showCheckbox();
      }
      else if (!this.hoveringOver){
        this.hideCheckbox();
      }
      items.forEach(item=>{
        if (this.FileFolder.fileId == item.fileId){
          containsFileId = true;
          if (this.selected == false){
            this.selected = true;
            this.showCheckbox();
            this.selectFileCheckbox.nativeElement.checked = true;
          }
        }
      });
      if (!containsFileId && this.selected){
        this.selected = false;
        this.hideCheckbox();
        this.selectFileCheckbox.nativeElement.checked = false;
      }
    });
  }

  @HostListener('window:click', ['$event'])
  onWindowClick(e: Event) {
    let clickedOnElement = e.target as HTMLElement;
    if (this.fileOptionsVisible){
      if ((clickedOnElement.parentElement!=this.fileOptionsMenu.nativeElement && clickedOnElement!=this.fileOptionsMenu.nativeElement) && clickedOnElement!=this.ellipsis.nativeElement) {
        this.expandOrCollapseOptions();
      }
    }
  }

  nameResizing() {
    if (this.name.length >= 32) {
      this.name = this.name.substring(0, 32) + "...";
    }
  }

  expandOrCollapseOptions(event?:Event) {
    event?.stopPropagation();
    const menu = this.fileOptionsMenu.nativeElement;
    if (this.fileOptionsVisible == false) {
      menu.style.visibility = "visible";
      menu.style.height = "200px";
      this.fileOptionsVisible = true;
      this.eventService.emit("file options expanded", this.uniqueComponentIdentifierUUID);
      this.ellipsis.nativeElement.style.backgroundColor = "lightgray";
      this.file.nativeElement.style.backgroundColor = "rgba(211, 211, 211, 0.593)";
    }
    else {
      menu.style.height = "0";
      this.fileOptionsVisible = false;
      setTimeout(() => {
        menu.style.visibility = "hidden";
      }, 400);
      this.ellipsis.nativeElement.style.backgroundColor = "";
      this.file.nativeElement.style.backgroundColor = "";
    }
  }

  renameEnter(event?:Event) {
    event?.stopPropagation();
    if (this.renaming==false){
      return;
    }
    if (this.fileNameInput?.nativeElement) {
      if (this.renameFormControl.valid) {
        let renameCompleted = false;
        if (this.FileFolder.uncreated){
          this.createFolder(this.fileNameInput.nativeElement.value);
        }
        else{
          this.foldersService.renameFolder(this.FileFolder.fileId, this.fileNameInput.nativeElement.value).subscribe({
            next: (response:File) => {
              this.FileFolder.fileName = response.fileName;
              this.FileFolder.filePath = response.filePath;
              this.name = response.fileName;
              this.eventService.emit("addNotif", ["Successfully renamed "+this.originalName+" to "+response.fileName, 15000]);
              this.originalName = response.fileName;
              this.nameResizing();
              renameCompleted = true;
            },
            error: err => {
              // TODO ErrorNotif for this, set renameCompleted to false incase of error

            },
            complete: () => {
              if (renameCompleted){
                this.finishRenaming();
              }
            }
          });
        }
      }
      else if (this.renameFormControl.invalid){
        if (this.renameFormControl.hasError("invalidCharacter")){
          this.eventService.emit("addNotif", ["Invalid character in input: "+this.renameFormControl.errors?.['invalidCharactersString'], 12000]);
        }
        else {
          this.eventService.emit("addNotif", ["Input cannot be empty", 12000]);
        }
        this.fileNameInput.nativeElement.focus();
        this.fileNameInput.nativeElement.classList.add("file-name-text-input-red");
      }
    }
  }

  setupInput(collapseOptions:boolean, event?:Event) {
    event?.stopPropagation();
    if (this.anyFileIsRenaming && !this.renaming){
      this.eventService.emit("addNotif", ["Please finish renaming the current file first", 15000]);
      this.expandOrCollapseOptions();
      return;
    }

    if (this.renamingFileOptionDiv?.nativeElement){
      this.renamingFileOptionDiv.nativeElement.innerText = "Renaming...";
    }

    this.renaming = true;

    if (collapseOptions){
      this.expandOrCollapseOptions();
    }

    this.cdRef.detectChanges();
    if (this.fileNameInput?.nativeElement){
      this.fileNameInput.nativeElement.focus();
      this.fileNameInput.nativeElement.classList.remove("file-name-text-input-red");
      this.fileNameInput.nativeElement.value = this.originalName;
      this.fileNameInput.nativeElement.select();
      this.renameFormControl.setValue(this.originalName);
    }
    this.filesState.setRenaming(true)
  }

  finishRenaming(){
    this.renaming = false;
    this.filesState.setRenaming(false);
    if (this.renamingFileOptionDiv?.nativeElement){
      this.renamingFileOptionDiv.nativeElement.innerText = "Rename";
    }
  }

  fetchSubFoldersRedirect(){
    if (this.fileOptionsVisible || this.anyFileIsRenaming || this.appIsInSelectionState) {
      return;
    }
    this.router.navigate(["folder", ...Utils.cleanPath(this.FileFolder.filePath)]);
  }

  createFolder(name:string){
    let folderCreationCompleted = false;
    this.foldersService.addFolder(name, this.FileFolder.filePath+name).subscribe({
      next: (response:File) => {
        this.FileFolder.fileId = response.fileId;
        this.FileFolder.fileName = response.fileName;
        this.FileFolder.filePath = response.filePath;
        this.FileFolder.isFavorite = response.isFavorite;
        this.FileFolder.isTrash = response.isTrash;
        this.FileFolder.uncreated = false;
        this.originalName = response.fileName;
        this.name = response.fileName;
        this.filesState.setUncreatedFolderExists(false);
        if (this.appIsInSelectionState) {
          this.cdRef.detectChanges();
          this.showCheckbox();
        }
        folderCreationCompleted = true;
      },
      error: err => {
        // TODO ErrorNotif for this
      },
      complete: () => {
        if (folderCreationCompleted){
          this.finishRenaming();
        }
      }
    })
  }

  hideCheckbox(){
    if (this.selectFileCheckbox){
      this.selectFileCheckbox.nativeElement.style.visibility = 'hidden'
    }
  }

  showCheckbox(){
    if (this.selectFileCheckbox){
      this.selectFileCheckbox.nativeElement.style.visibility = 'visible';
    }
  }

  toggleFavorite(event:MouseEvent){
    event.stopPropagation();
    this.foldersService.addOrRemoveFromFavorite(this.FileFolder.fileId).subscribe({
      next: (response:File) => {
        this.FileFolder.isFavorite = response.isFavorite;
      },
      error: err => {
        // TODO ErrorNotif for this
      }
    });
  }

  trash(event:MouseEvent){
    event.stopPropagation();
    this.foldersService.addOrRemoveFromTrash(this.FileFolder.fileId).subscribe({
      next: (response:File) => {
        this.FileFolder.isTrash = response.isTrash;
        this.destroy.emit();
        this.eventService.emit("addNotif", ["Successfully moved "+this.name+" in trash", 20000]);
      },
      error: err => {
        // TODO ErrorNotif for this
      }
    });
  }

  delete(event:MouseEvent){
    event.stopPropagation();
    this.eventService.emit("deleteConfirmNotif", "Are you sure you want to permanently delete "+this.name+"?", ()=>{
      this.foldersService.deleteFolder(this.FileFolder.fileId).subscribe({
        next: (response:File) => {
          this.eventService.emit("addNotif", ["Successfully deleted "+this.name, 20000]);
          this.destroy.emit();
        },
        error: err => {
          // TODO ErrorNotif for this
        }
      });
    });
  }

  restore(event:MouseEvent) {
    event.stopPropagation();
    this.foldersService.addOrRemoveFromTrash(this.FileFolder.fileId).subscribe({
      next: (response:File) => {
        this.FileFolder.isTrash = response.isTrash;
        this.destroy.emit();
        this.eventService.emit("addNotif", ["Successfully restored " + this.name, 20000]);
      },
      error: err => {
        // TODO ErrorNotif for this
      }
    });
  }
}
