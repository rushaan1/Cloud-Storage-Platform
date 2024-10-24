import { Component } from '@angular/core';

@Component({
  selector: 'panel',
  templateUrl: './panel.component.html',
  styleUrl: './panel.component.css'
})
export class PanelComponent {
  searchDarkenBorder(){
    (document.getElementsByClassName("search")[0] as HTMLElement).style.borderColor = "black";
  }

  searchLightenBorder(){
    (document.getElementsByClassName("search")[0] as HTMLElement).style.borderColor = "gray";
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
}
