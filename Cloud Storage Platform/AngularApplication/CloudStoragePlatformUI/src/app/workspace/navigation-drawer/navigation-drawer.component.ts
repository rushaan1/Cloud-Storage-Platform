import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';
import {EventService} from "../../services/event-service.service";

@Component({
  selector: 'drawer',
  templateUrl: './navigation-drawer.component.html',
  styleUrl: './navigation-drawer.component.css'
})
export class NavigationDrawerComponent implements OnInit {
  @Output() miniSet = new EventEmitter<boolean>();
  selectedTypeItems: string[] = [];
  constructor(private router: Router, protected eventService:EventService) {

  }
  ngOnInit(): void {
    const miniDrawer = document.getElementsByClassName("mini-drawer")[0] as HTMLElement;
    miniDrawer.style.translate = "-100%";
    // bg means loader's bg
    setTimeout(()=>{
      (document.getElementsByClassName("bg")[0] as HTMLElement).style.width = "50%";
    console.log(miniDrawer.style.translate);
    },100);
  }

  hoverTransitionTrigger(event: MouseEvent) {
    if (event) {
      const targetElement = event.currentTarget as HTMLElement;
      const inputElement = targetElement.querySelector('input[type="checkbox"]') as HTMLInputElement;
      const aElement = targetElement.querySelector("a") as HTMLAnchorElement;
      const iElement = aElement.getElementsByTagName("i")[0] as HTMLElement;
      if (inputElement) {
        inputElement.style.visibility = "visible";
        inputElement.style.marginLeft = "15px";
      }
      if (targetElement.classList.contains("m-cat-tab") == false) {
        aElement.style.paddingLeft = "21px"
      }
      if (aElement && this.isSelected(targetElement) == false) {
        this.setBackgroundColors(aElement, targetElement, iElement, "antiquewhite");
      }
    }
  }

  hoverTransitionOppositeTrigger(event: MouseEvent) {
    if (event) {
      const targetElement = event.currentTarget as HTMLElement;
      const inputElement = targetElement.querySelector('input[type="checkbox"]') as HTMLInputElement;
      const aElement = targetElement.querySelector("a") as HTMLAnchorElement;
      const iElement = aElement.getElementsByTagName("i")[0] as HTMLElement;
      console.log(targetElement);
      if (inputElement) {
        inputElement.style.visibility = "hidden";
        inputElement.style.marginLeft = "0";
      }
      if (aElement) {
        aElement.style.paddingLeft = "0"

        if (this.isSelected(targetElement) == false) {
          this.setBackgroundColors(aElement, targetElement, iElement, "");
        }
      }
    }
  }

  typeItemSelected(event: MouseEvent, liIndex: number) {
    var checkbox = document.getElementById(`${liIndex}-NavCheckbox`) as HTMLInputElement;
    const lis = document.getElementsByClassName("maindrawer-bottom")[0].getElementsByTagName("li");

    const li = lis[liIndex];
    const aLi = li.getElementsByTagName("a")[0] as HTMLAnchorElement; // corresponding anchor tag
    const iElement = aLi.getElementsByTagName("i")[0] as HTMLElement;
    const type = li.textContent?.trim();

    this.itemSelection(li, type, aLi, iElement, false, checkbox);
    this.hoverTransitionTrigger(event);
    console.log(this.selectedTypeItems);
  }

  itemSelection(li: HTMLElement, type: string | undefined, aLi: HTMLElement, iElement: HTMLElement, mini: boolean, checkbox: any | undefined) {
    if (li.style.backgroundColor != "bisque") {
      if (type) {
        this.selectedTypeItems.push(type);
      }
      if (!mini) {
        checkbox.checked = true;
      }
      this.setBackgroundColors(li, aLi, iElement, "bisque");
    }

    else if (li.style.backgroundColor == "bisque") {
      if (type) {
        this.selectedTypeItems.splice(this.selectedTypeItems.indexOf(type), 1);
      }
      if (!mini) {
        checkbox.checked = false;
      }
      this.setBackgroundColors(aLi, li, iElement, "antiquewhite");
      li.style.backgroundColor = "";
    }
  }

