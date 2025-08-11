import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { SurveyService, SurveyDto } from '../../../../common/services/survey.service';
import { ResponseService, SurveyResponseDto } from '../../../../common/services/response.service';
import { UserService, UserResponseDto } from '../../../../common/services/user.service';
import { AuthService } from '../../../../common/services/auth.service';
import { DashboardRefreshService } from '../../../../common/services/dashboard-refresh.service';
import { ResponseViewerComponent, ResponseViewerData } from '../response-viewer/response-viewer.component';
import { Subscription } from 'rxjs';

interface UserActivity {
  icon: string;
  title: string;
  description: string;
  time: string;
}

interface UserSurveyResponse {
  id: number;
  surveyId: number;
  surveyTitle: string;
  surveyDescription: string;
  submissionDate: string;
  respondentEmail: string;
  responses: any[];
}

@Component({
  selector: 'app-user-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class UserDashboardComponent implements OnInit, OnDestroy {
  availableSurveys: SurveyDto[] = [];
  completedSurveys: SurveyDto[] = [];
  userResponses: UserSurveyResponse[] = [];
  recentActivity: UserActivity[] = [];
  isLoading = false;
  errorMessage = '';
  userName = '';
  
  // Pagination for available surveys
  currentPage = 1;
  pageSize = 6;
  totalCount = 0;
  
  // Statistics
  totalResponses = 0;
  surveyParticipationRate = 0;
  
  // Make Math available in template
  Math = Math;

  private refreshSubscription?: Subscription;

  constructor(
    private surveyService: SurveyService, 
    private responseService: ResponseService,
    private userService: UserService,
    private authService: AuthService,
    private dashboardRefreshService: DashboardRefreshService,
    private router: Router,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.loadUserName();
    this.loadDashboardData();
    
    // Subscribe to refresh signals
    this.refreshSubscription = this.dashboardRefreshService.refresh$.subscribe(() => {
      this.loadDashboardData();
    });
  }

  ngOnDestroy() {
    if (this.refreshSubscription) {
      this.refreshSubscription.unsubscribe();
    }
  }

  loadUserName() {
    // Try to get user name from local storage or token
    const token = localStorage.getItem('token');
    if (token) {
      try {
        // Decode JWT token to get user info
        const tokenPayload = JSON.parse(atob(token.split('.')[1]));
        this.userName = tokenPayload.sub || tokenPayload.email || tokenPayload.unique_name || 'User';
        console.log('User name loaded:', this.userName);
      } catch (error) {
        console.warn('Could not decode token for user name:', error);
        this.userName = 'User';
      }
    } else {
      this.userName = 'User';
    }
  }

  refreshDashboard() {
    console.log('Refreshing dashboard...');
    this.currentPage = 1; // Reset to first page on refresh
    this.loadDashboardData();
  }

  loadDashboardData() {
    console.log('Loading dashboard data...');
    this.isLoading = true;
    this.errorMessage = '';

    // Load available surveys with pagination
    this.loadAvailableSurveys();
  }

  loadAvailableSurveys(page: number = 1) {
    this.currentPage = page;
    console.log(`Loading available surveys - page ${page}, pageSize ${this.pageSize}`);
    
    this.surveyService.getSurveysPaginated(page, this.pageSize).subscribe({
      next: (data: any) => {
        console.log('Survey API response:', data);
        
        let surveys: SurveyDto[] = [];
        let totalCount = 0;
        let currentPage = page;
        
        // Handle backend response with nested 'result' object
        if (data && data.result && Array.isArray(data.result.items)) {
          surveys = data.result.items;
          totalCount = typeof data.result.totalCount === 'number' ? data.result.totalCount : surveys.length;
          currentPage = typeof data.result.currentPage === 'number' ? data.result.currentPage : page;
        } else if (Array.isArray(data)) {
          surveys = data;
          totalCount = data.length;
          currentPage = 1;
        } else if (data && Array.isArray(data.items)) {
          surveys = data.items;
          totalCount = typeof data.totalCount === 'number' ? data.totalCount : surveys.length;
          currentPage = typeof data.currentPage === 'number' ? data.currentPage : page;
        } else {
          surveys = [];
          totalCount = 0;
        }

        if (!Array.isArray(surveys)) {
          surveys = [];
        }

        // Filter for active surveys only
        const now = new Date();
        this.availableSurveys = surveys.filter((survey: SurveyDto) => {
          const startDate = new Date(survey.startDate);
          const endDate = new Date(survey.endDate);
          return startDate <= now && endDate >= now;
        });

        console.log(`Filtered ${this.availableSurveys.length} active surveys from ${surveys.length} total surveys`);
        
        this.totalCount = totalCount;
        this.currentPage = currentPage;
        
        // Load user responses after surveys are loaded
        this.loadUserResponses();
      },
      error: (err: any) => {
        console.error('Failed to load available surveys:', err);
        this.availableSurveys = [];
        this.loadUserResponses(); // Still try to load responses
      }
    });
  }

  loadUserResponses() {
    this.userService.getUserResponses().subscribe({
      next: (responses: any) => {
        console.log('User responses raw response:', responses);
        console.log('Response type:', typeof responses);
        console.log('Response structure:', JSON.stringify(responses, null, 2));
        
        // Handle the response structure - it might be nested
        let responseList: UserResponseDto[] = [];
        if (Array.isArray(responses)) {
          responseList = responses;
        } else if (responses?.result && Array.isArray(responses.result)) {
          responseList = responses.result;
        } else if (responses?.items && Array.isArray(responses.items)) {
          responseList = responses.items;
        } else {
          console.warn('Unexpected response structure for user responses:', responses);
          responseList = [];
        }

        console.log('Processed response list:', responseList);

        // Transform to UserSurveyResponse format with proper property mapping
        this.userResponses = responseList.map((response: any, index: number) => {
          // Handle both camelCase and PascalCase properties from backend
          const userResponse: UserSurveyResponse = {
            id: response.responseId || response.ResponseId || index + 1,
            surveyId: response.surveyId || response.SurveyId,
            surveyTitle: response.surveyTitle || response.SurveyTitle || `Survey ${response.surveyId || response.SurveyId}`,
            surveyDescription: response.surveyDescription || response.SurveyDescription || 'No description available',
            submissionDate: response.submissionDate || response.SubmissionDate || new Date().toISOString(),
            respondentEmail: 'user@example.com',
            responses: response.responses || response.Responses || []
          };
          
          console.log(`Mapped user response ${index + 1}:`, userResponse);
          return userResponse;
        });

        // Create completed surveys list based on user responses
        this.completedSurveys = this.userResponses.map(response => ({
          id: response.surveyId,
          title: response.surveyTitle,
          description: response.surveyDescription,
          startDate: '',
          endDate: '',
          createdBy: '',
          shareLink: '',
          questions: []
        }));

        // Remove completed surveys from available surveys to avoid duplication
        const completedSurveyIds = this.userResponses.map(r => r.surveyId);
        this.availableSurveys = this.availableSurveys.filter(survey => 
          !completedSurveyIds.includes(survey.id)
        );

        console.log('Final dashboard data after loading user responses:', {
          availableSurveys: this.availableSurveys.length,
          userResponses: this.userResponses.length,
          completedSurveys: this.completedSurveys.length,
          completedSurveyIds: completedSurveyIds
        });

        this.calculateStatistics();
        this.generateRecentActivity();
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Failed to load user responses:', err);
        console.error('Error details:', {
          status: err.status,
          statusText: err.statusText,
          message: err.message,
          error: err.error
        });
        this.userResponses = [];
        this.completedSurveys = [];
        
        // If we have no data at all, show fallback
        if (this.availableSurveys.length === 0) {
          console.log('No data available, loading fallback...');
          this.loadFallbackData();
        } else {
          this.calculateStatistics();
          this.generateRecentActivity();
        }
        
        this.isLoading = false;
      }
    });
  }

  loadFallbackData() {
    console.log('Loading fallback mock data...');
    
    // Provide some mock data as fallback when backend is not available
    this.availableSurveys = [
      {
        id: 1,
        title: 'Customer Satisfaction Survey',
        description: 'Help us improve our services by sharing your feedback',
        startDate: new Date(Date.now() - 86400000).toISOString(),
        endDate: new Date(Date.now() + 604800000).toISOString(),
        createdBy: 'admin@example.com',
        shareLink: '',
        questions: [
          { id: 1, questionText: 'How satisfied are you with our service?', type: 'rating', required: true },
          { id: 2, questionText: 'Any additional feedback?', type: 'textarea', required: false }
        ]
      },
      {
        id: 2,
        title: 'Product Feedback Survey',
        description: 'Share your thoughts on our latest product release',
        startDate: new Date(Date.now() - 172800000).toISOString(),
        endDate: new Date(Date.now() + 1209600000).toISOString(),
        createdBy: 'admin@example.com',
        shareLink: '',
        questions: [
          { id: 3, questionText: 'Rate the product quality', type: 'rating', required: true },
          { id: 4, questionText: 'Would you recommend this product?', type: 'radio', required: true, options: ['Yes', 'No', 'Maybe'] }
        ]
      }
    ];

    // Mock user responses
    this.userResponses = [
      {
        id: 1,
        surveyId: 3,
        surveyTitle: 'Employee Engagement Survey',
        surveyDescription: 'Annual employee engagement survey',
        submissionDate: new Date(Date.now() - 432000000).toISOString(),
        respondentEmail: 'user@example.com',
        responses: [
          { questionId: 5, response: '4' },
          { questionId: 6, response: 'Great workplace culture and supportive team environment' }
        ]
      }
    ];

    // Create completed surveys list
    this.completedSurveys = this.userResponses.map(response => ({
      id: response.surveyId,
      title: response.surveyTitle,
      description: response.surveyDescription,
      startDate: '',
      endDate: '',
      createdBy: '',
      shareLink: '',
      questions: []
    }));

    this.calculateStatistics();
    this.generateRecentActivity();
  }

  calculateStatistics() {
    this.totalResponses = this.userResponses.length;
    const totalSurveys = this.availableSurveys.length + this.completedSurveys.length;
    this.surveyParticipationRate = totalSurveys > 0 ? Math.round((this.completedSurveys.length / totalSurveys) * 100) : 0;
  }

  generateRecentActivity() {
    this.recentActivity = [];
    
    // Add recent survey submissions
    this.userResponses.forEach(response => {
      this.recentActivity.push({
        icon: '✅',
        title: 'Survey Completed',
        description: `Completed "${response.surveyTitle}"`,
        time: this.formatRelativeTime(response.submissionDate)
      });
    });

    // Sort by most recent
    this.recentActivity.sort((a, b) => new Date(b.time).getTime() - new Date(a.time).getTime());
    
    // Limit to 5 most recent activities
    this.recentActivity = this.recentActivity.slice(0, 5);
  }

  formatRelativeTime(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000);

    if (diffInSeconds < 60) return 'Just now';
    if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)} minutes ago`;
    if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)} hours ago`;
    if (diffInSeconds < 604800) return `${Math.floor(diffInSeconds / 86400)} days ago`;
    return date.toLocaleDateString();
  }

  attendSurvey(surveyId: number) {
    console.log('Navigating to attend survey:', surveyId);
    this.router.navigate(['/user/attend', surveyId]);
  }

  viewResponse(responseId: number) {
    console.log('Opening response viewer for response ID:', responseId);
    
    // Find the response in userResponses
    const response = this.userResponses.find(r => r.id === responseId);
    if (!response) {
      console.error('Response not found:', responseId);
      alert('Response not found. Please refresh the page and try again.');
      return;
    }

    // Try to get the full survey details to enhance the response view
    this.surveyService.getSurveyById(response.surveyId).subscribe({
      next: (survey: any) => {
        console.log('Survey details for response viewer:', survey);
        
        // Extract survey data (handle both camelCase and PascalCase)
        const surveyData = survey?.result || survey;
        const questions = surveyData?.questions || surveyData?.Questions || [];
        
        // Prepare enhanced response data
        const modalData: ResponseViewerData = {
          surveyTitle: response.surveyTitle,
          surveyDescription: response.surveyDescription,
          submissionDate: response.submissionDate,
          responses: response.responses.map((r, index) => {
            // Find matching question for enhanced display
            const question = questions.find((q: any) => 
              (q.id || q.Id) === (r.questionId || r.QuestionId)
            );
            
            return {
              questionId: r.questionId || r.QuestionId || index + 1,
              response: r.response || r.Response || 'No response provided',
              questionText: question?.questionText || question?.QuestionText || `Question ${index + 1}`,
              questionType: question?.type || question?.Type || 'text'
            };
          })
        };

        this.openResponseModal(modalData);
      },
      error: (err) => {
        console.warn('Could not fetch survey details, using basic response data:', err);
        
        // Fallback to basic response data
        const modalData: ResponseViewerData = {
          surveyTitle: response.surveyTitle,
          surveyDescription: response.surveyDescription,
          submissionDate: response.submissionDate,
          responses: response.responses.map((r, index) => ({
            questionId: r.questionId || index + 1,
            response: r.response || 'No response provided',
            questionText: `Question ${index + 1}`,
            questionType: 'text'
          }))
        };

        this.openResponseModal(modalData);
      }
    });
  }

  private openResponseModal(modalData: ResponseViewerData) {
    // Open the modal
    const dialogRef = this.dialog.open(ResponseViewerComponent, {
      width: '90vw',
      maxWidth: '800px',
      height: '90vh',
      maxHeight: '600px',
      data: modalData,
      panelClass: 'response-viewer-dialog',
      autoFocus: false,
      restoreFocus: false,
      hasBackdrop: true,
      disableClose: false
    });

    // Handle modal close
    dialogRef.afterClosed().subscribe(result => {
      console.log('Response viewer modal closed');
      // You can add any cleanup logic here if needed
    });
  }

  navigateTo(path: string): void {
    this.router.navigate([path]);
  }

  logout(): void {
    this.authService.logout();
  }

  // Pagination methods
  goToPage(page: number) {
    if (page >= 1 && page <= this.getTotalPages()) {
      this.loadAvailableSurveys(page);
    }
  }

  previousPage() {
    if (this.currentPage > 1) {
      this.goToPage(this.currentPage - 1);
    }
  }

  nextPage() {
    if (this.currentPage < this.getTotalPages()) {
      this.goToPage(this.currentPage + 1);
    }
  }

  getTotalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  getPageNumbers(): number[] {
    const totalPages = this.getTotalPages();
    const pages: number[] = [];
    const maxVisiblePages = 5;
    
    let startPage = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
    let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);
    
    if (endPage - startPage + 1 < maxVisiblePages) {
      startPage = Math.max(1, endPage - maxVisiblePages + 1);
    }
    
    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    
    return pages;
  }
}
