import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileLargeComponent } from './file-large/file-large.component';
import { FileListComponent } from './file-list/file-list.component';



@NgModule({
  declarations: [
    FileLargeComponent,
    FileListComponent
  ],
  imports: [
    CommonModule
  ],
  exports:[
    FileLargeComponent,
    FileListComponent
  ]
})
export class ItemsModule { }
