import { Routes } from '@angular/router';
import { LoginComponent } from './auth/login/login.component';
import { RegisterComponent } from './auth/register/register.component';
import { SurveyCreateComponent } from './surveys/survey-create/survey-create.component';
import { UserDashboardComponent } from './shared/user-dashboard/user-dashboard.component'; // Adjust based on where you put it
import { AuthGuard } from './auth/auth.guard';
import { DashboardComponent } from './admin/dashboard/dashboard.component';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },

  // 👇 Admin Dashboard
  {
    path: 'admin/surveys',
    component: DashboardComponent,
    canActivate: [AuthGuard],
    data: { roles: ['Admin'] }
  },
  // 👇 Admin Create Survey
  {
    path: 'admin/surveys/create',
    component: SurveyCreateComponent,
    canActivate: [AuthGuard],
    data: { roles: ['Admin'] }
  },

  // 👇 Only Users can respond to surveys
  {
    path: 'user/dashboard',
    component: UserDashboardComponent,
    canActivate: [AuthGuard],
    data: { roles: ['User'] }
  }
];
