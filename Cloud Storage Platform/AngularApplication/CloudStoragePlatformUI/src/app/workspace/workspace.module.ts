import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationDrawerComponent } from '../navigation-drawer/navigation-drawer.component';
import { RouterLink } from '@angular/router';
import { AppRoutingModule } from '../app-routing.module';



@NgModule({
  declarations: [
    NavigationDrawerComponent
  ],
  imports: [
    CommonModule,
    AppRoutingModule
  ],
  exports:[
    NavigationDrawerComponent
  ]
})
export class WorkspaceModule { }
