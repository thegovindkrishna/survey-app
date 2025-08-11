import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterModule],
  template: `
    <div class="not-found-container">
      <div class="not-found-content">
        <h1>🔍 404 - Page Not Found</h1>
        <p>The page you're looking for doesn't exist or has been moved.</p>
        
        <div class="actions">
          @if (authService.isAuthenticated()) {
            @if (authService.isAdmin()) {
              <a routerLink="/admin" class="btn-primary">Go to Admin Dashboard</a>
            } @else if (authService.isUser()) {
              <a routerLink="/user" class="btn-primary">Go to User Dashboard</a>
            }
          } @else {
            <a routerLink="/auth/login" class="btn-primary">Login</a>
          }
          <a routerLink="/" class="btn-secondary">Go to Home</a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .not-found-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 60vh;
      padding: 2rem;
      background: #f8f9fa;
    }
    
    .not-found-content {
      text-align: center;
      max-width: 500px;
      background: white;
      padding: 3rem 2rem;
      border-radius: 12px;
      box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
    }
    
    h1 {
      font-size: 2.5rem;
      margin-bottom: 1rem;
      color: #6c757d;
      font-weight: 700;
    }
    
    p {
      font-size: 1.1rem;
      color: #6c757d;
      margin-bottom: 2rem;
      line-height: 1.6;
    }
    
    .actions {
      display: flex;
      gap: 1rem;
      justify-content: center;
      flex-wrap: wrap;
    }
    
    .btn-primary,
    .btn-secondary {
      padding: 12px 24px;
      border-radius: 8px;
      text-decoration: none;
      font-weight: 500;
      transition: all 0.2s ease;
      min-width: 140px;
      text-align: center;
    }
    
    .btn-primary {
      background: #fece22;
      color: #333;
      border: 2px solid #fece22;
    }
    
    .btn-primary:hover {
      background: #e8b71f;
      border-color: #e8b71f;
      transform: translateY(-1px);
    }
    
    .btn-secondary {
      background: transparent;
      color: #6c757d;
      border: 2px solid #dee2e6;
    }
    
    .btn-secondary:hover {
      background: #f8f9fa;
      border-color: #adb5bd;
      transform: translateY(-1px);
    }
    
    @media (max-width: 768px) {
      .not-found-container {
        padding: 1rem;
      }
      
      .not-found-content {
        padding: 2rem 1.5rem;
      }
      
      h1 {
        font-size: 2rem;
      }
      
      .actions {
        flex-direction: column;
        align-items: center;
      }
      
      .btn-primary,
      .btn-secondary {
        width: 100%;
        max-width: 200px;
      }
    }
  `]
})
export class NotFoundComponent {
  constructor(public authService: AuthService) {}
}
