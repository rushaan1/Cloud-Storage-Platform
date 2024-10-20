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
    navDrawer.style.width = event ? "50px" : "321px";
  }
}
