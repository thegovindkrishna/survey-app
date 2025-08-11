import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, throwError, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { AuthRequest, AuthResponse, RefreshTokenRequest, User, TokenPayload } from '../interfaces/auth.interface';
import { ToastService } from './toast.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_BASE = 'https://localhost:7245/api/auth'; // Updated to match backend launchSettings.json
  private readonly ACCESS_TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'current_user';

  // Reactive state using signals
  private currentUserSubject = new BehaviorSubject<User | null>(this.getUserFromStorage());
  public currentUser$ = this.currentUserSubject.asObservable();
  
  // Signals for reactive UI
  public currentUser = signal<User | null>(this.getUserFromStorage());
  public isAuthenticated = computed(() => this.currentUser() !== null);
  public isAdmin = computed(() => (this.currentUser()?.role || '').toLowerCase() === 'admin');
  public isUser = computed(() => (this.currentUser()?.role || '').toLowerCase() === 'user');

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    // Initialize current user state
    this.initializeAuth();
  }

  private toastService = inject(ToastService);

  private initializeAuth(): void {
    const user = this.getUserFromStorage();
    const token = this.getAccessToken();
    
    if (user && token && this.isTokenValid(token)) {
      this.currentUser.set(user);
      this.currentUserSubject.next(user);
    } else {
      this.clearAuthData();
    }
  }

  /**
   * Register a new user
   */
  register(authRequest: AuthRequest): Observable<any> {
    return this.http.post(`${this.API_BASE}/register`, authRequest)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Login user
   */
  login(email: string, password: string): Observable<AuthResponse> {
    const authRequest: AuthRequest = { email, password };
    
    console.log('AuthService: Making login request to:', `${this.API_BASE}/login`);
    console.log('AuthService: Request payload:', authRequest);
    
    return this.http.post<any>(`${this.API_BASE}/login`, authRequest)
      .pipe(
        map(raw => {
          console.log('AuthService: Raw login response received:', raw);
          // Support both wrapped (result) and unwrapped responses
          const resp = (raw && typeof raw === 'object' && 'result' in raw && raw.result) ? raw.result : raw;
          const normalizedRole = this.normalizeRole(resp.role, resp.isAdmin);
          const authData: AuthResponse = {
            accessToken: resp.accessToken,
            refreshToken: resp.refreshToken,
            email: resp.email ?? resp.username ?? '',
            username: resp.username ?? resp.email ?? '',
            role: normalizedRole
          };
          console.log('AuthService: Normalized auth data:', authData);
          return authData;
        }),
        tap(authData => {
          console.log('AuthService: Setting auth data:', authData);
          this.setAuthData(authData);
        }),
        catchError(error => {
          console.error('AuthService: Login error:', error);
          return this.handleError(error);
        })
      );
  }

  /**
   * Logout user
   */
  logout(): void {
    // Call backend logout endpoint
    this.http.post(`${this.API_BASE}/logout`, {}).subscribe({
      complete: () => {
        this.clearAuthData();
        this.toastService.info('You have been logged out successfully.', 'Logged Out');
        this.router.navigate(['/auth/login']);
      },
      error: () => {
        // Clear local data even if backend call fails
        this.clearAuthData();
        this.toastService.info('You have been logged out.', 'Logged Out');
        this.router.navigate(['/auth/login']);
      }
    });
  }

  /**
   * Refresh access token
   */
  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    const refreshRequest: RefreshTokenRequest = { refreshToken };
    
    return this.http.post<AuthResponse>(`${this.API_BASE}/refresh`, refreshRequest)
      .pipe(
        tap(response => {
          this.setAuthData(response);
        }),
        catchError(error => {
          this.clearAuthData();
          this.router.navigate(['/auth/login']);
          return throwError(() => error);
        })
      );
  }

  /**
   * Get current user information from backend
   */
  getCurrentUserInfo(): Observable<User> {
    return this.http.get<User>(`${this.API_BASE}/user`)
      .pipe(
        tap(user => {
          this.currentUser.set(user);
          this.currentUserSubject.next(user);
          localStorage.setItem(this.USER_KEY, JSON.stringify(user));
        }),
        catchError(this.handleError)
      );
  }

  /**
   * Check if user is authenticated
   */
  isUserAuthenticated(): boolean {
    const token = this.getAccessToken();
    return token !== null && this.isTokenValid(token);
  }

  /**
   * Check if user has admin role
   */
  hasAdminRole(): boolean {
    return (this.currentUser()?.role || '').toLowerCase() === 'admin';
  }

  /**
   * Check if user has user role
   */
  hasUserRole(): boolean {
    return (this.currentUser()?.role || '').toLowerCase() === 'user';
  }

  /**
   * Get access token
   */
  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_TOKEN_KEY);
  }

  /**
   * Get refresh token
   */
  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  /**
   * Set authentication data after successful login/refresh
   */
  private setAuthData(authResponse: AuthResponse): void {
    console.log('Setting auth data:', authResponse);
    
    // Guard against missing tokens
    if (!authResponse?.accessToken || !authResponse?.refreshToken) {
      console.error('AuthService: Missing tokens in auth response.');
      this.toastService.error('Authentication failed: invalid token response.', 'Login Error');
      this.clearAuthData();
      return;
    }

    const normalizedRole = this.normalizeRole(authResponse.role);
    
    localStorage.setItem(this.ACCESS_TOKEN_KEY, authResponse.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, authResponse.refreshToken);
    
    const user: User = {
      email: authResponse.email,
      role: normalizedRole
    };
    
    console.log('Setting user data:', user);
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    this.currentUser.set(user);
    this.currentUserSubject.next(user);
    
    console.log('Auth state after setting:', {
      isAuthenticated: this.isUserAuthenticated(),
      hasAdminRole: this.hasAdminRole(),
      currentUser: this.currentUser()
    });
  }

  /**
   * Clear all authentication data
   */
  private clearAuthData(): void {
    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUser.set(null);
    this.currentUserSubject.next(null);
  }

  /**
   * Get user from local storage
   */
  private getUserFromStorage(): User | null {
    const userStr = localStorage.getItem(this.USER_KEY);
    const user = userStr ? JSON.parse(userStr) as User : null;
    if (!user) return null;
    return { ...user, role: this.normalizeRole(user.role) };
  }

  // Normalize role helper
  private normalizeRole(role?: string, isAdminFlag?: boolean): 'Admin' | 'User' {
    if (typeof isAdminFlag === 'boolean') {
      return isAdminFlag ? 'Admin' : 'User';
    }
    const r = (role || '').toString().trim().toLowerCase();
    if (r === 'admin' || r === 'administrator' || r === 'admins') return 'Admin';
    return 'User';
  }

  /**
   * Check if token is valid (not expired)
   */
  private isTokenValid(token: string): boolean {
    try {
      const payload = this.decodeToken(token);
      const currentTime = Math.floor(Date.now() / 1000);
      return payload.exp > currentTime;
    } catch {
      return false;
    }
  }

  /**
   * Decode JWT token
   */
  private decodeToken(token: string): TokenPayload {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
    
    return JSON.parse(jsonPayload);
  }

  /**
   * Handle HTTP errors
   */
  private handleError = (error: HttpErrorResponse) => {
    let errorMessage = 'An error occurred';
    
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    } else {
      // Server-side error
      errorMessage = error.error?.message || `Error Code: ${error.status}\nMessage: ${error.message}`;
    }
    
    return throwError(() => new Error(errorMessage));
  };
}
