import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Guard to protect routes that require authentication
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  try {
    if (authService.isUserAuthenticated()) {
      return true;
    }

    // Redirect to login page with return url
    router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
    return false;
  } catch (error) {
    console.error('Auth guard error:', error);
    router.navigate(['/auth/login']);
    return false;
  }
};

/**
 * Guard to protect routes that require admin role
 */
export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  try {
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
  } catch (error) {
    console.error('Admin guard error:', error);
    router.navigate(['/auth/login']);
    return false;
  }
};

/**
 * Guard to protect routes that require user role
 */
export const userGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  try {
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
  } catch (error) {
    console.error('User guard error:', error);
    router.navigate(['/auth/login']);
    return false;
  }
};

/**
 * Guard to redirect authenticated users away from auth pages
 */
export const guestGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  try {
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
  } catch (error) {
    console.error('Guest guard error:', error);
    // If there's an error checking auth status, allow access to auth pages
    return true;
  }
};

/**
 * Guard that allows public access to specific routes (always returns true)
 */
export const publicGuard: CanActivateFn = (route, state) => {
  return true;
};

/**
 * Enhanced guard that checks for both authentication and specific admin permissions
 */
export const adminOnlyGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  try {
    // First check if user is authenticated
    if (!authService.isUserAuthenticated()) {
      router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
      return false;
    }

    // Check if user has admin role
    if (!authService.hasAdminRole()) {
      router.navigate(['/unauthorized']);
      return false;
    }

    return true;
  } catch (error) {
    console.error('Admin only guard error:', error);
    router.navigate(['/auth/login']);
    return false;
  }
};

/**
 * Enhanced guard that checks for both authentication and specific user permissions
 */
export const userOnlyGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  try {
    // First check if user is authenticated
    if (!authService.isUserAuthenticated()) {
      router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
      return false;
    }

    // Check if user has user role
    if (!authService.hasUserRole()) {
      router.navigate(['/unauthorized']);
      return false;
    }

    return true;
  } catch (error) {
    console.error('User only guard error:', error);
    router.navigate(['/auth/login']);
    return false;
  }
};
