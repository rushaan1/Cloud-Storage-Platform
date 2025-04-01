import {Component, ElementRef, Input, OnInit, ViewChild} from '@angular/core';
import {EventService} from "../../services/event-service.service";
import {Router} from "@angular/router";
import {Utils} from "../../Utils";

@Component({
  selector: 'breadcrumbs',
  templateUrl: './breadcrumbs.component.html',
  styleUrl: './breadcrumbs.component.css'
})
export class BreadcrumbsComponent implements OnInit {
  @Input() crumbs!:string[];
  @ViewChild('breadcrumbs') breadcrumbs!: ElementRef<HTMLDivElement>;
  protected readonly decodeURIComponent = decodeURIComponent;

  constructor(private eventService: EventService, private router: Router) {}

  ngOnInit(): void {
    this.initializeBreadcrumbs();
  }

  routeTo(i:number){
    let navigateRouteParams = this.crumbs.slice(0, i+1);
    for (let i = 0; i < navigateRouteParams.length; i++) {
      navigateRouteParams[i] = decodeURIComponent(navigateRouteParams[i]);
    }
    this.router.navigate(['folder', ...navigateRouteParams]);
  }

  initializeBreadcrumbs() {
    setTimeout(() => {
      if (this.breadcrumbs.nativeElement.scrollWidth > this.breadcrumbs.nativeElement.clientWidth){
        this.breadcrumbs.nativeElement.classList.add('breadcrumb-scroll-active');
        this.breadcrumbs.nativeElement.scrollTo({left: this.breadcrumbs.nativeElement.scrollWidth, behavior:'smooth'});
      }
      else{
        this.breadcrumbs.nativeElement.classList.remove('breadcrumb-scroll-active');
      }
    }, 100);
  }

  protected readonly Utils = Utils;
}
