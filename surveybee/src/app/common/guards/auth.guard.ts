import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Guard to protect routes that require authentication
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isUserAuthenticated()) {
    return true;
  }

  // Redirect to login page with return url
  router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
  return false;
};

/**
 * Guard to protect routes that require admin role
 */
export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isUserAuthenticated() && authService.hasAdminRole()) {
    return true;
  }

  if (!authService.isUserAuthenticated()) {
    // Not authenticated, redirect to login
    router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
  } else {
    // Authenticated but not admin, redirect to unauthorized page
    router.navigate(['/unauthorized']);
  }
  
  return false;
};

/**
 * Guard to protect routes that require user role
 */
export const userGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isUserAuthenticated() && authService.hasUserRole()) {
    return true;
  }

  if (!authService.isUserAuthenticated()) {
    // Not authenticated, redirect to login
    router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
  } else {
    // Authenticated but not user, redirect to unauthorized page
    router.navigate(['/unauthorized']);
  }
  
  return false;
};

/**
 * Guard to redirect authenticated users away from auth pages
 */
export const guestGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isUserAuthenticated()) {
    return true;
  }

  // User is authenticated, redirect to appropriate dashboard
  if (authService.hasAdminRole()) {
    router.navigate(['/admin']);
  } else if (authService.hasUserRole()) {
    router.navigate(['/user']);
  } else {
    router.navigate(['/']);
  }
  
  return false;
};
