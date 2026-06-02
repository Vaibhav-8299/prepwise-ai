import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, AsyncPipe],
  template: `
    <nav class="navbar">
      <div class="navbar-brand">
        <a routerLink="/dashboard" class="logo">
          <span class="logo-icon">🚀</span>
          <span class="logo-text">PrepWise AI</span>
        </a>
      </div>
      <div class="navbar-links">
        <a routerLink="/dashboard" class="nav-link">Dashboard</a>
      </div>
      <div class="navbar-user">
        @if (authService.currentUser$ | async; as user) {
          <span class="user-name">{{ user.name }}</span>
          <span class="user-role">{{ user.role }}</span>
          <button class="btn-logout" (click)="authService.logout()">Logout</button>
        }
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0.75rem 2rem;
      background: rgba(15, 15, 35, 0.95);
      backdrop-filter: blur(20px);
      border-bottom: 1px solid rgba(108, 99, 255, 0.2);
      position: sticky;
      top: 0;
      z-index: 100;
    }
    .logo {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      text-decoration: none;
    }
    .logo-icon { font-size: 1.5rem; }
    .logo-text {
      font-size: 1.25rem;
      font-weight: 700;
      background: linear-gradient(135deg, #6C63FF, #7B68EE);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }
    .navbar-links { display: flex; gap: 1rem; }
    .nav-link {
      color: #a0a0b8;
      text-decoration: none;
      font-size: 0.9rem;
      padding: 0.4rem 0.8rem;
      border-radius: 6px;
      transition: all 0.2s;
    }
    .nav-link:hover {
      color: #fff;
      background: rgba(108, 99, 255, 0.15);
    }
    .navbar-user {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }
    .user-name {
      color: #e0e0e0;
      font-size: 0.9rem;
      font-weight: 500;
    }
    .user-role {
      color: #6C63FF;
      font-size: 0.75rem;
      background: rgba(108, 99, 255, 0.15);
      padding: 0.2rem 0.5rem;
      border-radius: 4px;
      font-weight: 600;
    }
    .btn-logout {
      background: none;
      border: 1px solid rgba(244, 67, 54, 0.4);
      color: #f44336;
      padding: 0.35rem 0.8rem;
      border-radius: 6px;
      cursor: pointer;
      font-size: 0.8rem;
      transition: all 0.2s;
    }
    .btn-logout:hover {
      background: rgba(244, 67, 54, 0.15);
      border-color: #f44336;
    }
  `]
})
export class NavbarComponent {
  constructor(public authService: AuthService) {}
}
