import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard, adminGuard, userGuard, guestGuard } from './common/guards/auth.guard';
import { UnauthorizedComponent } from './common/components/unauthorized/unauthorized.component';

const routes: Routes = [
  {
    path: '',
    loadChildren: () => import('./modules/landing/landing-module').then(m => m.LandingModule)
  },
  {
    path: 'auth',
    // canActivate: [guestGuard], // Disabled for debugging
    loadChildren: () => import('./modules/auth/auth-module').then(m => m.AuthModule)
  },
  {
    path: 'admin',
    // canActivate: [adminGuard], // Disabled for debugging
    loadChildren: () => import('./modules/admin/admin-module').then(m => m.AdminModule)
  },
  {
    path: 'user',
    // canActivate: [userGuard], // Disabled for debugging
    loadChildren: () => import('./modules/user/user-module').then(m => m.UserModule)
  },
  {
    path: 'unauthorized',
    component: UnauthorizedComponent
  },
  {
    path: '**',
    redirectTo: ''
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
