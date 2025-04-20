import { Injectable } from '@angular/core';
import {BehaviorSubject} from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private loading = new BehaviorSubject<boolean>(false);
  loading$ = this.loading.asObservable();
  constructor() { }

  loadingStart(){
    this.loading.next(true);
  }

  loadingEnd(){
    this.loading.next(false);
  }

  getLoadingStatus(){
    return this.loading.getValue();
  }
}
