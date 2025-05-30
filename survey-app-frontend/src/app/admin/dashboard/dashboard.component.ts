import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { SurveyService, Survey } from '../../shared/services/survey.service';
import { AuthService } from '../../auth/auth.service';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSliderModule } from '@angular/material/slider';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatRadioModule } from '@angular/material/radio';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    FormsModule,
    ReactiveFormsModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatSnackBarModule,
    MatSliderModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatRadioModule,
    MatDividerModule
  ],
  template: `
    <div class="dashboard-container">
      <h1>Admin Dashboard</h1>
      
      <div class="dashboard-grid">
        <mat-card class="dashboard-card">
          <mat-card-header>
            <mat-card-title>Survey Management</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p>Create and manage surveys for your organization</p>
          </mat-card-content>
          <mat-card-actions>
            <button mat-raised-button color="primary" routerLink="/admin/survey-builder">
              <mat-icon>add</mat-icon>
              Create New Survey
            </button>
          </mat-card-actions>
        </mat-card>

        <mat-card class="dashboard-card" *ngIf="surveys.length > 0">
          <mat-card-header>
            <mat-card-title>Recent Surveys</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="survey-list">
              <div *ngFor="let survey of surveys" class="survey-item">
                <h3>{{ survey.title }}</h3>
                <p>{{ survey.description }}</p>
                <div class="survey-actions">
                  <button mat-button color="primary" (click)="editSurvey(survey)">
                    <mat-icon>edit</mat-icon>
                    Edit
                  </button>
                  <button mat-button color="warn" (click)="deleteSurvey(survey.id!)">
                    <mat-icon>delete</mat-icon>
                    Delete
                  </button>
                </div>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      padding: 20px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .dashboard-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 20px;
      margin-top: 20px;
    }

    .dashboard-card {
      height: 100%;
    }

    .survey-list {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .survey-item {
      padding: 16px;
      border: 1px solid #e0e0e0;
      border-radius: 4px;
    }

    .survey-actions {
      display: flex;
      gap: 8px;
      margin-top: 8px;
    }

    mat-card-actions {
      padding: 16px;
    }

    button {
      width: 100%;
    }
  `]
})
export class DashboardComponent implements OnInit {
  surveys: Survey[] = [];
  isLoading = false;
  errorMessage = '';

  constructor(
    private surveyService: SurveyService,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
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

  editSurvey(survey: Survey) {
    console.log('Editing survey:', survey);
    this.router.navigate(['/admin/survey-builder', survey.id]);
  }

  deleteSurvey(id: number) {
    console.log('Deleting survey with ID:', id);
    if (confirm('Are you sure you want to delete this survey?')) {
      this.surveyService.deleteSurvey(id).subscribe({
        next: () => {
          this.snackBar.open('Survey deleted successfully!', 'Close', { duration: 3000 });
          this.fetchSurveys(); // Refresh the surveys list automatically
        },
        error: (error) => {
          if (error.status === 404) {
            this.snackBar.open('Survey not found. It may have already been deleted.', 'Close', { duration: 5000 });
          } else {
            this.snackBar.open('Error deleting survey: ' + (error.error?.message || error.message), 'Close', { duration: 5000 });
          }
          this.fetchSurveys(); // Refresh the surveys list even on error
        }
      });
    }
  }

  logout() {
    this.authService.logout();
  }
} 