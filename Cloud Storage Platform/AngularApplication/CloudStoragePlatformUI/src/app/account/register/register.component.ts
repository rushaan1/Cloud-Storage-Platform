import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { AccountService, RegisterDTO } from '../../services/ApiServices/account.service';
import {Router} from "@angular/router";
import {TokenMonitorService} from "../../services/ApiServices/token-monitor.service";
import {SocialAuthService} from "@abacritt/angularx-social-login";
import { phoneNumberValidator } from '../../Utils';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  emailAlreadyRegistered = false;
  countries: string[] = [
    'United States', 'Canada', 'United Kingdom', 'Australia', 'Germany',
    'France', 'Spain', 'Italy', 'Japan', 'China', 'India', 'Brazil',
    'Mexico', 'Russia', 'South Africa'
  ];

  constructor(private fb: FormBuilder, private accountService: AccountService, private router:Router, private tokenMonitor:TokenMonitorService, private socialAuthService:SocialAuthService) {}

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      username: ['', [Validators.required, Validators.minLength(3)]],
      country: ['', Validators.required],
      phoneNumber: ['', [phoneNumberValidator]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required],
      rememberMe: [true]
    }, { validators: this.passwordMatchValidator });

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

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    return null;
  }

  onEmailInputClick(): void {
    this.emailAlreadyRegistered = false;
  }

  onSubmit(): void {
    if (this.registerForm.valid) {
      const formValue = this.registerForm.value;
      const registerDTO: RegisterDTO = {
        email: formValue.email,
        password: formValue.password,
        confirmPassword: formValue.confirmPassword,
        personName: formValue.username,
        country: formValue.country,
        phoneNumber: formValue.phoneNumber,
        rememberMe: formValue.rememberMe
      };
      this.accountService.register(registerDTO).subscribe({
        next: (res) => {
          // this.router.navigate(['filter','home']);
          console.log(res);
          localStorage.setItem('rememberMe', this.registerForm.value.rememberMe.toString());
          this.tokenMonitor.startMonitoring();
          this.router.navigate(['/']);
        },
        error: (err) => {
          console.error('Registration error', err);
          // Check if the error is specifically about email already being registered
          if (err.error.detail && typeof err.error.detail === 'string' &&
              (err.error.detail.includes('already taken') || err.error.detail.includes('already exists') || err.error.detail.includes('already registered'))) {
            this.emailAlreadyRegistered = true;
          }
        }
      });
    }
  }
}
