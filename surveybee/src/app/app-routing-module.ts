import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard, adminGuard, userGuard, guestGuard, publicGuard } from './common/guards/auth.guard';
import { UnauthorizedComponent } from './common/components/unauthorized/unauthorized.component';
import { NotFoundComponent } from './common/components/not-found/not-found.component';

const routes: Routes = [
  {
    path: '',
    canActivate: [publicGuard], // Always accessible
    loadChildren: () => import('./modules/landing/landing-module').then(m => m.LandingModule)
  },
  {
    path: 'auth',
    // Remove guestGuard to allow logout access
    loadChildren: () => import('./modules/auth/auth-module').then(m => m.AuthModule)
  },
  {
    path: 'logout',
    // No guards - logout should always be accessible
    redirectTo: '/',
    pathMatch: 'full'
  },
  {
    path: 'admin',
    canActivate: [adminGuard], // Require admin role
    loadChildren: () => import('./modules/admin/admin-module').then(m => m.AdminModule)
  },
  {
    path: 'user',
    canActivate: [userGuard], // Require user role
    loadChildren: () => import('./modules/user/user-module').then(m => m.UserModule)
  },
  {
    path: 'unauthorized',
    component: UnauthorizedComponent
  },
  {
    path: '404',
    component: NotFoundComponent
  },
  {
    path: '**',
    redirectTo: '404'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
