import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'drawer',
  templateUrl: './navigation-drawer.component.html',
  styleUrl: './navigation-drawer.component.css'
})
export class NavigationDrawerComponent {
  selectedTypeItems: string[] = [];
  constructor(private router: Router) {

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
      aElement.style.paddingLeft = "21px"
      if (aElement && this.isSelected(targetElement)==false) {
        // aElement.style.backgroundColor = "antiquewhite";
        // targetElement.style.backgroundColor = "antiquewhite";
        // iElement.style.backgroundColor = "antiquewhite";
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

      if (inputElement) {
        inputElement.style.visibility = "hidden";
        inputElement.style.marginLeft = "0";
      }
      if (aElement) {
        aElement.style.paddingLeft = "0"

        if (this.isSelected(targetElement)==false) {
          // aElement.style.backgroundColor = "";
          // iElement.style.backgroundColor = "";
          // targetElement.style.backgroundColor = "";
          this.setBackgroundColors(aElement, targetElement, iElement, "");
        }
      }
    }
  }

  typeItemSelected(event: MouseEvent, liIndex: number) {
    const checkbox = document.getElementById(`${liIndex}-NavCheckbox`) as HTMLInputElement;
    const lis = document.getElementsByClassName("bottom-category")[0].getElementsByTagName("li");
    const li = lis[liIndex];
    const aLi = li.getElementsByTagName("a")[0] as HTMLAnchorElement; // corresponding anchor tag
    const iElement = aLi.getElementsByTagName("i")[0] as HTMLElement;
    const type = li.textContent?.trim();

    if (li.style.backgroundColor != "bisque") {
      if (type) {
        this.selectedTypeItems.push(type);
      }
      checkbox.checked = true;

      // li.style.backgroundColor = "bisque";
      // aLi.style.backgroundColor = "bisque";
      // iElement.style.backgroundColor = "bisque";
      this.setBackgroundColors(li, aLi, iElement, "bisque");
    }
    else if (li.style.backgroundColor == "bisque") {
      if (type) {
        this.selectedTypeItems.splice(this.selectedTypeItems.indexOf(type), 1);
      }
      li.style.backgroundColor = "";
      checkbox.checked = false;
      // aLi.style.backgroundColor = "antiquewhite";
      // iElement.style.backgroundColor = "antiquewhite";
      // li.style.backgroundColor = "antiquewhite";
      this.setBackgroundColors(aLi, li, iElement, "antiquewhite");
      
    }
    console.log(this.selectedTypeItems);
  }

  isSelected(targetElement: HTMLElement): any {
    const content = targetElement.textContent?.trim();
    if (content != null) {
      const index = this.selectedTypeItems.indexOf(content);
      if (index != -1) {
        return true;
      }
      return false
    }
    return Error("Null content");
  }

  setBackgroundColors(first:HTMLElement, second:HTMLElement, third:HTMLElement, value:string){
    if (value==""){
      value = "transparent"
    }
    first.style.backgroundColor = value;
    second.style.backgroundColor = value;
    third.style.backgroundColor = value;
  }

  topCategoryLiHover(event:MouseEvent){
    const targetElement = event.currentTarget as HTMLElement;
    const aElement = targetElement.querySelector("a") as HTMLAnchorElement;
    const iElement = aElement.getElementsByTagName("i")[0] as HTMLElement;

    this.setBackgroundColors(targetElement,aElement,iElement,"antiquewhite");
  }

  topCategoryLiHoverAway(event:MouseEvent){
    const targetElement = event.currentTarget as HTMLElement;
    const aElement = targetElement.querySelector("a") as HTMLAnchorElement;
    const iElement = aElement.getElementsByTagName("i")[0] as HTMLElement;

    this.setBackgroundColors(targetElement,aElement,iElement,"");
  }
}
