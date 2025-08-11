import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UserLayout } from './shared/components/user-layout/user-layout';
import { UserDashboardComponent } from './features/dashboard/dashboard';
import { AttendSurveyComponent } from './features/attend-survey/attend-survey.component';

const routes: Routes = [
  {
    path: '',
    component: UserLayout,
    children: [
      { path: '', component: UserDashboardComponent },
      { path: 'dashboard', component: UserDashboardComponent },
      { path: 'attend/:id', component: AttendSurveyComponent }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UserRoutingModule {}
