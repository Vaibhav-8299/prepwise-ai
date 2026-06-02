// ===== API Response Wrapper =====
export interface ApiResponse<T> {
  message: string;
  data: T;
}

// ===== Auth DTOs =====
export interface RegisterDto {
  fullName: string;
  email: string;
  password: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface VerifyOtpDto {
  email: string;
  otpCode: string;
}

export interface ForgotPasswordDto {
  email: string;
}

export interface ResetPasswordDto {
  email: string;
  otpCode: string;
  newPassword: string;
}

export interface RefreshTokenDto {
  refreshToken: string;
}

// ===== Response Models =====
export interface AuthResponse {
  token: string;
  refreshToken: string;
  fullName: string;
  email: string;
  role: string;
}

export interface UserProfile {
  id: number;
  fullName: string;
  email: string;
  role: string;
  createdAt: string;
}

// ===== Stored User (localStorage) =====
export interface StoredUser {
  name: string;
  email: string;
  role: string;
}
