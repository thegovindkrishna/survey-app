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
    if (this.surveyId && !isNaN(this.surveyId)) {
      this.fetchSurveyResults();
    } else {
      this.error = 'Invalid survey ID';
      this.isLoading = false;
    }
  }

  fetchSurveyResults() {
    this.isLoading = true;
    this.error = null;
    
    this.surveyService.getSurveyResults(this.surveyId).subscribe({
      next: (response) => {
        if (!response) {
          this.error = 'No data received from server';
          this.isLoading = false;
          return;
        }

        // Extract the actual data from the response.result property
        const data = (response as any).result || response;
        
        if (!data) {
          this.error = 'No survey results found in response';
          this.isLoading = false;
          return;
        }

        // Ensure questionResults is an array
        if (data.questionResults && !Array.isArray(data.questionResults)) {
          data.questionResults = [];
        } else if (!data.questionResults) {
          data.questionResults = [];
        }

        // Ensure totalResponses is a valid number
        if (typeof data.totalResponses !== 'number' || isNaN(data.totalResponses)) {
          data.totalResponses = 0;
        }

        // Ensure each question result has the required properties
        if (Array.isArray(data.questionResults)) {
          data.questionResults = data.questionResults.map((qr: any) => ({
            ...qr,
            responseCounts: qr.responseCounts || {},
            averageRating: qr.averageRating || undefined
          }));
        }

        this.survey = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Failed to load survey results. ' + (err?.error?.message || err.message || 'Unknown error');
        this.isLoading = false;
      }
    });
  }

  getKeys(obj: any): string[] {
    return obj ? Object.keys(obj) : [];
  }

  getBarWidth(responseCounts: any, option: string, totalResponses?: number): number {
    if (!responseCounts || !option || !totalResponses || totalResponses === 0) {
      return 0;
    }
    const count = responseCounts[option] || 0;
    return (count / totalResponses) * 100;
  }
}
