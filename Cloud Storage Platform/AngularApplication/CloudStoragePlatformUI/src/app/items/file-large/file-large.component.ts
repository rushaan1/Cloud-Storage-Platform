import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ItemSelectionService } from '../../services/item-selection.service';
import { EventService } from '../../services/event-service.service';
import { v4 as uuidv4 } from 'uuid';
import {FormControl, Validators} from "@angular/forms";
import {invalidCharacter, invalidFileNameChars} from "../../CustomValidators";

@Component({
  selector: 'file-large-item',
  templateUrl: './file-large.component.html',
  styleUrl: './file-large.component.css'
})
export class FileLargeComponent implements OnInit {
  @Input() type: string = "";
  @Input() name: string = "";
  @Input() favorite: boolean = false;
  @Output() favoriteChange: EventEmitter<boolean> = new EventEmitter<boolean>();
  @Output() itemSelected: EventEmitter<boolean> = new EventEmitter<boolean>();

  @ViewChild("fileNameInput", { static: false }) fileNameInput?: ElementRef<HTMLInputElement>;
  @ViewChild("selectFile") selectFileCheckbox!: ElementRef<HTMLInputElement>;
  @ViewChild("fileOptionsMenu") fileOptionsMenu!: ElementRef<HTMLDivElement>;
  @ViewChild("file") file!: ElementRef<HTMLDivElement>;
  @ViewChild("ellipsis") ellipsis!: ElementRef<HTMLElement>;

  renameFormControl = new FormControl("", [Validators.required, Validators.pattern(/\S/), invalidCharacter]);

  uniqueComponentIdentifierUUID:string = ""; // after backend integration can be replaced with the id supplied by api
  originalName = "";
  fileOptionsVisible = false;
  renaming = false;
  selected = false;

  constructor(private itemSelectionService: ItemSelectionService, private eventService: EventService) { }

  ngOnInit(): void {
    this.uniqueComponentIdentifierUUID = uuidv4();
    this.originalName = this.name;
    this.nameResizing();
    this.eventService.listen("unselector all", () => {
      if (this.selected) {
        this.selectItemClick(true);
      }
    });

    this.eventService.listen("file options expanded", (uuid:string)=>{
      if (uuid != this.uniqueComponentIdentifierUUID && this.fileOptionsVisible == true)
        this.expandOptions();
    });

    window.addEventListener("click", (e) => {
      let clickedOnElement = e.target as HTMLElement;
      if (this.fileOptionsVisible){
        if ((clickedOnElement.parentElement!=this.fileOptionsMenu.nativeElement && clickedOnElement!=this.fileOptionsMenu.nativeElement) && clickedOnElement!=this.ellipsis.nativeElement) {
          this.expandOptions();
        }
      }
    });
  }

  nameResizing() {
    if (this.name.length >= 32) {
      let portionToBeExcluded = this.name.slice(32, this.name.length);
      this.name = this.name.replace(portionToBeExcluded, "") + "...";
    }
  }

  expandOptions() {
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

  renameEnter() {
    if (this.fileNameInput?.nativeElement) {
      if (this.renameFormControl.valid) {
        this.originalName = this.fileNameInput?.nativeElement?.value;
        this.name = this.originalName;
        this.nameResizing();
        this.renaming = false;
        localStorage["renaming"] = false;
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

  setupInput() {
    if (localStorage["renaming"] == "true"){
      this.eventService.emit("alreadyRenamingNotif");
      this.expandOptions();
      return;
    }
    this.renaming = this.renaming ? false : true;
    this.expandOptions();
    setTimeout(() => {
      if (this.fileNameInput?.nativeElement){
        this.fileNameInput.nativeElement.focus();
        this.fileNameInput.nativeElement.classList.remove("file-name-text-input-red");
        this.fileNameInput.nativeElement.value = this.originalName;
        this.fileNameInput.nativeElement.select();
      }
    }, 90);
    localStorage["renaming"] = true;
  }


  selectItemClick(unselectorAll: boolean = false) {
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
}
