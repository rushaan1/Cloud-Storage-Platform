import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { AccountService, RegisterDTO } from '../../services/ApiServices/account.service';
import {Router} from "@angular/router";
import {TokenMonitorService} from "../../services/ApiServices/token-monitor.service";

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  countries: string[] = [
    'United States', 'Canada', 'United Kingdom', 'Australia', 'Germany',
    'France', 'Spain', 'Italy', 'Japan', 'China', 'India', 'Brazil',
    'Mexico', 'Russia', 'South Africa'
  ];

  constructor(private fb: FormBuilder, private accountService: AccountService, private router:Router, private tokenMonitor:TokenMonitorService) {}

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      username: ['', [Validators.required, Validators.minLength(3)]],
      country: ['', Validators.required],
      phoneNumber: [''],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required],
      rememberMe: [true]
    }, { validators: this.passwordMatchValidator });
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
        },
        error: (err) => {
          console.error('Registration error', err);
        }
      });
    }
  }
}
