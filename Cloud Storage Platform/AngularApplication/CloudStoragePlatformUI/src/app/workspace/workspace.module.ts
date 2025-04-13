import { NgModule } from '@angular/core';
import {CommonModule, NgOptimizedImage} from '@angular/common';
import { NavigationDrawerComponent } from './navigation-drawer/navigation-drawer.component';
import { RouterLink } from '@angular/router';
import { AppRoutingModule } from '../app-routing.module';
import { ViewerComponent } from './viewer/viewer.component';
import { PanelComponent } from './panel/panel.component';
import { ItemsModule } from '../items/items.module';
import { NotificationCenterComponent } from '../notification-center/notification-center.component';
import {ReactiveFormsModule} from "@angular/forms";
import {HttpClientModule} from "@angular/common/http";
import { BreadcrumbsComponent } from '../items/breadcrumbs/breadcrumbs.component';
import { PreviewComponent } from './preview/preview.component';


@NgModule({
  declarations: [
    NavigationDrawerComponent,
    ViewerComponent,
    PanelComponent,
    PreviewComponent,
  ],
  imports: [
    CommonModule,
    AppRoutingModule,
    ItemsModule,
    ReactiveFormsModule,
    HttpClientModule,
    NotificationCenterComponent,
    NgOptimizedImage
  ],
  exports: [
    NavigationDrawerComponent,
    ViewerComponent,
    PanelComponent,
  ]
})
export class WorkspaceModule { }
