import { Injectable, NgZone } from '@angular/core';
import { AccountService } from './account.service';
import { Utils } from '../../Utils';
import {BehaviorSubject} from "rxjs";
import {RefreshedService} from "../StateManagementServices/refreshed.service";

@Injectable({
  providedIn: 'root'
})
export class TokenMonitorService {
  private monitorInterval: any = null;

  constructor(private accountService: AccountService, private ngZone: NgZone, private refreshed:RefreshedService) {
  }

  startMonitoring() {
    this.stopMonitoring();
    this.ngZone.runOutsideAngular(() => {
      this.monitorInterval = setInterval(() => this.monitor(), 1000);
      this.monitor();
    });
  }

  stopMonitoring() {
    if (this.monitorInterval) {
      clearInterval(this.monitorInterval);
      this.monitorInterval = null;
    }
  }

  private monitor() {
    const expiryStr = Utils.getCookie('jwt_expiry');
    const refreshTokenExpiryStr = Utils.getCookie('refresh_expiry');
    if (!expiryStr || !refreshTokenExpiryStr) {
      return;
    }
    const expiry = parseInt(expiryStr, 10);
    const refreshTokenExpiry = parseInt(refreshTokenExpiryStr, 10);
    if (isNaN(expiry) || expiry <= 0 || isNaN(refreshTokenExpiry) || refreshTokenExpiry <= 0) {
      return;
    }
    const now = Math.floor(Date.now() / 1000);
    const refreshTime = (expiry - 15) - now; // 6 minutes before expiry
    if (refreshTime <= 0 || expiry > refreshTokenExpiry) {
      // refresh 6 mins before JWT's expiry or if the refresh token itself is expiring before JWT because each refresh returns new JWT & new refresh token with new expiries
      this.safeRefresh();
    }
  }

  private safeRefresh() {
    const lastRefreshTime = localStorage.getItem('jwt_last_refreshed');
    let morethan4s_sinceLastRefresh = true;
    if (lastRefreshTime){
      morethan4s_sinceLastRefresh = (Math.floor(Date.now() / 1000) - parseInt(lastRefreshTime,10)) > 4;
    }
    if (localStorage.getItem("isRefreshing")!="y" && morethan4s_sinceLastRefresh){
      localStorage.setItem("isRefreshing", "y");
      this.accountService.refreshToken().subscribe({
        next: (res) => {
          localStorage.setItem("jwt_last_refreshed", Math.floor(Date.now() / 1000).toString());
          localStorage.setItem("isRefreshing", "n");
          this.refreshed.setHasRefreshed(true);
        },
        complete: () => {
          localStorage.setItem("jwt_last_refreshed", Math.floor(Date.now() / 1000).toString());
          localStorage.setItem("isRefreshing", "n");
          this.refreshed.setHasRefreshed(true);
        },
        error: (err) => {
          console.error('Token refresh error', err);
          localStorage.setItem("isRefreshing", "n");
        }
      });
    }
  }
}
