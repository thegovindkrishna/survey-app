import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    // Guard for browser environment
    const isBrowser = typeof window !== 'undefined' && !!window.localStorage;
    if (!isBrowser) {
      this.router.navigate(['/login']);
      return false;
    }

    const token = localStorage.getItem('token');
    const role = localStorage.getItem('role');

    if (!token || !role) {
      this.router.navigate(['/login']);
      return false;
    }

    const allowedRoles = route.data['roles'] as string[];
    if (allowedRoles && !allowedRoles.includes(role)) {
      this.router.navigate(['/login']);
      return false;
    }

    return true;
  }
}
