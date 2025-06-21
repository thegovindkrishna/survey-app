import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Survey } from '../models/survey.model';

export interface UserResponse {
  surveyId: number;
  surveyTitle: string;
  surveyDescription: string;
  submissionDate: Date;
  responseId: number;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = 'https://localhost:7245/api/user';

  constructor(private http: HttpClient) {}

  /**
   * Maps backend survey question format to frontend format.
   */
  private mapSurvey(backendSurvey: any): Survey {
    if (backendSurvey && backendSurvey.questions) {
      backendSurvey.questions = backendSurvey.questions.map((q: any) => {
        const { questionText, ...rest } = q;
        return {
          ...rest,
          text: questionText,
        };
      });
    }
    return backendSurvey as Survey;
  }

  /**
   * Get all available surveys that users can respond to
   */
  getAvailableSurveys(): Observable<Survey[]> {
    return this.http.get<any[]>(`${this.apiUrl}/surveys`).pipe(
      map(surveys => surveys.map(this.mapSurvey))
    );
  }

  /**
   * Get a specific survey by ID for user viewing
   */
  getSurvey(id: number): Observable<Survey> {
    return this.http.get<any>(`${this.apiUrl}/surveys/${id}`).pipe(
      map(this.mapSurvey)
    );
  }

  /**
   * Get responses submitted by the current user
   */
  getUserResponses(): Observable<UserResponse[]> {
    return this.http.get<UserResponse[]>(`${this.apiUrl}/responses`);
  }

  /**
   * Submit a survey response
   */
  submitResponse(surveyId: number, response: any): Observable<any> {
    return this.http.post(`https://localhost:7245/api/surveys/${surveyId}/responses`, response);
  }
} 