import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SurveyService, SurveyDto, PaginatedSurveys } from '../../../../../common/services/survey.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-view-surveys',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './view-surveys.html',
  styleUrls: ['./view-surveys.css']
})
export class ViewSurveysComponent {
  surveys: SurveyDto[] = [];
  isLoading = true;
  error: string | null = null;
  totalCount = 0;
  currentPage = 1;
  pageSize = 5;

  constructor(private surveyService: SurveyService, private router: Router) {
    this.fetchSurveys();
  }

  fetchSurveys(page: number = 1) {
    this.isLoading = true;
    this.surveyService.getSurveysPaginated(page, this.pageSize).subscribe({
      next: (data: any) => {
        let surveys: SurveyDto[] = [];
        let totalCount = 0;
        let currentPage = page;
        // LOG the data for debugging
        console.log('Survey API response:', data);
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
        } else if (data && Array.isArray(data.result)) {
          surveys = data.result;
          totalCount = typeof data.totalCount === 'number' ? data.totalCount : surveys.length;
          currentPage = typeof data.currentPage === 'number' ? data.currentPage : page;
        } else {
          surveys = [];
          totalCount = 0;
        }
        if (!Array.isArray(surveys)) {
          surveys = [];
        }
        // Sort by startDate descending (most recent first)
        this.surveys = surveys.sort((a, b) => {
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
        this.totalCount = totalCount;
        this.currentPage = currentPage;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Failed to load surveys.';
        this.isLoading = false;
      }
    });
  }

  goToDetails(id: number) {
    this.router.navigate(['/admin/surveys', id]);
  }

  goToResults(id: number) {
    this.router.navigate(['/admin/surveys', id, 'results']);
  }

  nextPage() {
    if (this.currentPage * this.pageSize < this.totalCount) {
      this.fetchSurveys(this.currentPage + 1);
    }
  }

  prevPage() {
    if (this.currentPage > 1) {
      this.fetchSurveys(this.currentPage - 1);
    }
  }

  // Call this after creating a survey to refresh the first page
  refreshFirstPage() {
    this.currentPage = 1;
    this.fetchSurveys(1);
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.fetchSurveys(page);
    }
  }

  get totalPages() {
    return this.totalCount === 0 ? 1 : Math.ceil(this.totalCount / this.pageSize);
  }

  getPageArray() {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  // --- Delete Survey ---
  deleteSurvey(id: number) {
    if (!confirm('Are you sure you want to delete this survey? This action cannot be undone.')) return;
    this.surveyService.deleteSurvey(id).subscribe({
      next: () => {
        this.fetchSurveys(this.currentPage);
      },
      error: (err) => {
        alert('Failed to delete survey.');
      }
    });
  }
}
