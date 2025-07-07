import { Injectable, NgZone } from '@angular/core';
import { AccountService } from './account.service';
import { Utils } from '../../Utils';

@Injectable({
  providedIn: 'root'
})
export class JwtMonitorService {
  private timer: any = null;
  private lastExpiry: number | null = null;
  private monitorInterval: any = null;

  constructor(private accountService: AccountService, private ngZone: NgZone) {}

  startMonitoring() {
    this.stopMonitoring();
    this.ngZone.runOutsideAngular(() => {
      this.monitorInterval = setInterval(() => this.checkAndSchedule(), 5000);
      this.checkAndSchedule();
    });
  }

  stopMonitoring() {
    if (this.timer) {
      clearTimeout(this.timer);
      this.timer = null;
    }
    if (this.monitorInterval) {
      clearInterval(this.monitorInterval);
      this.monitorInterval = null;
    }
    this.lastExpiry = null;
  }

  private checkAndSchedule() {
    const expiryStr = Utils.getCookie('jwt_expiry');
    if (!expiryStr) {
      this.clearTimer();
      this.lastExpiry = null;
      return;
    }
    const expiry = parseInt(expiryStr, 10);
    if (isNaN(expiry) || expiry <= 0) {
      this.clearTimer();
      this.lastExpiry = null;
      return;
    }
    if (this.lastExpiry !== expiry) {
      this.lastExpiry = expiry;
      this.clearTimer();
      const now = Math.floor(Date.now() / 1000);
      const refreshTime = (expiry - 120) - now; // 2 minutes before expiry
      if (refreshTime > 0) {
        this.timer = setTimeout(() => {
          this.accountService.refreshToken().subscribe({
            next: () => this.checkAndSchedule(),
            error: () => this.checkAndSchedule()
          });
        }, refreshTime * 1000);
      } else {
        // If already expired or less than 2 minutes, refresh immediately
        this.accountService.refreshToken().subscribe({
          next: () => this.checkAndSchedule(),
          error: () => this.checkAndSchedule()
        });
      }
    }
  }

  private clearTimer() {
    if (this.timer) {
      clearTimeout(this.timer);
      this.timer = null;
    }
  }
} 