import { Component, OnInit, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { SurveyService } from '../../shared/services/survey.service';
import { Survey } from '../../shared/models/survey.model';
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
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatMenuModule } from '@angular/material/menu';
import { MatChipsModule } from '@angular/material/chips';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialogModule } from '@angular/material/dialog';

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
    MatDividerModule,
    MatToolbarModule,
    MatMenuModule,
    MatChipsModule,
    MatBadgeModule,
    MatTooltipModule,
    MatDialogModule
  ],
  template: `
    <!-- Header with user info and logout -->
    <mat-toolbar color="primary" class="dashboard-header">
      <div class="header-content">
        <div class="header-left">
          <h1>Admin Dashboard</h1>
          <span class="breadcrumb">Welcome back, {{ currentUser?.email || 'Admin' }}!</span>
        </div>
        <div class="header-right">
          <span class="user-info">
            <mat-icon>account_circle</mat-icon>
            {{ currentUser?.email || 'Admin' }}
          </span>
          <button mat-icon-button [matMenuTriggerFor]="userMenu" matTooltip="User menu">
            <mat-icon>more_vert</mat-icon>
          </button>
          <mat-menu #userMenu="matMenu">
            <button mat-menu-item routerLink="/admin/survey-builder">
              <mat-icon>add</mat-icon>
              <span>Create Survey</span>
            </button>
            <mat-divider></mat-divider>
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
                <h3>{{ surveys.length }}</h3>
                <p>Total Surveys</p>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <mat-icon class="stat-icon">schedule</mat-icon>
              <div class="stat-info">
                <h3>{{ activeSurveys.length }}</h3>
                <p>Active Surveys</p>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <mat-icon class="stat-icon">question_answer</mat-icon>
              <div class="stat-info">
                <h3>{{ totalQuestions }}</h3>
                <p>Total Questions</p>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <mat-icon class="stat-icon">refresh</mat-icon>
              <div class="stat-info">
                <h3>{{ recentSurveys.length }}</h3>
                <p>Recent Surveys</p>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
      </div>

      <!-- Search and Filter Bar -->
      <div class="search-filter-bar">
        <mat-form-field appearance="outline" class="search-field">
          <mat-label>Search surveys</mat-label>
          <input matInput [(ngModel)]="searchTerm" (input)="filterSurveys()" placeholder="Search by title or description...">
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>

        <mat-form-field appearance="outline" class="filter-field">
          <mat-label>Filter by status</mat-label>
          <mat-select [(ngModel)]="statusFilter" (selectionChange)="filterSurveys()">
            <mat-option value="all">All Surveys</mat-option>
            <mat-option value="active">Active</mat-option>
            <mat-option value="expired">Expired</mat-option>
            <mat-option value="draft">Draft</mat-option>
          </mat-select>
        </mat-form-field>

        <button mat-stroked-button (click)="refreshSurveys()" [disabled]="isLoading" matTooltip="Refresh surveys (Ctrl+R)">
          <mat-icon>refresh</mat-icon>
          Refresh
        </button>
      </div>

      <!-- Quick Actions -->
      <div class="quick-actions">
        <button mat-raised-button color="primary" routerLink="/admin/survey-builder" class="action-button" matTooltip="Create new survey (Ctrl+N)">
          <mat-icon>add</mat-icon>
          Create New Survey
        </button>
        <button mat-stroked-button routerLink="/surveys" class="action-button" matTooltip="View all surveys">
          <mat-icon>list</mat-icon>
          View All Surveys
        </button>
      </div>

      <!-- Surveys List -->
      <mat-card class="surveys-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>list</mat-icon>
            Surveys
            <mat-chip-set>
              <mat-chip color="primary" selected>{{ filteredSurveys.length }} surveys</mat-chip>
            </mat-chip-set>
          </mat-card-title>
          <mat-card-subtitle>
            Manage your surveys and view their status
          </mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <div *ngIf="isLoading" class="loading-container">
            <mat-spinner diameter="40"></mat-spinner>
            <p>Loading surveys...</p>
          </div>

          <div *ngIf="errorMessage" class="error-message">
            <mat-icon>error</mat-icon>
            {{ errorMessage }}
            <button mat-button color="primary" (click)="fetchSurveys()">Retry</button>
          </div>

          <div *ngIf="!isLoading && !errorMessage" class="survey-list">
            <div *ngIf="filteredSurveys.length === 0" class="no-surveys">
              <mat-icon>inbox</mat-icon>
              <p>{{ searchTerm || statusFilter !== 'all' ? 'No surveys match your filters' : 'No surveys found. Create your first survey!' }}</p>
              <button *ngIf="searchTerm || statusFilter !== 'all'" mat-button color="primary" (click)="clearFilters()">
                Clear Filters
              </button>
            </div>
            
            <div *ngFor="let survey of filteredSurveys" class="survey-item">
              <div class="survey-header">
                <div class="survey-info">
                  <h3>{{ survey.title }}</h3>
                  <p>{{ survey.description }}</p>
                  <div class="survey-meta">
                    <mat-chip-set>
                      <mat-chip [color]="getSurveyStatusColor(survey)" selected>
                        {{ getSurveyStatus(survey) }}
                      </mat-chip>
                      <mat-chip *ngIf="survey.questions?.length" color="accent">
                        {{ survey.questions?.length }} questions
                      </mat-chip>
                    </mat-chip-set>
                    <div class="survey-dates">
                      <span class="date-info" *ngIf="survey.startDate && survey.endDate">
                        <mat-icon>schedule</mat-icon>
                        {{ survey.startDate | date:'shortDate' }} - {{ survey.endDate | date:'shortDate' }}
                      </span>
                      <span *ngIf="survey.createdAt" class="date-info">
                        <mat-icon>create</mat-icon>
                        Created: {{ survey.createdAt | date:'shortDate' }}
                      </span>
                    </div>
                  </div>
                </div>
                <div class="survey-actions">
                  <button mat-icon-button color="primary" (click)="editSurvey(survey.id)" matTooltip="Edit Survey">
                    <mat-icon>edit</mat-icon>
                  </button>
                  <button mat-icon-button color="accent" (click)="viewResults(survey.id!)" matTooltip="View Results">
                    <mat-icon>analytics</mat-icon>
                  </button>
                  <button mat-icon-button color="primary" (click)="exportSurveyToCsv(survey.id!)" matTooltip="Export to CSV">
                    <mat-icon>download</mat-icon>
                  </button>
                  <button mat-icon-button color="accent" (click)="shareSurvey(survey.id!)" matTooltip="Share Survey">
                    <mat-icon>share</mat-icon>
                  </button>
                  <button mat-icon-button color="warn" (click)="deleteSurvey(survey.id!, $event)" matTooltip="Delete Survey">
                    <mat-icon>delete</mat-icon>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </mat-card-content>
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

    .search-filter-bar {
      display: flex;
      gap: 16px;
      margin-bottom: 24px;
      align-items: center;
      flex-wrap: wrap;
    }

    .search-field {
      flex: 1;
      min-width: 300px;
    }

    .filter-field {
      min-width: 150px;
    }

    .quick-actions {
      display: flex;
      gap: 12px;
      margin-bottom: 24px;
      flex-wrap: wrap;
    }

    .action-button {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .surveys-card {
      margin-bottom: 24px;
    }

    .survey-list {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .survey-item {
      padding: 20px;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      transition: all 0.3s ease;
    }

    .survey-item:hover {
      box-shadow: 0 4px 12px rgba(0,0,0,0.1);
      transform: translateY(-2px);
    }

    .survey-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 16px;
    }

    .survey-info {
      flex: 1;
    }

    .survey-info h3 {
      margin: 0 0 8px 0;
      color: #333;
    }

    .survey-info p {
      margin: 0 0 12px 0;
      color: #666;
      line-height: 1.5;
    }

    .survey-meta {
      margin-top: 8px;
    }

    .survey-dates {
      margin-top: 8px;
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .date-info {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 0.85rem;
      color: #666;
    }

    .date-info mat-icon {
      font-size: 1rem;
      width: 1rem;
      height: 1rem;
    }

    .survey-actions {
      display: flex;
      gap: 4px;
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

    .no-surveys {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 60px 20px;
      color: #666;
      text-align: center;
    }

    .no-surveys mat-icon {
      font-size: 4rem;
      width: 4rem;
      height: 4rem;
      margin-bottom: 16px;
      opacity: 0.5;
    }

    mat-card-title {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    @media (max-width: 768px) {
      .header-content {
        padding: 0 16px;
      }

      .dashboard-container {
        padding: 16px;
      }

      .search-filter-bar {
        flex-direction: column;
        align-items: stretch;
      }

      .search-field,
      .filter-field {
        min-width: auto;
      }

      .survey-header {
        flex-direction: column;
        align-items: stretch;
      }

      .survey-actions {
        justify-content: flex-end;
      }
    }
  `]
})
export class DashboardComponent implements OnInit {
  surveys: Survey[] = [];
  filteredSurveys: Survey[] = [];
  isLoading = false;
  errorMessage = '';
  searchTerm = '';
  statusFilter = 'all';
  currentUser: any = null;

