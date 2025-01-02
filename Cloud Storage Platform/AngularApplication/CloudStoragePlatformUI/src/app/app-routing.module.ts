import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {ViewerComponent} from "./workspace/viewer/viewer.component";

const routes: Routes = [
  {path:'', component:ViewerComponent},
  {path:'filter/home', component:ViewerComponent},
  {path:'filter/recents', component:ViewerComponent},
  {path:'filter/favorites', component:ViewerComponent},
  {path:'filter/trash', component:ViewerComponent},
  {path:'folder/**', component:ViewerComponent},
  {path:'searchFilter', component:ViewerComponent},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
