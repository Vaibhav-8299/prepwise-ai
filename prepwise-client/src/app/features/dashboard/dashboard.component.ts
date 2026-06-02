import { Component } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [AsyncPipe],
  template: `
    <div class="dashboard">
      @if (authService.currentUser$ | async; as user) {
        <div class="welcome-card">
          <h1>Welcome, {{ user.name }}! 👋</h1>
          <p>Your PrepWise AI dashboard is ready. More features coming soon.</p>
        </div>
      }
    </div>
  `,
  styles: [`
    .dashboard { padding: 1rem 0; }
    .welcome-card {
      background: rgba(20, 20, 40, 0.8);
      border: 1px solid rgba(108, 99, 255, 0.15);
      border-radius: 16px;
      padding: 2.5rem;
      text-align: center;
    }
    .welcome-card h1 {
      color: #fff;
      font-size: 1.8rem;
      margin: 0 0 0.5rem 0;
    }
    .welcome-card p {
      color: #8888a8;
      font-size: 1rem;
      margin: 0;
    }
  `]
})
export class DashboardComponent {
  constructor(public authService: AuthService) {}
}
