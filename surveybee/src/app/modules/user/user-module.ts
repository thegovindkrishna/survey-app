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
import { MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { AttendSurveyComponent } from './features/attend-survey/attend-survey.component';
import { ResponseViewerComponent } from './features/response-viewer/response-viewer.component';

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
    MatDialogModule,
    MatButtonModule,
    AttendSurveyComponent,
    ResponseViewerComponent
  ]
})
export class UserModule {}
