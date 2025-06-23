import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { NgxChartsModule } from '@swimlane/ngx-charts';

import { SurveyService } from '../../shared/services/survey.service';
import { SurveyResults, QuestionResult } from '../../shared/models/survey.model';

@Component({
  selector: 'app-survey-results',
  standalone: true,
  imports: [
    CommonModule,
    NgxChartsModule,
    MatCardModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatDividerModule,
    MatListModule,
  ],
  template: `
    <div class="results-container">
      <div *ngIf="isLoading" class="loading-container">
        <mat-spinner diameter="50"></mat-spinner>
        <p>Loading results...</p>
      </div>

      <div *ngIf="errorMessage" class="error-container">
        <mat-icon>error</mat-icon>
        <h2>Error</h2>
        <p>{{ errorMessage }}</p>
        <button mat-raised-button color="primary" (click)="goBack()">Back to Dashboard</button>
      </div>

      <div *ngIf="!isLoading && !errorMessage && results" class="results-content">
        <button mat-flat-button color="primary" (click)="goBack()" class="back-button">
          <mat-icon>arrow_back</mat-icon>
          Back to Dashboard
        </button>

        <mat-card class="results-header">
          <mat-card-title>{{ results.surveyTitle }}</mat-card-title>
          <mat-card-subtitle>Total Responses: {{ results.totalResponses }}</mat-card-subtitle>
        </mat-card>

        <div *ngFor="let question of results.questionResults" class="question-result">
          <mat-card>
            <mat-card-header>
              <mat-card-title>{{ question.questionText }}</mat-card-title>
              <mat-card-subtitle>Type: {{ question.questionType }}</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <ng-container [ngSwitch]="question.questionType">
                <div *ngSwitchCase="'rating'">
                  <ngx-charts-number-card
                    [results]="[{ name: 'Average Rating', value: question.averageRating || 0 }]"
                    [view]="[200, 100]"
                    scheme="cool">
                  </ngx-charts-number-card>
                </div>
                <div *ngSwitchCase="'text'">
                  <mat-list>
                    <mat-list-item *ngFor="let entry of getChartData(question)">
                      {{ entry.name }} ({{ entry.value }})
                    </mat-list-item>
                  </mat-list>
                </div>
                <div *ngSwitchDefault>
                  <ngx-charts-bar-vertical
                    [results]="getChartData(question)"
                    [xAxis]="true"
                    [yAxis]="true"
                    [legend]="false"
                    [showXAxisLabel]="true"
                    [showYAxisLabel]="true"
                    xAxisLabel="Response"
                    yAxisLabel="Count"
                    scheme="cool">
                  </ngx-charts-bar-vertical>
                </div>
              </ng-container>
            </mat-card-content>
          </mat-card>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .results-container { max-width: 900px; margin: 0 auto; padding: 20px; }
    .loading-container, .error-container { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 60px 20px; gap: 16px; text-align: center; }
    .error-container mat-icon { font-size: 4rem; width: 4rem; height: 4rem; color: #f44336; }
    .back-button { margin-bottom: 20px; }
    .results-header { text-align: center; margin-bottom: 24px; background: #3f51b5; color: white; }
    .question-result { margin-bottom: 20px; }
  `]
})
export class SurveyResultsComponent implements OnInit {
  results: SurveyResults | null = null;
  isLoading = false;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private surveyService: SurveyService
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadResults(+id);
    } else {
      this.errorMessage = "Survey ID not found in URL.";
    }
  }

  loadResults(id: number) {
    this.isLoading = true;
    this.errorMessage = '';
    this.surveyService.getSurveyResults(id).subscribe({
      next: (data) => {
        this.results = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = "Failed to load survey results.";
        this.isLoading = false;
      }
    });
  }

  getChartData(question: QuestionResult): { name: string, value: number }[] {
    return Object.entries(question.responseCounts).map(([name, value]) => ({ name, value }));
  }

  goBack() {
    this.router.navigate(['/admin/dashboard']);
  }
} 