  typeItemSelectedMini(event: MouseEvent, name: string, liIndex: number) {
    const lis = document.getElementsByClassName("m-bottom-cat")[0].getElementsByTagName("li");
    const li = lis[liIndex];
    const aLi = li.getElementsByTagName("a")[0] as HTMLAnchorElement; // corresponding anchor tag
    const iElement = aLi.getElementsByTagName("i")[0] as HTMLElement;

    this.itemSelection(li, name, aLi, iElement, true, undefined);
    this.hoverTransitionTrigger(event);
    console.log(this.selectedTypeItems);
  }


  isSelected(targetElement: HTMLElement): any {
    var content = targetElement.textContent?.trim();
    // console.log(targetElement);
    if (content == null || content == undefined || content == "") {
      content = targetElement.id;
    }
    const index = this.selectedTypeItems.indexOf(content);
    if (index != -1) {
      return true;
    }
    return false
  }

  setBackgroundColors(first: HTMLElement, second: HTMLElement, third: HTMLElement, value: string) {
    if (value == "") {
      value = "transparent"
    }
    first.style.backgroundColor = value;
    second.style.backgroundColor = value;
    third.style.backgroundColor = value;
  }

  topCategoryLiHover(event: MouseEvent) {
    const targetElement = event.currentTarget as HTMLElement;
    const aElement = targetElement.querySelector("a") as HTMLAnchorElement;
    const iElement = aElement.getElementsByTagName("i")[0] as HTMLElement;
    if (targetElement.classList.contains("active")){
      return;
    }
    this.setBackgroundColors(targetElement, aElement, iElement, "antiquewhite");
  }

  topCategoryLiHoverAway(event: MouseEvent) {
    const targetElement = event.currentTarget as HTMLElement;
    const aElement = targetElement.querySelector("a") as HTMLAnchorElement;
    const iElement = aElement.getElementsByTagName("i")[0] as HTMLElement;
    this.setBackgroundColors(targetElement, aElement, iElement, "");
  }

  visibilityToggle(navDrawer: HTMLElement, visibility: string) {
    navDrawer.style.visibility = visibility;
    (document.getElementsByClassName("maindrawer-top")[0] as HTMLElement).style.visibility = visibility;
    (document.getElementsByClassName("maindrawer-bottom")[0] as HTMLElement).style.visibility = visibility;
  }

  drawerClick() {
    const navDrawer = document.getElementsByClassName("drawer")[0] as HTMLElement;
    const miniDrawer = document.getElementsByClassName("mini-drawer")[0] as HTMLElement;
    setTimeout(() => {
      this.miniSet.emit(true);
      this.visibilityToggle(navDrawer, "hidden");
    }, 700);
    miniDrawer.style.visibility = "visible";
    navDrawer.style.translate = "-300%";
    miniDrawer.style.translate = "0%"

    const miniTabs = document.getElementsByClassName("m-cat-tab");

    this.retainingSelection(miniTabs);
  }

  miniDrawerClick() {
    const navDrawer = document.getElementsByClassName("drawer")[0] as HTMLElement;
    const miniDrawer = document.getElementsByClassName("mini-drawer")[0] as HTMLElement;

    this.visibilityToggle(navDrawer, "visible");
    navDrawer.style.translate = "0%";
    miniDrawer.style.translate = "-100%"
    miniDrawer.style.visibility = "hidden";

    this.miniSet.emit(false);

    this.retainingSelection(document.getElementsByClassName("category-tab"));
  }

  retainingSelection(typeOptions: any) {
    for (let i = 0; i < typeOptions.length; i++) {
      let tab = typeOptions[i] as HTMLElement;

      const aElement = tab.querySelector("a") as HTMLAnchorElement;
      const iElement = aElement.getElementsByTagName("i")[0] as HTMLElement;
      const input = tab.getElementsByTagName("input")[0];

      if (this.selectedTypeItems.indexOf(tab.id) != -1) {
        this.setBackgroundColors(tab, aElement, iElement, "bisque");

        if (input != undefined && input != null) {
          input.checked = true;
        }
      }
      else {
        this.setBackgroundColors(tab, aElement, iElement, "");
        if (input != undefined && input != null) {
          input.checked = false;
        }
      }
    }
  }
}
