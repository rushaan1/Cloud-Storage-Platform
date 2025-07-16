import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService, LoginDTO } from '../../services/ApiServices/account.service';
import {TokenMonitorService} from "../../services/ApiServices/token-monitor.service";
import {SocialAuthService} from "@abacritt/angularx-social-login";
import {Router} from "@angular/router";

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  logininvalid = false;
  showPassword = false;
  constructor(private fb: FormBuilder, private accountService: AccountService, private tokenMonitor:TokenMonitorService, private socialAuthService:SocialAuthService, private router:Router) {}

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
      rememberMe: [true]
    });
    this.socialAuthService.authState.subscribe(authState => {
      if (authState && authState.provider === 'GOOGLE' && authState.idToken) {
        this.accountService.googlelogin(authState.idToken).subscribe({
          next: (res) => {
            localStorage.setItem('rememberMe', 'true');
            this.tokenMonitor.startMonitoring();
            this.router.navigate(['/']);
          },
          error: (err) => {
            console.error('Google login error', err);
          }
        });
      }
    });
  }

  dismissInvalidLoginAlert(): void {
    this.logininvalid = false;
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      const formValue = this.loginForm.value;
      const loginDTO: LoginDTO = {
        email: formValue.email,
        password: formValue.password,
        rememberMe: formValue.rememberMe
      };
      this.accountService.login(loginDTO).subscribe({
        next: (res) => {
          console.log('Login successful', res);
          localStorage.setItem('rememberMe', this.loginForm.value.rememberMe.toString());
          this.tokenMonitor.startMonitoring();
          this.router.navigate(['/']);
        },
        error: (err) => {
          console.error('Login error', err);
          if (err.error.detail == "Invalid email or password"){
            this.logininvalid = true;
          }
        }
      });
    }
  }
}
