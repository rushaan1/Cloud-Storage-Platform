import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationDrawerComponent } from './navigation-drawer/navigation-drawer.component';
import { RouterLink } from '@angular/router';
import { AppRoutingModule } from '../app-routing.module';
import { ViewerComponent } from './viewer/viewer.component';
import { PanelComponent } from './panel/panel.component';
import { ItemsModule } from '../items/items.module';
import { NotificationCenterComponent } from './notification-center/notification-center.component';


@NgModule({
  declarations: [
    NavigationDrawerComponent,
    ViewerComponent,
    PanelComponent,
    NotificationCenterComponent
  ],
  imports: [
    CommonModule,
    AppRoutingModule,
    ItemsModule
  ],
  exports:[
    NavigationDrawerComponent,
    ViewerComponent,
    PanelComponent
  ]
})
export class WorkspaceModule { }
