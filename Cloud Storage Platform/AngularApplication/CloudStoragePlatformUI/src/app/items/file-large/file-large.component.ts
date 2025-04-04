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
import {FilesStateService} from '../../services/StateManagementServices/files-state.service';
import {EventService} from '../../services/event-service.service';
import {v4 as uuidv4} from 'uuid';
import {FormControl, Validators} from "@angular/forms";
import {invalidCharacter} from "../../CustomValidators";
import {FilesAndFoldersService} from "../../services/ApiServices/files-and-folders.service";
import {File} from "../../models/File";
import {ActivatedRoute, Router} from "@angular/router";
import {Utils} from "../../Utils";
import {BreadcrumbService} from "../../services/StateManagementServices/breadcrumb.service";
import {FileType} from "../../models/FileType";

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
  @Input() type!: FileType;
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
  renameReqSent = false;
  selected = false;
  appIsInSelectionState = false;
  hoveringOver: boolean = false;
  crumbs: string[] = [];
  anyFileIsRenaming = false;
  anyUncreatedFolderExists = false;
  preservedExtensionForRename = "";

  constructor(private el: ElementRef, protected filesState: FilesStateService, protected router:Router, protected cdRef: ChangeDetectorRef, protected foldersService:FilesAndFoldersService, protected eventService: EventService, protected breadcrumbService:BreadcrumbService, private route:ActivatedRoute) { }

  ngOnInit(): void {
    this.uniqueComponentIdentifierUUID = uuidv4();
    this.originalName = this.FileFolder.fileName;
    this.name = this.originalName;
    this.name = Utils.resize(this.name, 32);

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
          if (!this.selected){
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

    this.route.queryParams.subscribe((params) => {
      const renameFocus = params["renameFocus"];
      if (renameFocus==this.FileFolder.fileId){
        this.setupInput(false);
        this.el.nativeElement.scrollIntoView({behavior: "smooth", block: "start"});
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
    if (this.renaming==false || this.renameReqSent){
      return;
    }
    if (this.fileNameInput?.nativeElement) {
      if (this.renameFormControl.valid) {
        let renameCompleted = false;
        if (this.FileFolder.uncreated){
          this.createFolder(this.fileNameInput.nativeElement.value);
        }
        else{
          const newName = this.fileNameInput.nativeElement.value+this.preservedExtensionForRename;
          this.foldersService.rename(this.FileFolder.fileId, newName, this.type==FileType.Folder).subscribe({
            next: () => {
              this.eventService.emit("addNotif", ["Successfully renamed "+this.originalName+" to "+newName, 15000]);
              this.originalName = newName;
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
        this.renameReqSent = true;
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
    this.renameReqSent = false;

    if (collapseOptions){
      this.expandOrCollapseOptions();
    }

    this.cdRef.detectChanges();
    if (this.fileNameInput?.nativeElement){
      this.fileNameInput.nativeElement.focus();
      this.fileNameInput.nativeElement.classList.remove("file-name-text-input-red");
      this.fileNameInput.nativeElement.value = this.originalName;
      this.renameFormControl.setValue(this.originalName);
      if (this.type!=FileType.Folder){
        let split = this.originalName.split(".");
        this.preservedExtensionForRename = "."+split.pop();
        this.fileNameInput.nativeElement.value = split.join(".");
        this.renameFormControl.setValue(this.fileNameInput.nativeElement.value);
      }
      this.fileNameInput.nativeElement.select();
    }
    this.filesState.setRenaming(true)
  }

  finishRenaming(){
    this.renaming = false;
    this.preservedExtensionForRename = "";
    this.filesState.setRenaming(false);
    if (this.renamingFileOptionDiv?.nativeElement){
      this.renamingFileOptionDiv.nativeElement.innerText = "Rename";
    }
  }


  fetchSubFoldersRedirect(){
    if (this.fileOptionsVisible || this.anyFileIsRenaming || this.appIsInSelectionState) {
      if (this.appIsInSelectionState){
        if (this.selected){
          this.filesState.deselectItem(this.FileFolder);
          this.selectFileCheckbox.nativeElement.checked = false;
          this.showCheckbox();
        }
        else if (!this.selected){
          this.filesState.addSelectedItem(this.FileFolder);
          this.selectFileCheckbox.nativeElement.checked = true;
        }
      }
      return;
    }
    this.router.navigate(["folder", ...Utils.cleanPath(this.FileFolder.filePath)]);
  }

  createFolder(name:string){
    let folderCreationCompleted = false;
    // filePath for creation set by viewer
    this.foldersService.addFolder(name, this.FileFolder.filePath+name).subscribe({
      next: (response:File) => {
        this.filesState.setUncreatedFolderExists(false);
        folderCreationCompleted = true;
        this.destroy.emit();
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
    this.foldersService.addOrRemoveFromFavorite(this.FileFolder.fileId, this.type==FileType.Folder).subscribe({
      next: (response:File) => {
        // this.FileFolder.isFavorite = response.isFavorite;
      },
      error: err => {
        // TODO ErrorNotif for this
      }
    });
  }

  trash(event:MouseEvent){
    event.stopPropagation();
    this.foldersService.addOrRemoveFromTrash(this.FileFolder.fileId,this.type==FileType.Folder).subscribe({
      next: (response:File) => {
        // this.FileFolder.isTrash = response.isTrash;
        // this.destroy.emit();
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
      this.foldersService.delete(this.FileFolder.fileId,this.type==FileType.Folder).subscribe({
        next: (response:File) => {
          this.eventService.emit("addNotif", ["Successfully deleted "+this.name, 20000]);
        },
        error: err => {
          // TODO ErrorNotif for this
        }
      });
    });
  }

  restore(event:MouseEvent) {
    event.stopPropagation();
    this.foldersService.addOrRemoveFromTrash(this.FileFolder.fileId,this.type==FileType.Folder).subscribe({
      next: (response:File) => {
        this.eventService.emit("addNotif", ["Successfully restored " + this.name, 20000]);
      },
      error: err => {
        // TODO ErrorNotif for this
      }
    });
  }

  activateMoveState(event:MouseEvent){
    event.stopPropagation();
    this.filesState.setItemsBeingMoved([this.FileFolder]);
    if (this.router.url.includes("filter/home")){
      this.eventService.emit("reload viewer list");
      return;
    }
    this.router.navigate(["filter","home"]);
  }

  move(){
    const itemsBeingMoved = this.filesState.getItemsBeingMoved();
    this.foldersService.batchMoveFolders(itemsBeingMoved.map((f)=>f.fileId), Utils.constructFilePathForApi(Utils.cleanPath(this.FileFolder.filePath))).subscribe({
      next:()=>{
        setTimeout(()=>{this.eventService.emit("addNotif", ["Moved "+itemsBeingMoved.length+" item(s) to "+this.name+".", 12000]);},800);
        this.filesState.setItemsBeingMoved([]);
        this.fetchSubFoldersRedirect();
      }
    });
  }

  protected readonly Utils = Utils;
  protected readonly FileType = FileType;
}
