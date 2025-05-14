import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { appRoutes } from './app.routes';

export const appConfig = {
  providers: [
    provideHttpClient(withInterceptorsFromDi()),
    provideRouter(appRoutes)
  ]
};
