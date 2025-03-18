import { Injectable } from '@angular/core';
import {BehaviorSubject} from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class BreadcrumbService {
  private breadcrumbs = new BehaviorSubject<string[]>([]);
  breadcrumbs$ = this.breadcrumbs.asObservable();
  constructor() { }

  setBreadcrumbs(breadcrumbs:string[]){
    this.breadcrumbs.next(breadcrumbs);
  }

  getBreadcrumbs():string[] {
    return this.breadcrumbs.value;
  }

}
