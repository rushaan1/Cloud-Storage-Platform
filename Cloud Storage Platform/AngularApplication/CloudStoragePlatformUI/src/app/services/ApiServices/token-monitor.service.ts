import { Injectable, NgZone } from '@angular/core';
import { AccountService } from './account.service';
import { Utils } from '../../Utils';

@Injectable({
  providedIn: 'root'
})
export class TokenMonitorService {
  private monitorInterval: any = null;

  public setRefreshTrue() {
    localStorage.setItem("jwt_refreshing", Math.floor(Date.now() / 1000).toString());
  }

  public isRefreshing():boolean{
    const lastRefreshTime = localStorage.getItem('jwt_refreshing');
    if (lastRefreshTime){
      return (Math.floor(Date.now() / 1000) - parseInt(lastRefreshTime,10)) <= 4;
    }
    return false;
  }

  constructor(private accountService: AccountService, private ngZone: NgZone) {
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
    if (this.isRefreshing()) return;

    this.setRefreshTrue();
    this.accountService.refreshToken().subscribe({});
  }
}
