import { NgModule } from '@angular/core';
import { CommonModule, DatePipe, TitleCasePipe } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCommonModule } from '@angular/material/core';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';

import { AdminRoutingModule } from './admin-routing-module';
import { Dashboard } from './features/dashboard/dashboard';
import { AdminLayout } from './shared/components/admin-layout/admin-layout';
import { ManageUsersComponent } from './features/users/manage-users.component';

@NgModule({
  declarations: [
    Dashboard,
    AdminLayout,
    ManageUsersComponent
  ],
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    FormsModule,
    AdminRoutingModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatCommonModule,
    MatChipsModule,
    MatDividerModule
  ],
  providers: [
    DatePipe,
    TitleCasePipe
  ]
})
export class AdminModule { }
