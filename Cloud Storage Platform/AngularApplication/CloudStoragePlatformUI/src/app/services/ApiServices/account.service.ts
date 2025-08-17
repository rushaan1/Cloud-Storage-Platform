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

export interface ResponseWithNameAndEmail {
  PersonName: string;
  Email: string;
  homeFolderSize?: number;
}

export interface AccountDetailsAndAnalytics {
  topExtensionsBySize: { extension: string; totalSize: number }[];
  topFilesBySize: { fileName: string; size: number }[];
  totalFolders: number;
  totalFiles: number;
  favoriteItems: number;
  itemsShared: number;
  email: string;
  createdAt: string;
  country: string;
  phoneNumber: string;
  personName: string;
}

export interface UpdateAccountDTO {
  email?: string | null;
  fullName?: string | null;
  country?: string | null;
  phoneNumber?: string | null;
  password?: string | null;
  confirmPassword?: string | null;
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

  refreshToken(): Observable<any> {
    const rememberMe = localStorage.getItem('rememberMe') == 'true';
    return this.http.post(`${this.API_URL}/regenerate-jwt-token?rememberMe=${rememberMe}`, {});
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.API_URL}/logout`,{});
  }

  googlelogin(idToken: string): Observable<any> {
    return this.http.post(
      `${this.API_URL}/google-login`,
      JSON.stringify(idToken),
      {
        withCredentials: true,
        headers: { 'Content-Type': 'application/json' }
      }
    );
  }

  getAccountDetailsAndAnalytics(): Observable<AccountDetailsAndAnalytics> {
    return this.http.get<AccountDetailsAndAnalytics>(`${this.API_URL}/account-details-analytics`);
  }

  updateAccount(dto: UpdateAccountDTO): Observable<any> {
    return this.http.patch(`${this.API_URL}/update-account`, dto);
  }

  getUser(): Observable<{ personName: string }> {
    return this.http.get<{ personName: string }>(`${this.API_URL}/get-user`);
  }
}
