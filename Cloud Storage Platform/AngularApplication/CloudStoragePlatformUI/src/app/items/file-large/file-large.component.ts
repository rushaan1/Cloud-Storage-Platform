import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';

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
  @ViewChild("fileNameInput", { static: false }) fileNameInput?: ElementRef<HTMLInputElement>;
  @ViewChild("selectFile") selectFileCheckbox!:ElementRef<HTMLInputElement>;

  @ViewChild("backBtn") backBtn!:ElementRef<HTMLElement>;
  @ViewChild("selectTxt") selectFile!:ElementRef<HTMLElement>;
  @ViewChild("fileOptionsMenu") fileOptionsMenu!:ElementRef<HTMLDivElement>;
  originalName = "";
  fileOptionsShouldBeVisible = false;
  renaming = false;
  moving = false;

  ngOnInit(): void {
    this.originalName = this.name;
    this.nameResizing();
  }

  changeMoveVisiblity(visibility:string){
    this.backBtn.nativeElement.style.visibility = visibility;
    this.selectFile.nativeElement.style.visibility = visibility;
    (document.getElementsByClassName("move-selection")[0] as HTMLElement).style.visibility = visibility;
  }

  nameResizing(){
    if (this.name.length >= 32) {
      let portionToBeExcluded = this.name.slice(32, this.name.length);
      this.name = this.name.replace(portionToBeExcluded, "") + "...";
    }
  }

  expandOptions() {
    this.moving = false;
    if (this.fileOptionsShouldBeVisible){
      setTimeout(()=>{this.changeMoveVisiblity("hidden")},300);
    }
    
    const menu = this.fileOptionsMenu.nativeElement;
    if (this.fileOptionsShouldBeVisible == false) {
      menu.style.visibility = "visible";
      menu.style.height = "200px";
      this.fileOptionsShouldBeVisible = true;
      this.applyMargin(menu.querySelectorAll("div"));
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
    if (!this.renameEnter) {
      return;
    }
    if (this.fileNameInput?.nativeElement){
      this.originalName = this.fileNameInput?.nativeElement?.value;
      this.name = this.originalName;
      this.nameResizing();
      console.log("Name updated?"); 
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

  move(){
    this.moving = true;
    setTimeout(()=>{this.changeMoveVisiblity("visible");},25);
  }

  back(){
    this.moving = false;
    setTimeout(()=>{this.applyMargin(this.fileOptionsMenu.nativeElement.querySelectorAll("div"));},20);
    setTimeout(()=>{this.changeMoveVisiblity("hidden");},25);
  }
}
