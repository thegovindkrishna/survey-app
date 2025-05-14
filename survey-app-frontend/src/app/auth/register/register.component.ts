import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

// Angular Material modules
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatButtonModule
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  // properties for template binding
  email = '';
  password = '';
  role = 'User';
  errorMessage = '';

  private http = inject(HttpClient);
  private router = inject(Router);

  onRegister() {
    if (!this.email || !this.password) {
      this.errorMessage = 'Email and password are required';
      return;
    }

    this.http
      .post('https://localhost:7245/api/auth/register', {
        email: this.email,
        password: this.password,
        role: this.role
      })
      .subscribe({
        next: () => {
          this.router.navigate(['/login']);
        },
        error: () => {
          this.errorMessage = 'Registration failed. Try again.';
        }
      });
  }
}
