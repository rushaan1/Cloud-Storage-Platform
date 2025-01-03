import {Component, ElementRef, ViewChild} from '@angular/core';
import {FormControl, Validators} from "@angular/forms";
import {EventService} from "../../services/event-service.service";
import {invalidCharacter, invalidFileNameChars} from "../../CustomValidators";

@Component({
  selector: 'panel',
  templateUrl: './panel.component.html',
  styleUrl: './panel.component.css'
})
export class PanelComponent {
  @ViewChild('searchDiv') searchDiv!: ElementRef<HTMLInputElement>;
  searchFormControl = new FormControl("", [Validators.required, Validators.pattern(/\S/), invalidCharacter]);

  constructor(public eventService:EventService){}

  searchDarkenBorder(){
    this.searchDiv.nativeElement.style.borderColor = "black";
    this.searchDiv.nativeElement.classList.remove("red-search-border");
  }

  searchLightenBorder(){
    this.searchDiv.nativeElement.style.borderColor = "gray";
  }

  styleChange(){
    if (localStorage["style"]!=undefined && localStorage["style"]!=null && localStorage["style"]!=""){
      if (localStorage["style"]=="large"){
        localStorage["style"] = "list";
      }
      else{
        localStorage["style"] = "large";
      }
    }
    else{
      localStorage["style"] = "list";
    }
    console.log(localStorage["style"]);
  }

  searchClick(){
    if (this.searchFormControl.invalid){
      if (this.searchFormControl.hasError("invalidCharacter")){
        this.eventService.emit("invalidCharacterNotif", this.searchFormControl.errors?.['invalidCharactersString']);
      }
      else{
        this.eventService.emit("emptyInputNotif");
      }
      this.searchDiv.nativeElement.classList.add("red-search-border");
    }
  }
}
