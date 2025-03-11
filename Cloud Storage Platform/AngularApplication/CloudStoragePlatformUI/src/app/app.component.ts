import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  ngOnInit(): void {
    this.miniToggle(false);
  }

  title = 'CloudStoragePlatformUI';
  miniset:boolean = false;

  miniToggle(event:boolean){
    this.miniset = event;
    const navDrawer = document.getElementsByClassName("navigation-drawer")[0] as HTMLElement;
    navDrawer.style.width = this.miniset ? "50px" : "321px";
    if (this.miniset){
      (document.getElementsByClassName("navigation-drawer")[0] as HTMLElement).style.minWidth = "54px";
      navDrawer.style.height = "";
    }
    else{
      (document.getElementsByClassName("navigation-drawer")[0] as HTMLElement).style.minWidth = "290px";
      navDrawer.style.height = `${document.documentElement.scrollHeight}px`;
    }
  }
}
