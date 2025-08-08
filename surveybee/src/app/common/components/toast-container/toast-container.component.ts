import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, Toast } from '../../services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container">
      @for (toast of toastService.toasts$(); track toast.id) {
        <div 
          class="toast"
          [class]="'toast-' + toast.type"
          [attr.role]="toast.type === 'error' ? 'alert' : 'status'"
          [attr.aria-live]="toast.type === 'error' ? 'assertive' : 'polite'">
          
          <div class="toast-content">
            @if (toast.title) {
              <div class="toast-title">{{ toast.title }}</div>
            }
            <div class="toast-message">{{ toast.message }}</div>
          </div>
          
          <button 
            class="toast-close"
            (click)="toastService.removeToast(toast.id)"
            aria-label="Close notification">
            ×
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 1rem;
      right: 1rem;
      z-index: 1050;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      max-width: 400px;
    }

    .toast {
      display: flex;
      align-items: flex-start;
      padding: 1rem;
      border-radius: 0.5rem;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      border: 1px solid;
      animation: slideIn 0.3s ease-out;
      min-width: 300px;
    }

    @keyframes slideIn {
      from {
        transform: translateX(100%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    .toast-content {
      flex: 1;
      margin-right: 0.5rem;
    }

    .toast-title {
      font-weight: 600;
      margin-bottom: 0.25rem;
      font-size: 0.875rem;
    }

    .toast-message {
      font-size: 0.875rem;
      line-height: 1.4;
    }

    .toast-close {
      background: none;
      border: none;
      font-size: 1.25rem;
      line-height: 1;
      cursor: pointer;
      padding: 0;
      margin-left: 0.5rem;
      opacity: 0.7;
      transition: opacity 0.2s;
    }

    .toast-close:hover {
      opacity: 1;
    }

    /* Toast type styles */
    .toast-success {
      background-color: #d4edda;
      border-color: #c3e6cb;
      color: #155724;
    }

    .toast-error {
      background-color: #f8d7da;
      border-color: #f5c6cb;
      color: #721c24;
    }

    .toast-warning {
      background-color: #fff3cd;
      border-color: #ffeaa7;
      color: #856404;
    }

    .toast-info {
      background-color: #d1ecf1;
      border-color: #bee5eb;
      color: #0c5460;
    }

    .toast-success .toast-close,
    .toast-error .toast-close,
    .toast-warning .toast-close,
    .toast-info .toast-close {
      color: inherit;
    }

    /* Responsive adjustments */
    @media (max-width: 768px) {
      .toast-container {
        top: 1rem;
        right: 1rem;
        left: 1rem;
        max-width: none;
      }

      .toast {
        min-width: auto;
      }
    }
  `]
})
export class ToastContainerComponent {
  constructor(public toastService: ToastService) {}
}
