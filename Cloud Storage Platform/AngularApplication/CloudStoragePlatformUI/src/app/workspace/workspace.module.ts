import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationDrawerComponent } from '../navigation-drawer/navigation-drawer.component';
import { RouterLink } from '@angular/router';
import { AppRoutingModule } from '../app-routing.module';
import { ViewerComponent } from '../viewer/viewer.component';
import { PanelComponent } from '../panel/panel.component';
import { ItemsModule } from '../items/items.module';



@NgModule({
  declarations: [
    NavigationDrawerComponent,
    ViewerComponent,
    PanelComponent
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
