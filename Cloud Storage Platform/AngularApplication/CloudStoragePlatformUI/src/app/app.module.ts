import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { WorkspaceModule } from './workspace/workspace.module';
import { NotificationCenterComponent } from './notification-center/notification-center.component';
import {AccountModule} from "./account/account.module";
import {HTTP_INTERCEPTORS} from "@angular/common/http";
import {WithCredentialsInterceptor} from "./services/ApiServices/with-credentials.interceptor";

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    WorkspaceModule,
    AccountModule
  ],
  providers: [
    {provide: HTTP_INTERCEPTORS, useClass: WithCredentialsInterceptor, multi: true}
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
