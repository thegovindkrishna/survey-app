import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SurveyService, SurveyDto } from '../../../../../common/services/survey.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-survey-results',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './survey-results.html',
  styleUrls: ['./survey-results.css']
})
export class SurveyResultsComponent {
  surveys: SurveyDto[] = [];
  isLoading = true;
  error: string | null = null;

  constructor(private surveyService: SurveyService, private router: Router) {
    this.fetchSurveys();
  }

  fetchSurveys() {
    this.isLoading = true;
    this.surveyService.getAllSurveys().subscribe({
      next: (data) => {
        this.surveys = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Failed to load surveys.';
        this.isLoading = false;
      }
    });
  }

  goToSurveyResults(surveyId: number) {
    this.router.navigate(['/admin/surveys', surveyId, 'results']);
  }
}
