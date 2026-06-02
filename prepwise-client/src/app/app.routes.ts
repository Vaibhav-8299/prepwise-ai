import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';
import { LayoutComponent } from './shared/layout/layout.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { VerifyEmailComponent } from './features/auth/verify-email/verify-email.component';
import { ForgotPasswordComponent } from './features/auth/forgot-password/forgot-password.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';

export const routes: Routes = [
  // Auth routes (no layout — full page)
  { path: 'auth/login', component: LoginComponent, canActivate: [guestGuard] },
  { path: 'auth/register', component: RegisterComponent, canActivate: [guestGuard] },
  { path: 'auth/verify-email', component: VerifyEmailComponent },
  { path: 'auth/forgot-password', component: ForgotPasswordComponent },

  // Protected routes (with layout — navbar + sidebar)
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },

  // Fallback
  { path: '**', redirectTo: 'auth/login' }
];
