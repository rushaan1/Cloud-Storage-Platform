import {AfterViewChecked, Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import {FormControl, Validators} from "@angular/forms";
import {EventService} from "../../services/event-service.service";
import {invalidCharacter, invalidFileNameChars} from "../../CustomValidators";
import {ActivatedRoute, Router} from "@angular/router";
import {BreadcrumbService} from "../../services/StateManagementServices/breadcrumb.service";

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
  @ViewChild('sortSelection') sortSelection!: ElementRef<HTMLSelectElement>;
  @ViewChild("sorting") sortingTxt!: ElementRef<HTMLSpanElement>;
  @ViewChild('sortParent') sortParent!: ElementRef<HTMLDivElement>;

  mouseHoveringOverPfp = false;
  onlyHiText = false;
  searchCrossVisibleDueToFocus = false;
  searchCrossVisibleDueToHover = false;

  searchFormControl = new FormControl("", [Validators.required, Validators.pattern(/\S/), invalidCharacter]);
  pfpDropdownShowing = false;
  crumbs:string[] = [];
  constructor(protected eventService:EventService, protected router: Router, private breadCrumbService:BreadcrumbService, protected route:ActivatedRoute){}

  ngOnInit(){
    this.showStartupWelcomeMsgWithPfpDropDown();
    this.breadCrumbService.breadcrumbs$.subscribe((crumbs)=>{
      this.crumbs = crumbs;
    });
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
    this.router.navigate(["searchFilter"], {queryParams:{q:this.searchFormControl.value}});
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

  searchClear(){
    this.searchFormControl.setValue("");
    this.searchDiv.nativeElement.classList.remove("red-search-border");
    if (this.crumbs[0]=="Search Results"){
      this.router.navigate(["filter", "home"]);
    }
  }

  sortingChanged(event:Event){
    const selectElem:HTMLSelectElement = event.target as HTMLSelectElement;
    const val = selectElem.value;
    const selected = selectElem.options[selectElem.selectedIndex];
    if (selected.className=="asc"){
      this.sortingTxt.nativeElement.innerHTML = selected.text.split(" (")[0] + "  &uarr;";
    }
    else{
      this.sortingTxt.nativeElement.innerHTML = selected.text.split(" (")[0] + "  &darr;";
    }
    this.sortParent.nativeElement.style.marginTop = "12px";
    localStorage.setItem("sort",val);
    this.sortSelection.nativeElement.selectedIndex = 0;
    this.router.navigate([], {
      relativeTo:this.route,
      queryParams: {
        sort: val
      },
      queryParamsHandling:'merge'
    });
    this.eventService.emit("reload viewer list");
  }

}
