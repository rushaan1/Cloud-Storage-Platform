import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ItemSelectionService } from '../../services/item-selection.service';
import { EventService } from '../../services/event-service.service';
import { v4 as uuidv4 } from 'uuid';
import {FormControl, Validators} from "@angular/forms";
import {invalidCharacter, invalidFileNameChars} from "../../CustomValidators";
import {FoldersService} from "../../services/ApiServices/folders.service";
import {File} from "../../models/File";
import {ActivatedRoute, Router, RouterLink} from "@angular/router";
import {HelperMethods} from "../../HelperMethods";

@Component({
  selector: 'file-large-item',
  templateUrl: './file-large.component.html',
  styleUrl: './file-large.component.css'
})
export class FileLargeComponent implements OnInit {
  /*
  * Important: BaseFileInterface contains all properties contained by Folders and every single one of those properties are also contained by File but File contains additional properties.
  *            File term is sometimes used to refer to Folder, especially in Frontend code (questionable practice)
  *            IF APPLICABLE: Difference between / and // handled by service
  * */
  @Input() type: string = "";
  @Input() FileFolder!:File;
  @Input() fileInfo?:any; // TODO
  @Output() favoriteChange: EventEmitter<boolean> = new EventEmitter<boolean>();
  @Output() itemSelected: EventEmitter<boolean> = new EventEmitter<boolean>();

  @ViewChild("fileNameInput", { static: false }) fileNameInput?: ElementRef<HTMLInputElement>;
  @ViewChild("selectFile") selectFileCheckbox!: ElementRef<HTMLInputElement>;
  @ViewChild("fileOptionsMenu") fileOptionsMenu!: ElementRef<HTMLDivElement>;
  @ViewChild("file") file!: ElementRef<HTMLDivElement>;
  @ViewChild("ellipsis") ellipsis!: ElementRef<HTMLElement>;
  @ViewChild("renamingFileOptionDiv") renamingFileOptionDiv?: ElementRef<HTMLDivElement>;

  renameFormControl = new FormControl("", [Validators.required, Validators.pattern(/\S/), invalidCharacter]);

  uniqueComponentIdentifierUUID:string = "";
  name = "";
  originalName = "";
  fileOptionsVisible = false;
  renaming = false;
  selected = false;


  constructor(private itemSelectionService: ItemSelectionService, private router:Router, private activatedRoute:ActivatedRoute, private foldersService:FoldersService, private eventService: EventService) { }

  ngOnInit(): void {
    this.uniqueComponentIdentifierUUID = uuidv4();
    this.originalName = this.FileFolder.fileName;
    this.name = this.originalName;
    this.nameResizing();
    this.eventService.listen("unselector all", () => {
      if (this.selected) {
        this.selectItemClick(undefined, true);
      }
    });

    if (this.FileFolder.uncreated) {
      localStorage["uncreatedFolderExists"] = true;
      this.setupInput(false);
    }

    this.eventService.listen("file options expanded", (uuid:string)=>{
      if (uuid != this.uniqueComponentIdentifierUUID && this.fileOptionsVisible == true)
        this.expandOrCollapseOptions();
    });

    window.addEventListener("click", (e) => {
      let clickedOnElement = e.target as HTMLElement;
      if (this.fileOptionsVisible){
        if ((clickedOnElement.parentElement!=this.fileOptionsMenu.nativeElement && clickedOnElement!=this.fileOptionsMenu.nativeElement) && clickedOnElement!=this.ellipsis.nativeElement) {
          this.expandOrCollapseOptions();
        }
      }
    });
  }

  nameResizing() {
    if (this.name.length >= 32) {
      let portionToBeExcluded = this.name.slice(32, this.name.length);
      this.name= this.name.replace(portionToBeExcluded, "") + "...";
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
        if (this.FileFolder.uncreated){
          this.createFolder(this.fileNameInput.nativeElement.value);
        }
        else{
          this.foldersService.renameFolder(this.FileFolder.fileId, this.fileNameInput.nativeElement.value).subscribe({
            next: (response:File) => {
              this.FileFolder.fileName = response.fileName;
              this.FileFolder.filePath = response.filePath;
              this.originalName = response.fileName;
              this.name = response.fileName;
              this.eventService.emit("renameSuccessNotif", response.fileName);
              this.nameResizing();
            },
            error: err => {
              // TODO ErrorNotif for this

            },
            complete: () => {}
          });
        }

        this.renaming = false;
        localStorage["renaming"] = false;
        if (this.renamingFileOptionDiv?.nativeElement){
          this.renamingFileOptionDiv.nativeElement.innerText = "Rename";
        }

        // (9th Jan, 2025) The above code should execute regardless of Observable error
      }
      else if (this.renameFormControl.invalid){
        if (this.renameFormControl.hasError("invalidCharacter")){
          this.eventService.emit("invalidCharacterNotif", this.renameFormControl.errors?.['invalidCharactersString']);
        }
        else {
          this.eventService.emit("emptyInputNotif");
        }
        this.fileNameInput.nativeElement.focus();
        this.fileNameInput.nativeElement.classList.add("file-name-text-input-red");
      }
    }
  }

  setupInput(collapseOptions:boolean, event?:Event) {
    event?.stopPropagation();
    if (localStorage["renaming"] == "true" && !this.renaming){
      this.eventService.emit("alreadyRenamingNotif");
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

    setTimeout(() => {
      if (this.fileNameInput?.nativeElement){
        this.fileNameInput.nativeElement.focus();
        this.fileNameInput.nativeElement.classList.remove("file-name-text-input-red");
        this.fileNameInput.nativeElement.value = this.originalName;
        this.fileNameInput.nativeElement.select();
        this.renameFormControl.setValue(this.originalName);
      }
    }, 90);
    localStorage["renaming"] = true;
  }


  selectItemClick(event?:Event, unselectorAll: boolean = false) {
    event?.stopPropagation()
    if (this.selected) {
      this.itemSelectionService.deSelectItem();
      this.selected = false;
      this.selectFileCheckbox.nativeElement.classList.remove("visible-checkbox");
      if (unselectorAll) {
        this.selectFileCheckbox.nativeElement.checked = false;
      }
    }
    else {
      this.itemSelectionService.selectItem();
      this.selected = true;
      this.selectFileCheckbox.nativeElement.classList.add("visible-checkbox");
    }
    this.eventService.emit("checkbox selection change");
  }

  fetchSubFoldersRedirect(){
    if (this.fileOptionsVisible || localStorage["renaming"] == "true") {
      return;
    }
    this.router.navigate(["folder", ...new HelperMethods().cleanPath(this.FileFolder.filePath)]);
  }

  createFolder(name:string){
    this.foldersService.addFolder(name, this.FileFolder.filePath+name).subscribe({
      next: (response:File) => {
        this.FileFolder.fileId = response.fileId;
        this.FileFolder.fileName = response.fileName;
        this.FileFolder.filePath = response.filePath;
        this.FileFolder.isFavorite = response.isFavorite;
        this.FileFolder.isTrash = response.isTrash;
        this.originalName = response.fileName;
        this.name = response.fileName;
        localStorage["uncreatedFolderExists"] = false;
      },
      error: err => {
        // TODO ErrorNotif for this
      },
      complete: () => {}
    })
  }
}
