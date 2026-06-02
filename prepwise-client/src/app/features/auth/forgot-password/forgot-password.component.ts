import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css'
})
export class ForgotPasswordComponent {
  // Step 1: Enter email → Step 2: Enter OTP + new password
  step = 1;
  email = '';
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  emailForm: FormGroup;
  resetForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.emailForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });

    this.resetForm = this.fb.group({
      otpCode: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSendOtp(): void {
    if (this.emailForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';
    this.email = this.emailForm.value.email;

    this.authService.forgotPassword({ email: this.email }).subscribe({
      next: () => {
        this.isLoading = false;
        this.step = 2;
        this.successMessage = 'OTP sent to your email!';
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Failed to send OTP.';
      }
    });
  }

  onResetPassword(): void {
    if (this.resetForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.resetPassword({
      email: this.email,
      otpCode: this.resetForm.value.otpCode,
      newPassword: this.resetForm.value.newPassword
    }).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Password reset successful! Redirecting to login...';
        setTimeout(() => this.router.navigate(['/auth/login']), 2000);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Reset failed. Please try again.';
      }
    });
  }
}
