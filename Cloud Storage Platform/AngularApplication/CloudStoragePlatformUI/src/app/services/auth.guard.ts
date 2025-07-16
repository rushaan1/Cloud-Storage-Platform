import {CanActivateFn, Router} from '@angular/router';
import {Utils} from "../Utils";
import {inject} from "@angular/core";

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const refreshExpiry = Utils.getCookie("refresh_expiry");
  if (refreshExpiry && Math.floor(Date.now() / 1000) < parseInt(refreshExpiry,10)){
    return true;
  }
  else {
    return router.parseUrl("account/login");
  }
};
