import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileLargeComponent } from './file-large/file-large.component';
import { FileListComponent } from './file-list/file-list.component';
import { WorkspaceModule } from '../workspace/workspace.module';
import {ReactiveFormsModule} from "@angular/forms";


@NgModule({
  declarations: [
    FileLargeComponent,
    FileListComponent
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
