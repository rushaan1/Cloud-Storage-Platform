import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService, LoginDTO } from '../../services/ApiServices/account.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;

  constructor(private fb: FormBuilder, private accountService: AccountService) {}

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
      rememberMe: [true]
    });
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      const formValue = this.loginForm.value;
      const loginDTO: LoginDTO = {
        email: formValue.email,
        password: formValue.password
      };
      this.accountService.login(loginDTO).subscribe({
        next: (res) => {
          console.log('Login successful', res);
        },
        error: (err) => {
          console.error('Login error', err);
        }
      });
    }
  }
}
