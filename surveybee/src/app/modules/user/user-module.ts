import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { UserRoutingModule } from './user-routing-module';
import { UserDashboardComponent } from './features/dashboard/dashboard';
import { UserLayout } from './shared/components/user-layout/user-layout';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { AttendSurveyComponent } from './features/attend-survey/attend-survey.component';

@NgModule({
  declarations: [
    UserDashboardComponent,
    UserLayout
  ],
  imports: [
    CommonModule,
    RouterOutlet,
    UserRoutingModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    AttendSurveyComponent
  ]
})
export class UserModule {}
