import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable, throwError, BehaviorSubject, filter, take, switchMap, catchError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';

let isRefreshing = false;
const refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<any>, next: HttpHandlerFn): Observable<HttpEvent<any>> => {
  const authService = inject(AuthService);
  const toastService = inject(ToastService);

  // Skip interceptor for auth endpoints (login, register, refresh)
  if (req.url.includes('/api/auth/login') || 
      req.url.includes('/api/auth/register') || 
      req.url.includes('/api/auth/refresh')) {
    return next(req);
  }

  // Add authorization header if token exists
  const token = authService.getAccessToken();
  if (token) {
    req = addTokenToRequest(req, token);
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 Unauthorized errors
      if (error.status === 401 && token) {
        return handle401Error(req, next, authService, toastService);
      }
      
      return throwError(() => error);
    })
  );
};

function addTokenToRequest(request: HttpRequest<any>, token: string): HttpRequest<any> {
  return request.clone({
    headers: request.headers.set('Authorization', `Bearer ${token}`)
  });
}

function handle401Error(request: HttpRequest<any>, next: HttpHandlerFn, authService: AuthService, toastService: ToastService): Observable<HttpEvent<any>> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    const refreshToken = authService.getRefreshToken();
    
    if (refreshToken) {
      return authService.refreshToken().pipe(
        switchMap((authResponse: any) => {
          isRefreshing = false;
          refreshTokenSubject.next(authResponse.accessToken);
          
          // Retry the original request with new token
          return next(addTokenToRequest(request, authResponse.accessToken));
        }),
        catchError((error) => {
          isRefreshing = false;
          refreshTokenSubject.next(null);
          
          // Show error toast for session expiry
          toastService.warning('Your session has expired. Please login again.', 'Session Expired');
          
          // Refresh failed, logout user
          authService.logout();
          return throwError(() => error);
        })
      );
    } else {
      // No refresh token available, logout user
      isRefreshing = false;
      toastService.error('Authentication failed. Please login again.', 'Authentication Error');
      authService.logout();
      return throwError(() => new Error('No refresh token available'));
    }
  } else {
    // If refreshing is in progress, wait for the new token
    return refreshTokenSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap(token => next(addTokenToRequest(request, token)))
    );
  }
}
