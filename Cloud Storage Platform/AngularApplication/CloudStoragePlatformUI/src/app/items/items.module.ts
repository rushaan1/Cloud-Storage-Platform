import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileItemComponent } from './file-item/file-item.component';
import { WorkspaceModule } from '../workspace/workspace.module';
import {ReactiveFormsModule} from "@angular/forms";
import {ResponseInterceptor} from "../services/ApiServices/response.interceptor";
import {HTTP_INTERCEPTORS} from "@angular/common/http";
import { InfoComponent } from './info/info.component';
import {BreadcrumbsComponent} from "./breadcrumbs/breadcrumbs.component";
import {NotificationCenterComponent} from "../notification-center/notification-center.component";


@NgModule({
  declarations: [
    FileItemComponent,
    InfoComponent,
    BreadcrumbsComponent
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ResponseInterceptor,
      multi: true,
    },
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    NotificationCenterComponent
  ],
  exports:[
    FileItemComponent,
    BreadcrumbsComponent
  ]
})
export class ItemsModule { }
