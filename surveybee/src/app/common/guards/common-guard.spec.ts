import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { commonGuard } from './common-guard';

describe('commonGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) => 
      TestBed.runInInjectionContext(() => commonGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
