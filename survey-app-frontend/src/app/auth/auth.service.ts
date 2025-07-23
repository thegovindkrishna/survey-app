import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:7245/api/auth'; // Updated to match components

  constructor(private http: HttpClient, private router: Router) {}

  register(user: { email: string; password: string; role: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, user).pipe(
      catchError(this.handleError)
    );
  }

  login(credentials: { email: string; password: string }): Observable<{ token: string; role: string }> {
    return this.http.post<any>(`${this.apiUrl}/login`, credentials).pipe(
      map(response => {
        console.log('Login response:', response); // Debug log
        if (!response) {
          throw new Error('Empty response from server');
        }
        if (!response.token || !response.role) {
          throw new Error(`Invalid response format. Received: ${JSON.stringify(response)}`);
        }
        return {
          token: response.token,
          role: response.role
        };
      }),
      catchError(this.handleError)
    );
  }

  saveToken(token: string, role: string, email?: string) {
    localStorage.setItem('token', token); // Changed from 'jwt' to 'token'
    localStorage.setItem('role', role);
    if (email) {
      localStorage.setItem('userEmail', email);
    }
  }

  logout() {
    localStorage.removeItem('token'); // Changed from 'jwt' to 'token'
    localStorage.removeItem('role');
    localStorage.removeItem('userEmail');
    this.router.navigate(['/login']);
  }

  getToken() {
    return localStorage.getItem('token'); // Changed from 'jwt' to 'token'
  }

  getRole() {
    return localStorage.getItem('role');
  }

  getCurrentUser() {
    const email = localStorage.getItem('userEmail');
    const role = this.getRole();
    return email ? { email, role } : null;
  }

  isLoggedIn() {
    return !!this.getToken();
  }

  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'An error occurred';
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    } else {
      // Server-side error
      if (typeof error.error === 'string') {
        errorMessage = error.error;
      } else if (error.error?.message) {
        errorMessage = error.error.message;
      } else {
        errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
      }
    }
    console.error('Auth error:', error); // Debug log
    return throwError(() => errorMessage);
  }
}
