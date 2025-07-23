import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatMenuModule } from '@angular/material/menu';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { MatTabsModule } from '@angular/material/tabs';

import { UserService, UserResponse } from '../services/user.service';
import { AuthService } from '../../auth/auth.service';
import { Survey } from '../models/survey.model';

@Component({
  selector: 'app-user-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatToolbarModule,
    MatMenuModule,
    MatChipsModule,
    MatTooltipModule,
    MatDividerModule,
    MatTabsModule
  ],
  template: `
    <!-- Header with user info and logout -->
    <mat-toolbar color="primary" class="dashboard-header">
      <div class="header-content">
        <div class="header-left">
          <h1>User Dashboard</h1>
          <span class="breadcrumb">Welcome back, {{ currentUser?.email || 'User' }}!</span>
        </div>
        <div class="header-right">
          <span class="user-info">
            <mat-icon>account_circle</mat-icon>
            {{ currentUser?.email || 'User' }}
          </span>
          <button mat-icon-button [matMenuTriggerFor]="userMenu" matTooltip="User menu">
            <mat-icon>more_vert</mat-icon>
          </button>
          <mat-menu #userMenu="matMenu">
            <button mat-menu-item (click)="logout()">
              <mat-icon>logout</mat-icon>
              <span>Logout</span>
            </button>
          </mat-menu>
        </div>
      </div>
    </mat-toolbar>

    <div class="dashboard-container">
      <!-- Statistics Cards -->
      <div class="stats-grid">
        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <mat-icon class="stat-icon">poll</mat-icon>
              <div class="stat-info">
                <h3>{{ availableSurveys.length }}</h3>
                <p>Available Surveys</p>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <mat-icon class="stat-icon">check_circle</mat-icon>
              <div class="stat-info">
                <h3>{{ userResponses.length }}</h3>
                <p>Completed Surveys</p>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <mat-icon class="stat-icon">pending</mat-icon>
              <div class="stat-info">
                <h3>{{ pendingSurveys.length }}</h3>
                <p>Pending Surveys</p>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
      </div>

      <!-- Main Content Tabs -->
      <mat-card class="main-content">
        <mat-tab-group>
          <!-- Available Surveys Tab -->
          <mat-tab label="Available Surveys">
            <div class="tab-content">
              <div *ngIf="isLoading" class="loading-container">
                <mat-spinner diameter="40"></mat-spinner>
                <p>Loading surveys...</p>
              </div>

              <div *ngIf="errorMessage" class="error-message">
                <mat-icon>error</mat-icon>
                {{ errorMessage }}
                <button mat-button color="primary" (click)="refreshData()">Retry</button>
              </div>

              <div *ngIf="!isLoading && !errorMessage" class="surveys-list">
                <div *ngIf="availableSurveys.length === 0" class="no-surveys">
                  <mat-icon>inbox</mat-icon>
                  <p>No surveys available at the moment.</p>
                  <p>Check back later for new surveys!</p>
                </div>

                <div *ngFor="let survey of availableSurveys" class="survey-item">
                  <div class="survey-info">
                    <h3>{{ survey.title }}</h3>
                    <p>{{ survey.description }}</p>
                    <div class="survey-meta">
                      <mat-chip-set>
                        <mat-chip color="primary" selected>
                          {{ survey.questions?.length || 0 }} questions
                        </mat-chip>
                        <mat-chip *ngIf="survey.startDate && survey.endDate" color="accent">
                          {{ survey.startDate | date:'shortDate' }} - {{ survey.endDate | date:'shortDate' }}
                        </mat-chip>
                      </mat-chip-set>
                    </div>
                  </div>
                  <div class="survey-actions">
                    <button mat-raised-button 
                            color="primary" 
                            (click)="takeSurvey(survey.id)"
                            [disabled]="hasResponded(survey.id)">
                      <mat-icon>{{ hasResponded(survey.id) ? 'check' : 'play_arrow' }}</mat-icon>
                      {{ hasResponded(survey.id) ? 'Completed' : 'Take Survey' }}
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </mat-tab>

          <!-- My Responses Tab -->
          <mat-tab label="My Responses">
            <div class="tab-content">
              <div *ngIf="isLoadingResponses" class="loading-container">
                <mat-spinner diameter="40"></mat-spinner>
                <p>Loading responses...</p>
              </div>

              <div *ngIf="!isLoadingResponses" class="responses-list">
                <div *ngIf="userResponses.length === 0" class="no-responses">
                  <mat-icon>assignment</mat-icon>
                  <p>You haven't completed any surveys yet.</p>
                  <p>Take a survey to see your responses here!</p>
                </div>

                <div *ngFor="let response of userResponses" class="response-item">
                  <div class="response-info">
                    <h3>{{ response.surveyTitle }}</h3>
                    <p>{{ response.surveyDescription }}</p>
                    <div class="response-meta">
                      <mat-chip-set>
                        <mat-chip color="success" selected>
                          Completed
                        </mat-chip>
                        <mat-chip color="accent">
                          {{ response.submissionDate | date:'medium' }}
                        </mat-chip>
                      </mat-chip-set>
                    </div>
                  </div>
                  <div class="response-actions">
                    <button mat-stroked-button color="primary" (click)="viewResponse(response.surveyId)">
                      <mat-icon>visibility</mat-icon>
                      View Response
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </mat-tab>
        </mat-tab-group>
      </mat-card>
    </div>
  `,
  styles: [`
    .dashboard-header {
      position: sticky;
      top: 0;
      z-index: 1000;
    }

    .header-content {
      display: flex;
      justify-content: space-between;
      align-items: center;
      width: 100%;
      max-width: 1200px;
      margin: 0 auto;
      padding: 0 20px;
    }

    .header-left h1 {
      margin: 0;
      font-size: 1.5rem;
    }

    .breadcrumb {
      font-size: 0.9rem;
      opacity: 0.9;
      margin-left: 16px;
    }

    .header-right {
      display: flex;
      align-items: center;
      gap: 16px;
    }

    .user-info {
      display: flex;
      align-items: center;
      gap: 8px;
      color: white;
    }

    .dashboard-container {
      padding: 20px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 16px;
      margin-bottom: 24px;
    }

    .stat-card {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
    }

    .stat-content {
      display: flex;
      align-items: center;
      gap: 16px;
    }

    .stat-icon {
      font-size: 2rem;
      width: 2rem;
      height: 2rem;
    }

    .stat-info h3 {
      margin: 0;
      font-size: 2rem;
      font-weight: bold;
    }

    .stat-info p {
      margin: 0;
      opacity: 0.9;
    }

    .main-content {
      margin-bottom: 24px;
    }

    .tab-content {
      padding: 20px;
    }

    .surveys-list,
    .responses-list {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .survey-item,
    .response-item {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 16px;
      padding: 20px;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      transition: all 0.3s ease;
    }

    .survey-item:hover,
    .response-item:hover {
      box-shadow: 0 4px 12px rgba(0,0,0,0.1);
      transform: translateY(-2px);
    }

    .survey-info,
    .response-info {
      flex: 1;
    }

    .survey-info h3,
    .response-info h3 {
      margin: 0 0 8px 0;
      color: #333;
    }

    .survey-info p,
    .response-info p {
      margin: 0 0 12px 0;
      color: #666;
      line-height: 1.5;
    }

    .survey-meta,
    .response-meta {
      margin-top: 8px;
    }

    .survey-actions,
    .response-actions {
      flex-shrink: 0;
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 40px;
      gap: 16px;
    }

    .error-message {
      display: flex;
      align-items: center;
      gap: 12px;
      color: #f44336;
      padding: 16px;
      background-color: #ffebee;
      border-radius: 8px;
      margin: 16px 0;
    }

    .no-surveys,
    .no-responses {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 60px 20px;
      color: #666;
      text-align: center;
    }

    .no-surveys mat-icon,
    .no-responses mat-icon {
      font-size: 4rem;
      width: 4rem;
      height: 4rem;
      margin-bottom: 16px;
      opacity: 0.5;
    }

    @media (max-width: 768px) {
      .header-content {
        padding: 0 16px;
      }

      .dashboard-container {
        padding: 16px;
      }

      .survey-item,
      .response-item {
        flex-direction: column;
        align-items: stretch;
      }

      .survey-actions,
      .response-actions {
        align-self: stretch;
      }

      .survey-actions button,
      .response-actions button {
        width: 100%;
      }
    }
  `]
})
export class UserDashboardComponent implements OnInit {
  availableSurveys: Survey[] = [];
  userResponses: UserResponse[] = [];
  currentUser: any = null;
  isLoading = false;
  isLoadingResponses = false;
  errorMessage = '';

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadCurrentUser();
    this.loadData();
  }

  loadCurrentUser() {
    this.currentUser = this.authService.getCurrentUser();
  }

  loadData() {
    this.loadAvailableSurveys();
    this.loadUserResponses();
  }

  loadAvailableSurveys() {
    this.isLoading = true;
    this.errorMessage = '';

    this.userService.getAvailableSurveys().subscribe({
      next: (surveys) => {
        this.availableSurveys = surveys;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading surveys:', error);
        this.errorMessage = 'Failed to load surveys: ' + (error.error?.message || error.message);
        this.isLoading = false;
        this.snackBar.open(this.errorMessage, 'Close', { duration: 5000 });
      }
    });
  }

  loadUserResponses() {
    this.isLoadingResponses = true;

    this.userService.getUserResponses().subscribe({
      next: (responses) => {
        this.userResponses = responses;
        this.isLoadingResponses = false;
      },
      error: (error) => {
        console.error('Error loading responses:', error);
        this.isLoadingResponses = false;
        // Don't show error for responses as it's not critical
      }
    });
  }

  get pendingSurveys(): Survey[] {
    return this.availableSurveys.filter(survey => !this.hasResponded(survey.id));
  }

  hasResponded(surveyId: number | undefined): boolean {
    if (!surveyId) return false;
    return this.userResponses.some(response => response.surveyId === surveyId);
  }

  takeSurvey(surveyId: number | undefined) {
    if (!surveyId) return;
    this.router.navigate(['/user/survey', surveyId]);
  }

  viewResponse(surveyId: number) {
    this.router.navigate(['/user/response', surveyId]);
  }

  refreshData() {
    this.loadData();
    this.snackBar.open('Data refreshed!', 'Close', { duration: 2000 });
  }

  logout() {
    if (confirm('Are you sure you want to logout?')) {
      this.authService.logout();
      this.snackBar.open('Logged out successfully!', 'Close', { duration: 2000 });
    }
  }
}
