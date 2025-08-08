import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title?: string;
  message: string;
  duration?: number;
  persistent?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toasts = signal<Toast[]>([]);
  public toasts$ = this.toasts.asReadonly();

  private generateId(): string {
    return Math.random().toString(36).substring(2) + Date.now().toString(36);
  }

  private addToast(toast: Omit<Toast, 'id'>): void {
    const id = this.generateId();
    const newToast: Toast = {
      id,
      duration: 5000, // Default 5 seconds
      ...toast
    };

    this.toasts.update(toasts => [...toasts, newToast]);

    // Auto remove toast after duration (unless persistent)
    if (!newToast.persistent && newToast.duration && newToast.duration > 0) {
      setTimeout(() => {
        this.removeToast(id);
      }, newToast.duration);
    }
  }

  success(message: string, title?: string, options?: Partial<Toast>): void {
    this.addToast({
      type: 'success',
      title,
      message,
      ...options
    });
  }

  error(message: string, title?: string, options?: Partial<Toast>): void {
    this.addToast({
      type: 'error',
      title: title || 'Error',
      message,
      duration: 7000, // Errors stay longer
      ...options
    });
  }

  warning(message: string, title?: string, options?: Partial<Toast>): void {
    this.addToast({
      type: 'warning',
      title: title || 'Warning',
      message,
      ...options
    });
  }

  info(message: string, title?: string, options?: Partial<Toast>): void {
    this.addToast({
      type: 'info',
      title: title || 'Info',
      message,
      ...options
    });
  }

  removeToast(id: string): void {
    this.toasts.update(toasts => toasts.filter(toast => toast.id !== id));
  }

  clearAll(): void {
    this.toasts.set([]);
  }
}
