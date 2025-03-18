import { Injectable } from "@angular/core";
import {Observable, Subject, Subscription} from "rxjs";

@Injectable({ providedIn: "root" })
export class EventService {
  private subject = new Subject();

  emit(eventName: string, payload: any=null, callback?: (event: any) => void) {
    if (payload==null && callback==null){
      this.subject.next({ eventName });
      return;
    }
    else if (payload==null && callback!=null){
      this.subject.next({ eventName, callback });
      return;
    }
    else if (payload!=null && callback==null){
      this.subject.next({ eventName, payload });
      return;
    }
    this.subject.next({ eventName, payload, callback });
  }

  listen(eventName: string, callback: (payload: any, callBackFn?:()=>void) => void):Subscription {
    return this.subject.asObservable().subscribe((obj: any) => {
      if (eventName === obj.eventName) {
        callback(obj.payload, obj.callback);
      }
    });
  }
}
