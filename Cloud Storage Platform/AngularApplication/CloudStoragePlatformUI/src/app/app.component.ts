import {AfterViewInit, Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import {LoadingService} from "./services/StateManagementServices/loading.service";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements AfterViewInit {
  @ViewChild("navigationDrawer") navDrawer!: ElementRef<HTMLElement>;
  @ViewChild("loaderOverlay") loaderOverlay!: ElementRef<HTMLElement>;
  @ViewChild("routerContainer") routerContainer!: ElementRef<HTMLElement>;
  loading = false;
  miniset:boolean = false;
  initialMiniSet:boolean = false;
  title = 'CloudStoragePlatformUI';

  constructor(private loadingService:LoadingService) {}

  ngAfterViewInit(): void {
    this.loadingService.loading$.subscribe(loading => {
      this.loading = loading;
      if (!this.initialMiniSet){
        this.miniToggle(this.miniset);
        this.initialMiniSet = true;
      }
      // if (loading){
      //   window.scrollTo({behavior: 'instant', top: 0});
      // }
    });
  }


  miniToggle(event:boolean){
    this.miniset = event;
    this.navDrawer.nativeElement.style.width = this.miniset ? "50px" : "321px";
    if (this.miniset){
      this.navDrawer.nativeElement.style.minWidth = "54px";
      this.navDrawer.nativeElement.style.height = "";
      this.routerContainer.nativeElement.style.maxWidth = "calc(100vw - 54px)";
    }
    else{
      this.navDrawer.nativeElement.style.minWidth = "290px";
      this.navDrawer.nativeElement.style.height = `${document.documentElement.scrollHeight}px`;
      this.routerContainer.nativeElement.style.maxWidth = "calc(100vw - 290px)";
    }
  }
}
