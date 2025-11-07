import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { AccountService, RegisterDTO, EmailVerificationRequest, EmailVerificationDTO } from '../../services/ApiServices/account.service';
import {Router} from "@angular/router";
import {TokenMonitorService} from "../../services/ApiServices/token-monitor.service";
import {SocialAuthService} from "@abacritt/angularx-social-login";
import { phoneNumberValidator } from '../../Utils';
import {formatDate} from "@angular/common";

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
  showPassword = false;
  showOtpVerification = false;
  otpCode: string = '';
  otpSentToEmail: string | null = null;
  registrationSuccess = false;
  otpError: string = '';
  userIdAfterRegistrationBeforeVerification: string = '';

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
        phoneNumber: formValue.phoneNumber
        // Note: rememberMe is no longer sent to register endpoint
      };

      this.accountService.register(registerDTO).subscribe({
        next: (res:any) => {
          console.log('Registration successful', res);
          this.registrationSuccess = true;
          this.otpSentToEmail = formValue.email;
          console.log("FrmValEmail: "+formValue.email);
          this.userIdAfterRegistrationBeforeVerification = res.userId;
          // Automatically send OTP after successful registration
          this.startOtpVerification();
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

  startOtpVerification(): void {
    const emailControl = this.registerForm.get('email');
    this.otpSentToEmail = (emailControl?.value || '').toString();
    console.log("emailformcontrolval:", emailControl?.value);
    if (!this.otpSentToEmail) {
      this.otpError = 'Please enter an email address first';
      return;
    }

    // Send OTP to the email
    const request: EmailVerificationRequest = {
      email: this.otpSentToEmail
    };

    this.accountService.sendVerificationOtp(request).subscribe({
      next: (res) => {
        this.showOtpVerification = true;
        this.otpError = '';
      },
      error: (err) => {
        console.error('Failed to send OTP', err);
        this.otpError = err.error?.detail || 'Failed to send OTP. Please try again.';
      }
    });
  }

  verifyOtp(): void {
    if (!this.otpCode.trim()) {
      this.otpError = 'Please enter the OTP';
      return;
    }

    const email = this.otpSentToEmail || this.registerForm.get('email')?.value;
    const rememberMe = this.registerForm.get('rememberMe')?.value;

    const request: EmailVerificationDTO = {
      email: email,
      otp: this.otpCode,
      rememberMe: rememberMe,
      userId: this.userIdAfterRegistrationBeforeVerification
    };

    this.accountService.verifyEmail(request).subscribe({
      next: (res) => {
        console.log('Email verified successfully', res);
        localStorage.setItem('rememberMe', rememberMe.toString());
        this.tokenMonitor.startMonitoring();
        this.router.navigate(['/']);

        this.accountService.getUser().subscribe({
          next: (userRes) => {
            if (userRes && userRes.personName) {
              localStorage.setItem('name', userRes.personName);
            }
          },
          error: () => {}
        });
      },
      error: (err) => {
        console.error('OTP verification failed', err);
        this.otpError = err.error?.message || 'Invalid or expired OTP for this registration attempt. Please try again.';
      }
    });
  }



  backToRegister(): void {
    this.showOtpVerification = false;
    this.otpError = '';
    this.otpCode = '';
  }
}
