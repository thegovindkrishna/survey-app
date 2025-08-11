import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface QuestionCreateDto {
  questionText: string;
  type: string;
  required: boolean;
  options?: string[];
  maxRating?: number;
}

export interface SurveyCreateDto {
  title: string;
  description?: string;
  startDate: string;
  endDate: string;
  questions: QuestionCreateDto[];
}

export interface QuestionDto {
  id: number;
  questionText: string;
  type: string;
  required: boolean;
  options?: string[];
  maxRating?: number;
}

export interface SurveyDto {
  id: number;
  title: string;
  description?: string;
  startDate: string;
  endDate: string;
  createdBy: string;
  shareLink?: string;
  questions: QuestionDto[];
}

export interface PaginatedSurveys {
  items: SurveyDto[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
}

// --- Survey Results Analytics ---
export interface QuestionResultDto {
  questionId: number;
  questionText: string;
  questionType: string;
  responseCounts: { [option: string]: number };
  averageRating?: number;
}

export interface SurveyResultsDto {
  surveyId: number;
  surveyTitle: string;
  totalResponses: number;
  questionResults: QuestionResultDto[];
}

@Injectable({ providedIn: 'root' })
export class SurveyService {
  private API_BASE = 'https://localhost:7245/api/v1.0'; // <-- Use versioned API path matching backend

  constructor(private http: HttpClient) {}

  createSurvey(survey: SurveyCreateDto): Observable<any> {
    return this.http.post(`${this.API_BASE}/survey`, survey);
  }

  getAllSurveys(): Observable<SurveyDto[]> {
    // Use default pagination params to ensure backend returns data
    return this.http.get<SurveyDto[]>(`${this.API_BASE}/survey?pageNumber=1&pageSize=100`);
  }

  getSurveysPaginated(pageNumber = 1, pageSize = 5): Observable<PaginatedSurveys> {
    // Always request sorted by newest first
    return this.http.get<PaginatedSurveys>(`${this.API_BASE}/survey?pageNumber=${pageNumber}&pageSize=${pageSize}&sortBy=startDate&sortOrder=desc`);
  }

  getSurveyById(id: number) {
    return this.http.get<SurveyDto>(`${this.API_BASE}/survey/${id}`);
  }

  updateSurvey(id: number, survey: any) {
    return this.http.put(`${this.API_BASE}/survey/${id}`, survey);
  }

  // --- Survey Results Analytics --- (Different controller, different base path)
  getSurveyResults(surveyId: number): Observable<SurveyResultsDto> {
    return this.http.get<SurveyResultsDto>(`https://localhost:7245/api/surveys/${surveyId}/results`);
  }

  // --- Delete Survey ---
  deleteSurvey(id: number): Observable<any> {
    return this.http.delete(`${this.API_BASE}/survey/${id}`);
  }
}
