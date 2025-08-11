import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { SurveyService, SurveyResultsDto } from '../../../../../common/services/survey.service';

@Component({
  selector: 'app-survey-results-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './survey-results-detail.html',
  styleUrls: ['./survey-results-detail.css']
})
export class SurveyResultsDetailComponent implements OnInit {
  surveyId!: number;
  survey: SurveyResultsDto | null = null;
  isLoading = true;
  error: string | null = null;

  constructor(private route: ActivatedRoute, private surveyService: SurveyService) {}

  ngOnInit() {
    this.surveyId = Number(this.route.snapshot.paramMap.get('id'));
    this.fetchSurveyResults();
  }

  fetchSurveyResults() {
    this.isLoading = true;
    this.surveyService.getSurveyResults(this.surveyId).subscribe({
      next: (data) => {
        console.log('Survey results API response:', data);
        this.survey = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Survey results API error:', err);
        this.error = 'Failed to load survey results. ' + (err?.error?.message || err.message || '');
        this.isLoading = false;
      }
    });
  }

  getKeys(obj: any): string[] {
    return obj ? Object.keys(obj) : [];
  }
}
