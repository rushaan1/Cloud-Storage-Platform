import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationDrawerComponent } from '../navigation-drawer/navigation-drawer.component';
import { RouterLink } from '@angular/router';
import { AppRoutingModule } from '../app-routing.module';
import { ViewerComponent } from '../viewer/viewer.component';



@NgModule({
  declarations: [
    NavigationDrawerComponent,
    ViewerComponent
  ],
  imports: [
    CommonModule,
    AppRoutingModule
  ],
  exports:[
    NavigationDrawerComponent,
    ViewerComponent
  ]
})
export class WorkspaceModule { }
