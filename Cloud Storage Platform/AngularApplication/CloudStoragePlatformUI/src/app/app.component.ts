import {AfterViewInit, ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {LoadingService} from "./services/StateManagementServices/loading.service";
import {NavigationEnd, Router} from "@angular/router";
import {filter} from "rxjs/operators";
import {TokenMonitorService} from "./services/ApiServices/token-monitor.service";
import {RefreshedService} from "./services/StateManagementServices/refreshed.service";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements AfterViewInit, OnInit {
  @ViewChild("navigationDrawer") navDrawer!: ElementRef<HTMLElement>;
  @ViewChild("loaderOverlay") loaderOverlay!: ElementRef<HTMLElement>;
  @ViewChild("routerContainer") routerContainer!: ElementRef<HTMLElement>;
  @ViewChild("panel") panel!: ElementRef<HTMLElement>;
  loading = false;
  miniset:boolean = false;
  initialMiniSet:boolean = false;
  title = 'CloudStoragePlatformUI';
  isAuthPage = false;
  isAccDashboard = false;
  private lastUrl = '';

  constructor(
    private loadingService: LoadingService,
    private router: Router,
    private cd: ChangeDetectorRef,
    private tokenMonitor: TokenMonitorService,
    private refreshed: RefreshedService
  ) {}

  ngOnInit(): void {
    this.tokenMonitor.startMonitoring();
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      const wasAuthPage = this.isAuthPage;
      this.isAuthPage = event.url.includes('/account/login') || event.url.includes('/account/register');
      this.isAccDashboard = event.url.includes('/account/dashboard');
      this.lastUrl = event.url;

      // Only apply miniToggle when entering non-auth pages, not when leaving them
      if (!this.isAuthPage && (wasAuthPage !== this.isAuthPage)) {
        // Wait for navDrawer to be rendered after *ngIf condition changes
        setTimeout(() => {
          this.miniToggle(this.miniset);
        }, 0);
      }
    });
    window.addEventListener('beforeunload', this.handleBeforeUnload.bind(this));
  }

  handleBeforeUnload() {
    if (this.refreshed.currentTabRefreshing){
      localStorage.setItem("isRefreshing", "n");
    }
  }

  ngAfterViewInit(): void {
    this.loadingService.loading$.subscribe(loading => {
      this.loading = loading;
      if (!this.initialMiniSet && !this.isAuthPage){
        this.miniToggle(this.miniset);
        this.initialMiniSet = true;
      }

      if (this.panel && this.panel.nativeElement) {
        this.panel.nativeElement.scrollIntoView({behavior:'instant', block: 'start'});
      }
    });
  }

  miniToggle(event:boolean){
    this.miniset = event;

    // Skip this method entirely if we're on an auth page or if elements are undefined
    if (this.isAuthPage || !this.navDrawer || !this.navDrawer.nativeElement || !this.routerContainer || !this.routerContainer.nativeElement) {
      return;
    }

    if (this.miniset){
      this.navDrawer.nativeElement.style.minWidth = "50px";
      this.navDrawer.nativeElement.style.height = "";
      this.routerContainer.nativeElement.style.maxWidth = "calc(100vw - 50px)";
      localStorage.setItem("miniDrawerSet", "Y");
    }
    else{
      this.navDrawer.nativeElement.style.minWidth = "270px";
      this.navDrawer.nativeElement.style.height = `${document.documentElement.scrollHeight}px`;
      this.routerContainer.nativeElement.style.maxWidth = "calc(100vw - 270px)";
      localStorage.setItem("miniDrawerSet", "N");
    }
    this.cd.detectChanges();
  }

}
