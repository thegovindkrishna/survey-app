import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminLayout } from './shared/components/admin-layout/admin-layout';
import { Dashboard } from './features/dashboard/dashboard';
import { CreateSurveyComponent } from './features/surveys/survey-create/create-survey';
import { ManageUsersComponent } from './features/users/manage-users.component';

const routes: Routes = [
  {
    path: '',
    component: AdminLayout,
    children: [
      { path: '', component: Dashboard },
      { path: 'dashboard', component: Dashboard },
      { path: 'surveys', children: [
        { path: '', loadComponent: () => import('./features/surveys/view-surveys/view-surveys').then(m => m.ViewSurveysComponent) },
        { path: 'create', loadComponent: () => import('./features/surveys/survey-create/create-survey').then(m => m.CreateSurveyComponent) },
        { path: 'results', loadComponent: () => import('./features/surveys/survey-results/survey-results').then(m => m.SurveyResultsComponent) },
        { path: ':id/results', loadComponent: () => import('./features/surveys/survey-results-detail/survey-results-detail').then(m => m.SurveyResultsDetailComponent) },
        { path: ':id', loadComponent: () => import('./features/surveys/survey-edit/edit-survey').then(m => m.EditSurveyComponent) } // Added route for editing a survey
      ] },
      { path: 'users', component: ManageUsersComponent } // Changed from loadComponent to component
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
