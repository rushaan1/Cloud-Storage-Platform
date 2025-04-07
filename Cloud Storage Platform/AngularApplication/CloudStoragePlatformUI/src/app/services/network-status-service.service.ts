import { Injectable, NgZone } from '@angular/core';
import { BehaviorSubject, fromEvent, merge, Observable, of } from 'rxjs';
import {debounceTime, map, mapTo} from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import {EventService} from "./event-service.service";

@Injectable({
  providedIn: 'root',
})
export class NetworkStatusService {
  private onlineStatus = new BehaviorSubject<boolean>(navigator.onLine);
  private previousStatus:boolean = true;
  constructor(private ngZone: NgZone) {
    const online$ = fromEvent(window, 'online').pipe(map(()=>true));
    const offline$ = fromEvent(window, 'offline').pipe(map(()=>false));

    merge(online$, offline$).subscribe(status => {
      this.ngZone.run(() => {
        if (this.previousStatus!=status){
          this.onlineStatus.next(status);
        }
        this.previousStatus = status;
      });
    });
  }

  getStatus(): Observable<boolean> {
    return this.onlineStatus.asObservable();
  }

  statusVal():boolean{
    return this.onlineStatus.value;
  }
}
