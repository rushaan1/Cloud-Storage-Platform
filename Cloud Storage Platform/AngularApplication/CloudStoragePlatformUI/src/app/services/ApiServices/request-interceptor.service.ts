import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable, from, of, timer } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';
import { Utils } from '../../Utils';
import { AccountService } from './account.service';

@Injectable()
export class RequestInterceptor implements HttpInterceptor {
  constructor(private accountService: AccountService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const isApiCall = req.url.startsWith('https://localhost:7219/api/');
    let request = req;
    if (isApiCall) {
      request = req.clone({
        withCredentials: true
      });
      const exceptions = ["account/regenerate-jwt-token","account/login", "account/logout", "account/register"];
      // Check if the request is one of the exceptions
      if (exceptions.some(exception => request.url.toLowerCase().includes(exception))) {
        return next.handle(request);
      }
      return from(this.handleJwtAndRefresh(request, next));
    }
    return next.handle(request);
  }

  private async handleJwtAndRefresh(request: HttpRequest<any>, next: HttpHandler): Promise<HttpEvent<any>> {
    const expiryStr = Utils.getCookie('jwt_expiry');
    const now = Math.floor(Date.now() / 1000);
    let expired = false;
    if (expiryStr) {
      const expiry = parseInt(expiryStr, 10);
      expired = isNaN(expiry) || expiry <= now;
    }

    if (expired) {
      let lastRefresh = localStorage.getItem('jwt_refreshing');
      let lastRefreshTime = lastRefresh ? parseInt(lastRefresh, 10) : 0;
      let diff = now - lastRefreshTime;
      if (diff > 4) {
        // Set refresh time and call refresh
        localStorage.setItem('jwt_refreshing', now.toString());
        await this.accountService.refreshToken().toPromise();
      } else {
        // Wait until 4 seconds have passed since last refresh
        await new Promise(resolve => setTimeout(resolve, (4 - diff) * 1000));
      }
    }
    // Continue with the original request
    const result = await next.handle(request).toPromise();
    if (!result) {
      throw new Error('No HttpEvent returned from next.handle(request)');
    }
    return result;
  }
}
