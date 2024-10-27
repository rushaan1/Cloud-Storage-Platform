import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileLargeComponent } from './file-large/file-large.component';
import { FileListComponent } from './file-list/file-list.component';
import { WorkspaceModule } from '../workspace/workspace.module';
import { MoveSelectionComponent } from './move-selection/move-selection.component';



@NgModule({
  declarations: [
    FileLargeComponent,
    FileListComponent,
    MoveSelectionComponent
  ],
  imports: [
    CommonModule
  ],
  exports:[
    FileLargeComponent,
    FileListComponent,
    MoveSelectionComponent
  ]
})
export class ItemsModule { }
