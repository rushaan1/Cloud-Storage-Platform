import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileLargeComponent } from './file-large/file-large.component';
import { FileListComponent } from './file-list/file-list.component';
import { WorkspaceModule } from '../workspace/workspace.module';
import {ReactiveFormsModule} from "@angular/forms";
import {ResponseInterceptor} from "../services/ApiServices/response.interceptor";
import {HTTP_INTERCEPTORS} from "@angular/common/http";


@NgModule({
  declarations: [
    FileLargeComponent,
    FileListComponent
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
        ReactiveFormsModule
    ],
  exports:[
    FileLargeComponent,
    FileListComponent
  ]
})
export class ItemsModule { }
