import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {ViewerComponent} from "./workspace/viewer/viewer.component";
import {InfoComponent} from "./items/info/info.component";

const routes: Routes = [
  {path:'filter/home', component:ViewerComponent},
  {path:'filter/recents', component:ViewerComponent},
  {path:'filter/favorites', component:ViewerComponent},
  {path:'filter/trash', component:ViewerComponent},
  {path:'searchFilter', component:ViewerComponent},
  {path:'foldermetadata/:id', component:InfoComponent},
  {path:'filemetadata/:id', component:InfoComponent},
  {path:'', component:ViewerComponent},
  {path: '**', component: ViewerComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {useHash: true})],
  exports: [RouterModule]
})
export class AppRoutingModule { }
