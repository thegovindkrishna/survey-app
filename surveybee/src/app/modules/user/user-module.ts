import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { UserRoutingModule } from './user-routing-module';
import { Dashboard } from './features/dashboard/dashboard';
import { UserLayout } from './shared/components/user-layout/user-layout';


@NgModule({
  declarations: [
    Dashboard,
    UserLayout
  ],
  imports: [
    CommonModule,
    UserRoutingModule
  ]
})
export class UserModule { }
