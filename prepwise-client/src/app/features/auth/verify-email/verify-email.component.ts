import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './verify-email.component.html',
  styleUrl: './verify-email.component.css'
})
export class VerifyEmailComponent implements OnInit {
  verifyForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  email = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.verifyForm = this.fb.group({
      otpCode: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]]
    });
  }

  ngOnInit(): void {
    this.email = this.route.snapshot.queryParams['email'] || '';
    if (!this.email) {
      this.router.navigate(['/auth/register']);
    }
  }

  onSubmit(): void {
    if (this.verifyForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.verifyEmail({
      email: this.email,
      otpCode: this.verifyForm.value.otpCode
    }).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Email verified successfully! Redirecting to login...';
        setTimeout(() => this.router.navigate(['/auth/login']), 2000);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Verification failed. Please try again.';
      }
    });
  }
}
