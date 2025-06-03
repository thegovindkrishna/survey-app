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
import { MatSpinner } from '@angular/material/progress-spinner';

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

        <mat-card class="dashboard-card">
          <mat-card-header>
            <mat-card-title>Recent Surveys</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div *ngIf="isLoading" class="loading-container">
              <mat-spinner diameter="40"></mat-spinner>
              <p>Loading surveys...</p>
            </div>

            <div *ngIf="errorMessage" class="error-message">
              {{ errorMessage }}
            </div>

            <div *ngIf="!isLoading && !errorMessage" class="survey-list">
              <div *ngIf="surveys.length === 0" class="no-surveys">
                <p>No surveys found. Create your first survey!</p>
              </div>
              <div *ngFor="let survey of surveys" class="survey-item">
                <h3>{{ survey.title }}</h3>
                <p>{{ survey.description }}</p>
                <div class="survey-actions">
                  <button mat-button color="primary" (click)="editSurvey(survey.id)">
                    <mat-icon>edit</mat-icon>
                    Edit
                  </button>
                  <button mat-button color="warn" (click)="deleteSurvey(survey.id)">
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

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 20px;
      gap: 16px;
    }

    .error-message {
      color: #f44336;
      padding: 16px;
      background-color: #ffebee;
      border-radius: 4px;
      margin: 16px 0;
    }

    .no-surveys {
      text-align: center;
      padding: 32px;
      color: #666;
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
    this.errorMessage = '';
    console.log('Fetching surveys...');
    this.surveyService.getSurveys().subscribe({
      next: (data) => {
        console.log('Received surveys:', data);
        this.surveys = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error fetching surveys:', err);
        this.errorMessage = 'Failed to load surveys: ' + (err.error?.message || err.message);
        this.isLoading = false;
        this.snackBar.open(this.errorMessage, 'Close', { duration: 5000 });
      }
    });
  }

  editSurvey(surveyId: number | undefined) {
    console.log('Editing survey with ID:', surveyId);
    if (!surveyId || surveyId <= 0) {
      console.error('Invalid survey ID:', surveyId);
      this.snackBar.open('Invalid survey ID', 'Close', { duration: 3000 });
      return;
    }
    this.router.navigate(['/admin/survey-builder', surveyId]).then(success => {
      if (!success) {
        console.error('Navigation failed');
        this.snackBar.open('Error navigating to survey editor', 'Close', { duration: 3000 });
      }
    });
  }

  deleteSurvey(id: number | undefined) {
    console.log('Deleting survey with ID:', id);
    if (!id || id <= 0) {
      console.error('Invalid survey ID:', id);
      this.snackBar.open('Invalid survey ID', 'Close', { duration: 3000 });
      return;
    }
    if (confirm('Are you sure you want to delete this survey?')) {
      this.surveyService.deleteSurvey(id).subscribe({
        next: () => {
          console.log('Survey deleted successfully');
          this.snackBar.open('Survey deleted successfully!', 'Close', { duration: 3000 });
          this.fetchSurveys(); // Refresh the surveys list
        },
        error: (error) => {
          console.error('Error deleting survey:', error);
          let errorMessage = 'Error deleting survey';
          if (error.status === 404) {
            errorMessage = 'Survey not found. It may have already been deleted.';
          } else if (error.error?.message) {
            errorMessage = error.error.message;
          } else if (error.message) {
            errorMessage = error.message;
          }
          this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
          this.fetchSurveys(); // Refresh the surveys list even on error
        }
      });
    }
  }

  logout() {
    this.authService.logout();
  }
} 