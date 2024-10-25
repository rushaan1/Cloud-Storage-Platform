import { Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';

@Component({
  selector: 'file-large-item',
  templateUrl: './file-large.component.html',
  styleUrl: './file-large.component.css'
})
export class FileLargeComponent implements OnInit {
  @Input() type: string = "";
  @Input() name: string = "";
  @ViewChild("fileNameInput") fileNameInput!: ElementRef;
  originalName="";
  fileOptionsShouldBeVisible = false;
  renaming = false;

  ngOnInit(): void {
    this.originalName = this.name;
    if (this.name.length>=32){
      let portionToBeExcluded = this.name.slice(32, this.name.length);
      this.name = this.name.replace(portionToBeExcluded, "")+"...";
    }
  }

  expandOptions(){
    const menu = document.getElementsByClassName("file-options-menu")[0] as HTMLElement;
    if (this.fileOptionsShouldBeVisible == false){
      menu.style.visibility = "visible";
      menu.style.height = "200px";
      this.fileOptionsShouldBeVisible = true;
      this.applyMargin(menu.querySelectorAll("div"));
    }
    else{
      menu.style.height = "0";
      this.fileOptionsShouldBeVisible = false;
      setTimeout(()=>{
        menu.style.visibility = "hidden";
      },800);
    }
  }

  applyMargin(options:NodeListOf<HTMLDivElement>){
    for (let i = 1; i<options.length; i++){
      console.log(options[i].textContent);
      options[i].style.marginTop = `${31*i}px`;
    }
  }

  renameEnter(){
    if (!this.renameEnter){
      return;
    }
    this.renaming = false;
  }
}
