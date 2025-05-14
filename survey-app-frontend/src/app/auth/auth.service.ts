import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:5001/api/Auth'; // Adjust base URL if needed

  constructor(private http: HttpClient, private router: Router) {}

  register(user: { email: string; password: string; role: string }) {
    return this.http.post(`${this.apiUrl}/register`, user);
  }

  login(credentials: { email: string; password: string }) {
    return this.http.post<{ token: string; role: string }>(`${this.apiUrl}/login`, credentials);
  }

  saveToken(token: string, role: string) {
    localStorage.setItem('jwt', token);
    localStorage.setItem('role', role);
  }

  logout() {
    localStorage.removeItem('jwt');
    localStorage.removeItem('role');
    this.router.navigate(['/login']);
  }

  getToken() {
    return localStorage.getItem('jwt');
  }

  getRole() {
    return localStorage.getItem('role');
  }

  isLoggedIn() {
    return !!this.getToken();
  }
}
