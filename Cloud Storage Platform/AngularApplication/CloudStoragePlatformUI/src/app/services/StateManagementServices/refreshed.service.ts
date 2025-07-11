import { Injectable } from '@angular/core';
import {BehaviorSubject} from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class RefreshedService {
  private _hasRefreshed = new BehaviorSubject<boolean>(false);
  hasRefreshed$ = this._hasRefreshed.asObservable();

  public setHasRefreshed(value: boolean) {
    this._hasRefreshed.next(value);
    if (value){
      setTimeout(()=>this._hasRefreshed.next(false),1000); // Reset after emitting true
    }
  }

  constructor() { }
}
