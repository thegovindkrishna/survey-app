import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthLayoutComponent } from './shared/components/auth-layout/auth-layout';
import { LoginComponent } from './features/login/login';
import { RegisterComponent } from './features/register/register';
import { guestGuard } from '../../common/guards/auth.guard';

const routes: Routes = [
  {
    path: '',
    component: AuthLayoutComponent,
    children: [
      { 
        path: 'login', 
        component: LoginComponent,
        canActivate: [guestGuard] // Only apply guard to login
      },
      { 
        path: 'register', 
        component: RegisterComponent,
        canActivate: [guestGuard] // Only apply guard to register
      },
      { path: 'logout', redirectTo: '/', pathMatch: 'full' }, // Logout redirect, no guards
      { path: '', redirectTo: 'login', pathMatch: 'full' }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthRoutingModule {}
