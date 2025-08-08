import { CanActivateFn } from '@angular/router';

export const commonGuard: CanActivateFn = (route, state) => {
  return true;
};
