import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {ViewerComponent} from "./workspace/viewer/viewer.component";
import {InfoComponent} from "./items/info/info.component";
import {LoginComponent} from "./account/login/login.component";
import {RegisterComponent} from "./account/register/register.component";
import {DashboardComponent} from "./account/dashboard/dashboard.component";
import {authGuard} from "./services/auth.guard";

const routes: Routes = [
  {path:'filter/home', component:ViewerComponent, canActivate: [authGuard]},
  {path:'filter/recents', component:ViewerComponent, canActivate: [authGuard]},
  {path:'filter/favorites', component:ViewerComponent, canActivate: [authGuard]},
  {path:'filter/trash', component:ViewerComponent, canActivate: [authGuard]},
  {path:'searchFilter', component:ViewerComponent, canActivate: [authGuard]},
  {path:'account/login', component:LoginComponent},
  {path:'account/register', component:RegisterComponent},
  {path:'account/dashboard', component:DashboardComponent, canActivate: [authGuard]},
  {path:'foldermetadata/:id', component:InfoComponent, canActivate: [authGuard]},
  {path:'filemetadata/:id', component:InfoComponent, canActivate: [authGuard]},
  {path:'', component:ViewerComponent, canActivate: [authGuard]},
  {path: '**', component: ViewerComponent, canActivate: [authGuard] },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {useHash: true})],
  exports: [RouterModule]
})
export class AppRoutingModule { }
