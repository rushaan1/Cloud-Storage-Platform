import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {ViewerComponent} from "./workspace/viewer/viewer.component";

const routes: Routes = [
  {path:'', component:ViewerComponent},
  {path:'preDefinedFilter/home', component:ViewerComponent},
  {path:'preDefinedFilter/recents', component:ViewerComponent},
  {path:'preDefinedFilter/favorites', component:ViewerComponent},
  {path:'preDefinedFilter/trash', component:ViewerComponent},
  {path:'folder/**', component:ViewerComponent},
  {path:'searchFilter', component:ViewerComponent},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
