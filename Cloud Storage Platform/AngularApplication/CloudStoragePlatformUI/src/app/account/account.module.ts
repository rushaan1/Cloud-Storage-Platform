import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import {GoogleLoginProvider, GoogleSigninButtonModule, SocialAuthServiceConfig} from "@abacritt/angularx-social-login";
import { DashboardComponent } from './dashboard/dashboard.component';
import {WorkspaceModule} from "../workspace/workspace.module";
import {NgxChartsModule} from "@swimlane/ngx-charts";
import { FormsModule } from '@angular/forms';
import {RouterLink} from "@angular/router";

@NgModule({
  declarations: [
    LoginComponent,
    RegisterComponent,
    DashboardComponent
  ],
    imports: [
        CommonModule,
        ReactiveFormsModule,
        GoogleSigninButtonModule,
        WorkspaceModule,
        NgxChartsModule,
        FormsModule,
        RouterLink
    ],
  providers: [{
    provide: 'SocialAuthServiceConfig',
    useValue: {
      autoLogin: false,
      oneTapEnabled: false,
      lang: 'en',
      providers: [
        {
          id: GoogleLoginProvider.PROVIDER_ID,
          provider: new GoogleLoginProvider(
            '306744964755-pnv48ppn8d30l9r8605chh4t21udj7p2.apps.googleusercontent.com'
          )
        }
      ],
      onError: (err) => {
        console.error(err);
      }
    } as SocialAuthServiceConfig,
  }]
})
export class AccountModule { }
