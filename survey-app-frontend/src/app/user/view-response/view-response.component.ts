import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { forkJoin } from 'rxjs';
import { MatListModule } from '@angular/material/list';

import { UserService, UserResponse } from '../../shared/services/user.service';
import { Survey } from '../../shared/models/survey.model';

@Component({
  selector: 'app-view-response',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatDividerModule,
    MatChipsModule,
    MatListModule,
  ],
  template: `
    <div class="view-response-container">
      <div *ngIf="isLoading" class="loading-container">
        <mat-spinner diameter="50"></mat-spinner>
        <p>Loading your response...</p>
      </div>

      <div *ngIf="errorMessage" class="error-container">
        <mat-icon>error</mat-icon>
        <h2>Error</h2>
        <p>{{ errorMessage }}</p>
        <button mat-raised-button color="primary" (click)="goBack()">Back to Dashboard</button>
      </div>

      <div *ngIf="!isLoading && !errorMessage && survey && userResponse" class="response-content">
        <mat-card class="survey-header">
          <mat-card-header>
            <mat-card-title>{{ survey.title }}</mat-card-title>
            <mat-card-subtitle>{{ survey.description }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <p>Submitted on: {{ userResponse.submissionDate | date:'fullDate' }}</p>
          </mat-card-content>
        </mat-card>

        <mat-card class="response-body">
          <mat-card-title>Your Answers</mat-card-title>
          <mat-divider></mat-divider>
          <mat-list>
            <div *ngFor="let question of survey.questions; let i = index">
              <mat-list-item>
                <div class="question-answer">
                  <span class="question-text">{{ i + 1 }}. {{ question.text }}</span>
                  <span class="answer-text">{{ getAnswer(question.id) }}</span>
                </div>
              </mat-list-item>
              <mat-divider></mat-divider>
            </div>
          </mat-list>
        </mat-card>
        
        <div class="form-actions">
          <button mat-raised-button color="primary" (click)="goBack()">Back to Dashboard</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .view-response-container { max-width: 800px; margin: 0 auto; padding: 20px; }
    .loading-container, .error-container { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 60px 20px; gap: 16px; text-align: center; }
    .error-container mat-icon { font-size: 4rem; width: 4rem; height: 4rem; color: #f44336; }
    .survey-header { margin-bottom: 24px; }
    .response-body { margin-bottom: 24px; }
    .question-answer { width: 100%; display: flex; justify-content: space-between; align-items: center; padding: 8px 0; }
    .question-text { font-weight: 500; }
    .answer-text { color: #3f51b5; font-style: italic; text-align: right; }
    .form-actions { display: flex; justify-content: center; margin-top: 24px; }
  `]
})
export class ViewResponseComponent implements OnInit {
  survey: Survey | null = null;
  userResponse: UserResponse | null = null;
  isLoading = false;
  errorMessage = '';
  private surveyId: number = 0;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.surveyId = +id;
      this.loadResponse();
    } else {
      this.errorMessage = "Survey ID not found in URL.";
    }
  }

  loadResponse() {
    this.isLoading = true;
    this.errorMessage = '';
    
    // Fetch both the survey structure and all user responses in parallel
    forkJoin({
      survey: this.userService.getSurvey(this.surveyId),
      userResponses: this.userService.getUserResponses()
    }).subscribe({
      next: ({ survey, userResponses }) => {
        this.survey = survey;
        // Find the specific response for this survey
        const matchingResponse = userResponses.find(r => r.surveyId === this.surveyId);
        
        if (matchingResponse) {
          this.userResponse = matchingResponse;
        } else {
          this.errorMessage = "You have not submitted a response for this survey.";
        }
        
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = "Failed to load your response. Please try again later.";
        this.isLoading = false;
      }
    });
  }

  getAnswer(questionId?: number): string {
    if (!questionId || !this.userResponse) {
      return "Not answered";
    }
    const answer = this.userResponse.responses.find(r => r.questionId === questionId);
    return answer ? answer.response : "Not answered";
  }

  goBack() {
    this.router.navigate(['/user/dashboard']);
  }
} 