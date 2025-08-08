import { NgModule } from '@angular/core';
import { CommonModule, DatePipe, TitleCasePipe } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { AdminRoutingModule } from './admin-routing-module';
import { Dashboard } from './features/dashboard/dashboard';
import { AdminLayout } from './shared/components/admin-layout/admin-layout';

@NgModule({
  declarations: [
    Dashboard,
    AdminLayout
  ],
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    FormsModule,
    AdminRoutingModule
  ],
  providers: [
    DatePipe,
    TitleCasePipe
  ]
})
export class AdminModule { }