  constructor(
    private surveyService: SurveyService,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadCurrentUser();
    this.fetchSurveys();
  }

  @HostListener('window:keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    // Ctrl+R to refresh surveys
    if (event.ctrlKey && event.key === 'r') {
      event.preventDefault();
      this.refreshSurveys();
    }
    // Ctrl+N to create new survey
    if (event.ctrlKey && event.key === 'n') {
      event.preventDefault();
      this.router.navigate(['/admin/survey-builder']);
    }
  }

  loadCurrentUser() {
    // Get current user info from auth service
    this.currentUser = this.authService.getCurrentUser();
  }

  get activeSurveys(): Survey[] {
    const now = new Date();
    return this.surveys.filter(survey => {
      if (!survey.startDate || !survey.endDate) return false;
      const startDate = new Date(survey.startDate);
      const endDate = new Date(survey.endDate);
      return startDate <= now && endDate >= now;
    });
  }

  get recentSurveys(): Survey[] {
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);
    return this.surveys.filter(survey => {
      const dateToCheck = survey.createdAt || survey.startDate;
      if (!dateToCheck) return false;
      return new Date(dateToCheck) >= thirtyDaysAgo;
    });
  }

  get totalQuestions(): number {
    return this.surveys.reduce((total, survey) => 
      total + (survey.questions?.length || 0), 0
    );
  }

  fetchSurveys() {
    this.isLoading = true;
    this.errorMessage = '';
    console.log('Fetching surveys...');
    this.surveyService.getSurveys().subscribe({
      next: (data) => {
        console.log('Received surveys:', data);
        this.surveys = data;
        this.filterSurveys();
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

  filterSurveys() {
    let filtered = this.surveys;

    // Filter by search term
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(survey =>
        survey.title.toLowerCase().includes(term) ||
        survey.description.toLowerCase().includes(term)
      );
    }

    // Filter by status
    if (this.statusFilter !== 'all') {
      const now = new Date();
      filtered = filtered.filter(survey => {
        if (!survey.startDate || !survey.endDate) {
          return this.statusFilter === 'draft';
        }
        const startDate = new Date(survey.startDate);
        const endDate = new Date(survey.endDate);
        
        switch (this.statusFilter) {
          case 'active':
            return startDate <= now && endDate >= now;
          case 'expired':
            return endDate < now;
          case 'draft':
            return startDate > now;
          default:
            return true;
        }
      });
    }

    this.filteredSurveys = filtered;
  }

  clearFilters() {
    this.searchTerm = '';
    this.statusFilter = 'all';
    this.filterSurveys();
  }

  refreshSurveys() {
    this.fetchSurveys();
    this.snackBar.open('Surveys refreshed!', 'Close', { duration: 2000 });
  }

  getSurveyStatus(survey: Survey): string {
    const now = new Date();
    const startDate = survey.startDate ? new Date(survey.startDate) : null;
    const endDate = survey.endDate ? new Date(survey.endDate) : null;

    if (!startDate || !endDate) return 'Draft';
    if (startDate > now) return 'Draft';
    if (endDate < now) return 'Expired';
    return 'Active';
  }

  getSurveyStatusColor(survey: Survey): string {
    const status = this.getSurveyStatus(survey);
    switch (status) {
      case 'Active': return 'primary';
      case 'Expired': return 'warn';
      case 'Draft': return 'accent';
      default: return 'primary';
    }
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

  viewResults(surveyId: number) {
    this.router.navigate(['/admin/results', surveyId]);
  }

  deleteSurvey(surveyId: number, event: MouseEvent) {
    event.stopPropagation();
    console.log('Deleting survey with ID:', surveyId);
    if (!surveyId || surveyId <= 0) {
      console.error('Invalid survey ID:', surveyId);
      this.snackBar.open('Invalid survey ID', 'Close', { duration: 3000 });
      return;
    }
    if (confirm('Are you sure you want to delete this survey? This action cannot be undone.')) {
      this.surveyService.deleteSurvey(surveyId).subscribe({
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

  exportSurveys() {
    this.snackBar.open('Export feature coming soon!', 'Close', { duration: 3000 });
    // TODO: Implement survey export functionality
  }

  exportSurveyToCsv(surveyId: number) {
    this.surveyService.exportSurveyToCsv(surveyId).subscribe({
      next: (blob) => {
        // Create a download link
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `survey_${surveyId}_responses.csv`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
        this.snackBar.open('CSV file downloaded successfully!', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error exporting survey:', error);
        this.snackBar.open('Error exporting survey data', 'Close', { duration: 3000 });
      }
    });
  }

  shareSurvey(surveyId: number) {
    // Generate share link using the existing endpoint
    this.surveyService.getSurveyResults(surveyId).subscribe({
      next: (results) => {
        // Create shareable link (assuming frontend URL structure)
        const shareLink = `${window.location.origin}/survey/${surveyId}`;
        
        // Copy to clipboard
        navigator.clipboard.writeText(shareLink).then(() => {
          this.snackBar.open('Share link copied to clipboard!', 'Close', { duration: 3000 });
        }).catch(() => {
          // Fallback for older browsers
          const textArea = document.createElement('textarea');
          textArea.value = shareLink;
          document.body.appendChild(textArea);
          textArea.select();
          document.execCommand('copy');
          document.body.removeChild(textArea);
          this.snackBar.open('Share link copied to clipboard!', 'Close', { duration: 3000 });
        });
      },
      error: (error) => {
        console.error('Error generating share link:', error);
        // Fallback: create basic share link
        const shareLink = `${window.location.origin}/survey/${surveyId}`;
        navigator.clipboard.writeText(shareLink).then(() => {
          this.snackBar.open('Share link copied to clipboard!', 'Close', { duration: 3000 });
        }).catch(() => {
          this.snackBar.open('Share link: ' + shareLink, 'Close', { duration: 5000 });
        });
      }
    });
  }

  logout() {
    if (confirm('Are you sure you want to logout?')) {
      this.authService.logout();
      this.snackBar.open('Logged out successfully!', 'Close', { duration: 2000 });
    }
  }
} 