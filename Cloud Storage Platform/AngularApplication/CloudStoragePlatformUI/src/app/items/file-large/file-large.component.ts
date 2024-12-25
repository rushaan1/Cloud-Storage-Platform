import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ItemSelectionService } from '../../services/item-selection.service';
import { EventService } from '../../services/event-service.service';
import { v4 as uuidv4 } from 'uuid';

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

  uniqueComponentIdentifierUUID:string = ""; // after backend integration can be replaced with the id supplied by api
  originalName = "";
  fileOptionsShouldBeVisible = false;
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
      if (uuid != this.uniqueComponentIdentifierUUID && this.fileOptionsShouldBeVisible == true)
        this.expandOptions();
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
    if (this.fileOptionsShouldBeVisible == false) {
      menu.style.visibility = "visible";
      menu.style.height = "200px";
      this.fileOptionsShouldBeVisible = true;
      this.applyMargin(menu.querySelectorAll("div"));
      this.eventService.emit("file options expanded", this.uniqueComponentIdentifierUUID);
    }
    else {
      menu.style.height = "0";
      this.fileOptionsShouldBeVisible = false;
      setTimeout(() => {
        menu.style.visibility = "hidden";
      }, 400);
    }
  }

  applyMargin(options: NodeListOf<HTMLDivElement>) {
    for (let i = 1; i < options.length; i++) {
      console.log(options[i].textContent);
      options[i].style.marginTop = `${31 * i}px`;
    }
  }

  renameEnter() {
    if (this.fileNameInput?.nativeElement) {
      const onlyWhiteSpaceOrEmptyCheckingRegexPattern = /\S+/;
      if (onlyWhiteSpaceOrEmptyCheckingRegexPattern.test(this.fileNameInput?.nativeElement?.value)) {
        this.originalName = this.fileNameInput?.nativeElement?.value;
        this.name = this.originalName;
        this.nameResizing();
        console.log("Name updated?");
      }
    }
    this.renaming = false;
  }

  setupInput() {
    this.renaming = this.renaming ? false : true;
    this.expandOptions();
    setTimeout(() => {
      this.fileNameInput?.nativeElement?.focus();
      this.fileNameInput?.nativeElement?.select();
    }, 50);
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
    this.eventService.emit("checkbox selection change", 0);
  }
}
