import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard {
  constructor(private router: Router) {}

  navigateToSurveys(): void {
    // Placeholder for survey navigation
    console.log('Navigate to surveys (not implemented yet)');
  }
}
