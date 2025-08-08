import { CommonModule } from "@angular/common";
import { LandingRoutingModule } from "./landing-routing-module";
import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router";
import { Home } from './pages/home/home';

@NgModule({
  declarations: [
    Home
  ],
  imports: [
    CommonModule,
    RouterModule,
    LandingRoutingModule
  ]
})
export class LandingModule { 
}

