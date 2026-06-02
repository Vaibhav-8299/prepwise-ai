import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ApiResponse, AuthResponse, RegisterDto, LoginDto,
  VerifyOtpDto, ForgotPasswordDto, ResetPasswordDto,
  RefreshTokenDto, UserProfile, StoredUser
} from '../models/interfaces';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private currentUserSubject = new BehaviorSubject<StoredUser | null>(this.getStoredUser());
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  // ===== AUTH API CALLS =====

  register(dto: RegisterDto): Observable<ApiResponse<UserProfile>> {
    return this.http.post<ApiResponse<UserProfile>>(`${this.apiUrl}/register`, dto);
  }

  verifyEmail(dto: VerifyOtpDto): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.apiUrl}/verify-email`, dto);
  }

  login(dto: LoginDto): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/login`, dto).pipe(
      tap(response => {
        this.saveTokens(response.data);
        this.saveUser(response.data);
      })
    );
  }

  refreshToken(): Observable<ApiResponse<AuthResponse>> {
    const refreshToken = localStorage.getItem('refreshToken');
    const dto: RefreshTokenDto = { refreshToken: refreshToken || '' };
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/refresh-token`, dto).pipe(
      tap(response => {
        this.saveTokens(response.data);
        this.saveUser(response.data);
      })
    );
  }

  logout(): void {
    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) {
      this.http.post(`${this.apiUrl}/logout`, { refreshToken }).subscribe();
    }
    localStorage.clear();
    this.currentUserSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  forgotPassword(dto: ForgotPasswordDto): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.apiUrl}/forgot-password`, dto);
  }

  resetPassword(dto: ResetPasswordDto): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.apiUrl}/reset-password`, dto);
  }

  getProfile(): Observable<ApiResponse<UserProfile>> {
    return this.http.get<ApiResponse<UserProfile>>(`${this.apiUrl}/profile`);
  }

  // ===== TOKEN HELPERS =====

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    const user = this.getStoredUser();
    return user?.role === 'Admin';
  }

  getCurrentUser(): StoredUser | null {
    return this.currentUserSubject.value;
  }

  // ===== PRIVATE HELPERS =====

  private saveTokens(data: AuthResponse): void {
    localStorage.setItem('token', data.token);
    localStorage.setItem('refreshToken', data.refreshToken);
  }

  private saveUser(data: AuthResponse): void {
    const user: StoredUser = {
      name: data.fullName,
      email: data.email,
      role: data.role
    };
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  private getStoredUser(): StoredUser | null {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }
}
