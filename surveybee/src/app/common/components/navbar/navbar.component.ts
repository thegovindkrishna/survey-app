import { Component, computed } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  template: `
    <nav class="navbar">
      <div class="nav-brand">
        <a routerLink="/">SurveyBee</a>
      </div>
      
      <div class="nav-links">
        @if (authService.isAuthenticated()) {
          <div class="user-info">
            <span class="user-email">{{ authService.currentUser()?.email }}</span>
            <span class="user-role badge" [class]="'badge-' + authService.currentUser()?.role?.toLowerCase()">
              {{ authService.currentUser()?.role }}
            </span>
            
            @if (authService.isAdmin()) {
              <a routerLink="/admin" class="nav-link">Admin Dashboard</a>
            }
            
            @if (authService.isUser()) {
              <a routerLink="/user" class="nav-link">User Dashboard</a>
            }
            
            <button (click)="logout()" class="btn-logout">Logout</button>
          </div>
        } @else {
          <div class="auth-links">
            <a routerLink="/auth/login" class="nav-link">Login</a>
            <a routerLink="/auth/register" class="nav-link">Register</a>
          </div>
        }
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem 2rem;
      background: #2c3e50;
      color: white;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    
    .nav-brand a {
      font-size: 1.5rem;
      font-weight: bold;
      color: #3498db;
      text-decoration: none;
    }
    
    .user-info {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    
    .user-email {
      font-weight: 500;
    }
    
    .badge {
      padding: 0.25rem 0.5rem;
      border-radius: 0.25rem;
      font-size: 0.75rem;
      font-weight: bold;
      text-transform: uppercase;
    }
    
    .badge-admin {
      background: #e74c3c;
      color: white;
    }
    
    .badge-user {
      background: #27ae60;
      color: white;
    }
    
    .nav-link {
      color: #ecf0f1;
      text-decoration: none;
      padding: 0.5rem 1rem;
      border-radius: 0.25rem;
      transition: background-color 0.2s;
    }
    
    .nav-link:hover {
      background: #34495e;
    }
    
    .btn-logout {
      background: #e74c3c;
      color: white;
      border: none;
      padding: 0.5rem 1rem;
      border-radius: 0.25rem;
      cursor: pointer;
      transition: background-color 0.2s;
    }
    
    .btn-logout:hover {
      background: #c0392b;
    }
    
    .auth-links {
      display: flex;
      gap: 1rem;
    }
  `],
  imports: [RouterModule]
})
export class NavbarComponent {
  constructor(public authService: AuthService) {}

  logout(): void {
    this.authService.logout();
  }
}
