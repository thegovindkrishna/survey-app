import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminLayout } from './shared/components/admin-layout/admin-layout';
import { Dashboard } from './features/dashboard/dashboard';
import { CreateSurveyComponent } from './features/surveys/survey-create/create-survey';
import { ManageUsersComponent } from './features/users/manage-users.component';
import { adminOnlyGuard } from '../../common/guards/auth.guard';

const routes: Routes = [
  {
    path: '',
    component: AdminLayout,
    canActivate: [adminOnlyGuard], // Additional security check
    children: [
      { path: '', component: Dashboard },
      { path: 'dashboard', component: Dashboard },
      { path: 'surveys', children: [
        { path: '', loadComponent: () => import('./features/surveys/view-surveys/view-surveys').then(m => m.ViewSurveysComponent) },
        { path: 'create', loadComponent: () => import('./features/surveys/survey-create/create-survey').then(m => m.CreateSurveyComponent) },
        { path: 'results', loadComponent: () => import('./features/surveys/survey-results/survey-results').then(m => m.SurveyResultsComponent) },
        { path: ':id/results', loadComponent: () => import('./features/surveys/survey-results-detail/survey-results-detail').then(m => m.SurveyResultsDetailComponent) },
        { path: ':id', loadComponent: () => import('./features/surveys/survey-edit/edit-survey').then(m => m.EditSurveyComponent) }
      ] },
      { path: 'users', component: ManageUsersComponent }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
