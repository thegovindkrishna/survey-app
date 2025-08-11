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
        // Sort surveys by newest first (by startDate or ID)
        this.surveys = (Array.isArray(data) ? data : []).sort((a, b) => {
          // First try to sort by startDate
          const dateA = new Date(a.startDate);
          const dateB = new Date(b.startDate);
          
          // If dates are valid, sort by them
          if (!isNaN(dateA.getTime()) && !isNaN(dateB.getTime())) {
            return dateB.getTime() - dateA.getTime(); // Newest first
          }
          
          // If dates are invalid, fall back to ID (assuming higher ID = newer)
          return (b.id || 0) - (a.id || 0); // Higher ID first
        });
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
