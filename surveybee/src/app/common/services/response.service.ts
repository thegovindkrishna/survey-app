import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SubmitQuestionResponseDto {
  questionId: number;
  response: string;
}

export interface SubmitResponseDto {
  responses: SubmitQuestionResponseDto[];
}

export interface SurveyResponseDto {
  id: number;
  surveyId: number;
  respondentEmail: string;
  submissionDate: string;
  responses: any[];
}

@Injectable({ providedIn: 'root' })
export class ResponseService {
  private API_BASE = 'https://localhost:7245/api/surveys';

  constructor(private http: HttpClient) {}

  submitResponse(surveyId: number, response: SubmitResponseDto): Observable<any> {
    console.log(`Submitting response to: ${this.API_BASE}/${surveyId}/responses`);
    console.log('Response payload:', response);
    return this.http.post<any>(`${this.API_BASE}/${surveyId}/responses`, response);
  }

  getResponses(surveyId: number): Observable<SurveyResponseDto[]> {
    return this.http.get<SurveyResponseDto[]>(`${this.API_BASE}/${surveyId}/responses`);
  }

  getResponse(surveyId: number, responseId: number): Observable<SurveyResponseDto> {
    return this.http.get<SurveyResponseDto>(`${this.API_BASE}/${surveyId}/responses/${responseId}`);
  }
}
