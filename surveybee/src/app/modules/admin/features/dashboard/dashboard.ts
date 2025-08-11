import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { SurveyService, SurveyDto } from '../../../../common/services/survey.service';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
  providers: [SurveyService]
})
export class Dashboard implements OnInit {
  totalSurveys = 0;
  activeSurveys = 0;
  totalResponses = 0;
  registeredUsers = 'N/A'; // Placeholder, update if endpoint is available
  isLoading = true;

  constructor(private router: Router, private surveyService: SurveyService) {}

  ngOnInit() {
    this.fetchDashboardStats();
  }

  async fetchDashboardStats() {
    this.isLoading = true;
    try {
      const surveysResponse: any = (await this.surveyService.getAllSurveys().toPromise()) ?? [];
      
      // Handle different response structures
      let surveys: SurveyDto[] = [];
      if (Array.isArray(surveysResponse)) {
        surveys = surveysResponse;
      } else if (surveysResponse?.result?.items) {
        surveys = surveysResponse.result.items;
      } else if (surveysResponse?.items) {
        surveys = surveysResponse.items;
      }
      
      // Sort surveys by newest first for better statistics
      surveys = surveys.sort((a, b) => {
        const dateA = new Date(a.startDate);
        const dateB = new Date(b.startDate);
        
        if (!isNaN(dateA.getTime()) && !isNaN(dateB.getTime())) {
          return dateB.getTime() - dateA.getTime(); // Newest first
        }
        
        return (b.id || 0) - (a.id || 0); // Higher ID first
      });
      
      this.totalSurveys = surveys.length;
      const now = new Date();
      this.activeSurveys = surveys.filter((s: SurveyDto) => {
        const start = new Date(s.startDate);
        const end = new Date(s.endDate);
        return start <= now && end >= now;
      }).length;
      // Fetch total responses for all surveys
      let totalResponses = 0;
      for (const survey of surveys) {
        try {
          const results = await this.surveyService.getSurveyResults(survey.id).toPromise();
          totalResponses += results?.totalResponses ?? 0;
        } catch (e) { /* ignore errors for missing results */ }
      }
      this.totalResponses = totalResponses;
    } catch (e) {
      console.error('Error fetching dashboard stats:', e);
    }
    this.isLoading = false;
  }

  navigateToSurveys(): void {
    this.router.navigate(['/admin/surveys']);
  }
}
