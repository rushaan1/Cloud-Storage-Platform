import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationDrawerComponent } from '../navigation-drawer/navigation-drawer.component';
import { RouterLink } from '@angular/router';
import { AppRoutingModule } from '../app-routing.module';
import { ViewerComponent } from '../viewer/viewer.component';
import { PanelComponent } from '../panels/panel/panel.component';
import { ItemsModule } from '../items/items.module';
import { InfoPanelComponent } from '../panels/info-panel/info-panel.component';
import { SelectedMenuOptionsComponent } from '../panels/selected-menu-options/selected-menu-options.component';
import { MoveSelectionComponent } from '../items/move-selection/move-selection.component';
import { PanelHandlerComponent } from '../panels/panel-handler/panel-handler.component';


@NgModule({
  declarations: [
    NavigationDrawerComponent,
    ViewerComponent,
    PanelComponent,
    InfoPanelComponent,
    SelectedMenuOptionsComponent,
    PanelHandlerComponent
  ],
  imports: [
    CommonModule,
    AppRoutingModule,
    ItemsModule
  ],
  exports:[
    NavigationDrawerComponent,
    ViewerComponent,
    PanelComponent,
    InfoPanelComponent
  ]
})
export class WorkspaceModule { }
