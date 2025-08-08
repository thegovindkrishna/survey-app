import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  template: `
    <div class="unauthorized-container">
      <div class="unauthorized-content">
        <h1>🚫 Access Denied</h1>
        <p>You don't have permission to access this page.</p>
        
        @if (authService.isAuthenticated()) {
          <p>Current role: <strong>{{ authService.currentUser()?.role }}</strong></p>
          <div class="actions">
            @if (authService.isAdmin()) {
              <a routerLink="/admin" class="btn-primary">Go to Admin Dashboard</a>
            } @else {
              <a routerLink="/user" class="btn-primary">Go to User Dashboard</a>
            }
            <a routerLink="/" class="btn-secondary">Go to Home</a>
          </div>
        } @else {
          <div class="actions">
            <a routerLink="/auth/login" class="btn-primary">Login</a>
            <a routerLink="/" class="btn-secondary">Go to Home</a>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .unauthorized-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 60vh;
      padding: 2rem;
    }
    
    .unauthorized-content {
      text-align: center;
      max-width: 500px;
    }
    
    h1 {
      font-size: 2.5rem;
      margin-bottom: 1rem;
      color: #e74c3c;
    }
    
    p {
      font-size: 1.1rem;
      color: #666;
      margin-bottom: 1rem;
    }
    
    .actions {
      display: flex;
      gap: 1rem;
      justify-content: center;
      flex-wrap: wrap;
      margin-top: 2rem;
    }
    
    .btn-primary, .btn-secondary {
      padding: 0.75rem 1.5rem;
      border-radius: 0.375rem;
      text-decoration: none;
      font-weight: 500;
      transition: all 0.2s;
    }
    
    .btn-primary {
      background-color: #007bff;
      color: white;
    }
    
    .btn-primary:hover {
      background-color: #0056b3;
    }
    
    .btn-secondary {
      background-color: #6c757d;
      color: white;
    }
    
    .btn-secondary:hover {
      background-color: #545b62;
    }
  `],
  imports: [RouterModule]
})
export class UnauthorizedComponent {
  constructor(public authService: AuthService) {}
}
