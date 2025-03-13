import {AfterViewChecked, Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import {FormControl, Validators} from "@angular/forms";
import {EventService} from "../../services/event-service.service";
import {invalidCharacter, invalidFileNameChars} from "../../CustomValidators";

@Component({
  selector: 'panel',
  templateUrl: './panel.component.html',
  styleUrl: './panel.component.css'
})
export class PanelComponent implements OnInit, AfterViewChecked {
  @ViewChild('searchDiv') searchDiv!: ElementRef<HTMLInputElement>;
  @ViewChild('pfpDropdown') pfpDropdownDiv!: ElementRef<HTMLDivElement>;
  @ViewChild('panelMainFlex') panelMainFlex!: ElementRef<HTMLDivElement>;
  @ViewChild('pfp') pfp!: ElementRef<HTMLDivElement>;
  @ViewChild('pfpDropDownSpan') pfpDropDownSpan!: ElementRef<HTMLSpanElement>;
  mouseHoveringOverPfp = false;
  onlyHiText = false;

  searchFormControl = new FormControl("", [Validators.required, Validators.pattern(/\S/), invalidCharacter]);
  pfpDropdownShowing = false;
  constructor(public eventService:EventService){}

  ngOnInit(){
    this.showStartupWelcomeMsgWithPfpDropDown();
  }

  ngAfterViewChecked(){
    if (this.pfpDropdownShowing && this.pfpDropdownDiv.nativeElement){
      const rect = this.pfp.nativeElement.getBoundingClientRect();
      this.pfpDropdownDiv.nativeElement.style.right = `${rect.top}px`;
      if (this.onlyHiText){
        this.pfpDropDownSpan.nativeElement.innerText = "Hi FirstName";
      }
      else{
        this.pfpDropDownSpan.nativeElement.innerHTML = "Hi FirstName, <br> <b>Click to open account settings</b>";
      }
    }
  }

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
        this.eventService.emit("addNotif",["Invalid character in input: "+this.searchFormControl.errors?.['invalidCharactersString'], 8000]);
      }
      else{
        this.eventService.emit("addNotif",["Input cannot be empty", 8000]);
      }
      this.searchDiv.nativeElement.classList.add("red-search-border");
    }
  }

  showPfpDropdown(onlyHiText:boolean){
    this.pfpDropdownShowing = true;
    this.onlyHiText = onlyHiText;
  }

  hidePfpDropdown(){
    this.pfpDropdownShowing = false;
  }

  showStartupWelcomeMsgWithPfpDropDown(){
    this.showPfpDropdown(true);

    setTimeout(() => {
      if (!this.mouseHoveringOverPfp){
        this.hidePfpDropdown();
      }
    }, 8000);
  }
}
