import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  preferredCurrency: string;
}

export interface UserResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  preferredCurrency: string;
  token: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5237/api/auth';
  private readonly tokenKey = 'flowmind_token';

  login(request: LoginRequest): Observable<ApiResponse<UserResponse>> {
    return this.http
      .post<ApiResponse<UserResponse>>(`${this.baseUrl}/login`, request)
      .pipe(tap(res => this.saveToken(res.data.token)));
  }

  register(request: RegisterRequest): Observable<ApiResponse<UserResponse>> {
    return this.http
      .post<ApiResponse<UserResponse>>(`${this.baseUrl}/register`, request)
      .pipe(tap(res => this.saveToken(res.data.token)));
  }

  loginWithGoogle(idToken: string): Observable<ApiResponse<UserResponse>> {
    return this.http
      .post<ApiResponse<UserResponse>>(`${this.baseUrl}/google-login`, { idToken })
      .pipe(tap(res => this.saveToken(res.data.token)));
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  private saveToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  }
}