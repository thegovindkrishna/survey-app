import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { SurveyService, Survey } from '../../surveys/survey.service';
import { AuthService } from '../../auth/auth.service';
// Angular Material and Common modules
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  surveys: Survey[] = [];
  isLoading = false;
  errorMessage = '';

  constructor(
    private surveyService: SurveyService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.fetchSurveys();
  }

  fetchSurveys() {
    this.isLoading = true;
    this.surveyService.getSurveys().subscribe({
      next: (data) => {
        this.surveys = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to load surveys.';
        this.isLoading = false;
      }
    });
  }

  createSurvey() {
    this.router.navigate(['/admin/surveys/create']);
  }

  editSurvey(id: number) {
    this.router.navigate(['/admin/surveys/edit', id]);
  }

  viewSurvey(id: number) {
    this.router.navigate(['/admin/surveys', id]);
  }

  deleteSurvey(id: number) {
    if (confirm('Are you sure you want to delete this survey?')) {
      this.surveyService.deleteSurvey(id).subscribe({
        next: () => {
          this.surveys = this.surveys.filter(s => s.id !== id);
        },
        error: () => {
          this.errorMessage = 'Failed to delete survey.';
        }
      });
    }
  }

  logout() {
    this.authService.logout();
  }
} 