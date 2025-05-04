import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { WorkspaceModule } from './workspace/workspace.module';
import { NotificationCenterComponent } from './notification-center/notification-center.component';
import {AccountModule} from "./account/account.module";

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
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
