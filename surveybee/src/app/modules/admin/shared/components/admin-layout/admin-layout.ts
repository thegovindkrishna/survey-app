import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-admin-layout',
  standalone: false,
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css'
})
export class AdminLayout {
  constructor(private router: Router) {}

  navigateTo(path: string): void {
    this.router.navigate([path]);
  }

  logout(): void {
    // Placeholder for logout functionality
    console.log('Logout functionality (not implemented yet)');
    this.router.navigate(['/auth/login']);
  }
}
