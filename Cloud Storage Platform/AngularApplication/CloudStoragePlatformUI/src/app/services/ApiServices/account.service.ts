import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface RegisterDTO {
  email: string;
  password: string;
  confirmPassword: string;
  personName: string;
  country: string;
  phoneNumber?: string;
  rememberMe: boolean;
}

export interface LoginDTO {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface RegenerateTokenModel {
  token: string;
  refreshToken: string;
}

export interface ResponseWithNameAndEmail {
  personName: string;
  email: string;
}

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private API_URL = 'https://localhost:7219/api/Account';

  constructor(private http: HttpClient) {}

  register(registerDTO: RegisterDTO): Observable<ResponseWithNameAndEmail> {
    return this.http.post<ResponseWithNameAndEmail>(`${this.API_URL}/register`, registerDTO);
  }

  login(loginDTO: LoginDTO): Observable<ResponseWithNameAndEmail> {
    return this.http.post<ResponseWithNameAndEmail>(`${this.API_URL}/login`, loginDTO);
  }

  refreshToken():Observable<any> {
    return this.http.post(`${this.API_URL}/regenerate-jwt-token`,{});
  }

  logout(): Observable<void> {
    return this.http.get<void>(`${this.API_URL}/logout`);
  }
}
