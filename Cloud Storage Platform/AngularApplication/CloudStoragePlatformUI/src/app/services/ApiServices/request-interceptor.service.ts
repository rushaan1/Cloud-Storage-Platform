import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import {Observable, from, of, timer, take} from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';
import { Utils } from '../../Utils';
import { AccountService } from './account.service';
import {RefreshedService} from "../StateManagementServices/refreshed.service";

@Injectable()
export class RequestInterceptor implements HttpInterceptor {
  constructor(private refreshed:RefreshedService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const isApiCall = req.url.startsWith('https://localhost:7219/api/');
    let request = req;
    if (isApiCall) {
      request = req.clone({
        withCredentials: true
      });
      const exceptions = ["account/regenerate-jwt-token","account/login", "account/logout", "account/register", "account/send-verification-otp", "account/verify-email", "api/shared"];
      // Check if the request is one of the exceptions
      if (exceptions.some(exception => request.url.toLowerCase().includes(exception))) {
        return next.handle(request);
      }
      if (this.hasJwtExpired()){
        return this.refreshed.hasRefreshed$.pipe(
          filter((refreshed)=>refreshed),
          take(1),
          switchMap(()=>next.handle(request))
        );
      }
    }
    return next.handle(request);
  }

  hasJwtExpired(): boolean {
    const expiryStr = Utils.getCookie('jwt_expiry');
    const now = Math.floor(Date.now() / 1000);
    let expired = false;
    if (expiryStr) {
      const expiry = parseInt(expiryStr, 10);
      expired = isNaN(expiry) || expiry <= now;
    }
    return expired;
  }
}
