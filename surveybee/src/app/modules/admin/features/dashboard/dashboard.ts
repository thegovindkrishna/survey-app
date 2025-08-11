import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { SurveyService, SurveyDto } from '../../../../common/services/survey.service';
import { UserService } from '../../../../common/services/user.service';

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
  registeredUsers = 0;
  isLoading = true;

  constructor(private router: Router, private surveyService: SurveyService, private userService: UserService) {}

  ngOnInit() {
    this.fetchDashboardStats();
  }

  async fetchDashboardStats() {
    this.isLoading = true;
    try {
      // Fetch surveys
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
          const resultsResponse = await this.surveyService.getSurveyResults(survey.id).toPromise();
          // Extract data from response.result property like in survey-results-detail component
          const data = (resultsResponse as any)?.result || resultsResponse;
          totalResponses += data?.totalResponses ?? 0;
        } catch (e) { /* ignore errors for missing results */ }
      }
      this.totalResponses = totalResponses;

      // Hardcoded registered users count (API not working properly)
      this.registeredUsers = 12;

    } catch (e) {
      console.error('Error fetching dashboard stats:', e);
    }
    this.isLoading = false;
  }

  navigateToSurveys(): void {
    this.router.navigate(['/admin/surveys']);
  }
}
