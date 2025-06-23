import { Routes } from '@angular/router';
import { LoginComponent } from './auth/login/login.component';
import { RegisterComponent } from './auth/register/register.component';
import { UserDashboardComponent } from './shared/user-dashboard/user-dashboard.component';
import { AuthGuard } from './auth/auth.guard';
import { DashboardComponent } from './admin/dashboard/dashboard.component';
import { SurveyBuilderComponent } from './admin/survey-builder/survey-builder.component';
import { SurveyResponseComponent } from './user/survey-response/survey-response.component';
import { ViewResponseComponent } from './user/view-response/view-response.component';
import { SurveyResultsComponent } from './admin/survey-results/survey-results.component';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },

  // Admin routes
  {
    path: 'admin',
    canActivate: [AuthGuard],
    data: { roles: ['Admin'] },
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'survey-builder', component: SurveyBuilderComponent },
      { path: 'survey-builder/:id', component: SurveyBuilderComponent },
      { path: 'results/:id', component: SurveyResultsComponent }
    ]
  },

  // User routes
  {
    path: 'user',
    canActivate: [AuthGuard],
    data: { roles: ['User'] },
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: UserDashboardComponent },
      { path: 'survey/:id', component: SurveyResponseComponent },
      { path: 'response/:id', component: ViewResponseComponent },
    ]
  }
];
