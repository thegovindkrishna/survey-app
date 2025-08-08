import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminLayout } from './shared/components/admin-layout/admin-layout';
import { Dashboard } from './features/dashboard/dashboard';

const routes: Routes = [
  {
    path: '',
    component: AdminLayout,
    children: [
      { path: '', component: Dashboard },
      { path: 'dashboard', component: Dashboard }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
