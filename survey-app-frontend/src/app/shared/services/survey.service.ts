import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Survey, Question, SurveyResults } from '../models/survey.model';

@Injectable({
  providedIn: 'root'
})
export class SurveyService {
  private apiUrl = 'https://localhost:7245/api/surveys';

  constructor(private http: HttpClient) {}

  private handleError(error: HttpErrorResponse) {
    console.error('An error occurred:', error);
    let errorMessage = 'An error occurred';
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    } else {
      // Server-side error
      errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
    }
    return throwError(() => new Error(errorMessage));
  }

  // Helper method to map backend question to frontend question
  private mapQuestion(backendQuestion: any): Question {
    return {
      id: backendQuestion.id,
      text: backendQuestion.questionText || backendQuestion.text, // Handle both property names
      type: backendQuestion.type,
      required: backendQuestion.required,
      options: backendQuestion.options || [],
      maxRating: backendQuestion.maxRating
    };
  }

  // Helper method to map backend survey to frontend survey
  private mapSurvey(backendSurvey: any): Survey {
    return {
      id: backendSurvey.id,
      title: backendSurvey.title,
      description: backendSurvey.description,
      startDate: backendSurvey.startDate ? new Date(backendSurvey.startDate) : undefined,
      endDate: backendSurvey.endDate ? new Date(backendSurvey.endDate) : undefined,
      createdBy: backendSurvey.createdBy,
      createdAt: backendSurvey.createdAt ? new Date(backendSurvey.createdAt) : undefined,
      shareLink: backendSurvey.shareLink,
      questions: (backendSurvey.questions || []).map((q: any) => this.mapQuestion(q))
    };
  }

  createSurvey(survey: Survey): Observable<Survey> {
    console.log('Creating survey:', survey);
    
    // Map frontend structure to backend structure
    const backendSurvey = {
      title: survey.title,
      description: survey.description,
      startDate: survey.startDate,
      endDate: survey.endDate,
      questions: (survey.questions || []).map(q => ({
        questionText: q.text,
        type: q.type,
        required: q.required,
        options: q.options,
        maxRating: q.maxRating
      }))
    };
    
    console.log('Backend survey structure for creation:', backendSurvey);
    
    return this.http.post<Survey>(this.apiUrl, backendSurvey).pipe(
      map(response => {
        console.log('Create response:', response);
        return this.mapSurvey(response);
      }),
      catchError(this.handleError)
    );
  }

  getSurveys(): Observable<Survey[]> {
    return this.http.get<Survey[]>(this.apiUrl).pipe(
      map(response => {
        console.log('Received surveys:', response);
        return response.map(survey => this.mapSurvey(survey));
      }),
      catchError(this.handleError)
    );
  }

  getSurvey(id: number): Observable<Survey> {
    console.log('Getting survey:', id);
    return this.http.get<Survey>(`${this.apiUrl}/${id}`).pipe(
      map(response => {
        console.log('Received survey:', response);
        return this.mapSurvey(response);
      }),
      catchError(this.handleError)
    );
  }

  updateSurvey(id: number, survey: Survey): Observable<Survey> {
    console.log('Updating survey:', id, survey);
    
    // Map frontend structure to backend structure
    const backendSurvey = {
      title: survey.title,
      description: survey.description,
      startDate: survey.startDate,
      endDate: survey.endDate,
      questions: (survey.questions || []).map(q => ({
        questionText: q.text,
        type: q.type,
        required: q.required,
        options: q.options,
        maxRating: q.maxRating
      }))
    };
    
    console.log('Backend survey structure for update:', backendSurvey);
    
    return this.http.put<Survey>(`${this.apiUrl}/${id}`, backendSurvey).pipe(
      map(response => {
        console.log('Raw update response:', response);
        // If the response is a string, it's an error message
        if (typeof response === 'string') {
          throw new Error(response);
        }
        return this.mapSurvey(response);
      }),
      catchError(this.handleError)
    );
  }

  deleteSurvey(id: number): Observable<void> {
    console.log('Deleting survey:', id);
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  getSurveyResults(id: number): Observable<SurveyResults> {
    return this.http.get<SurveyResults>(`${this.apiUrl}/${id}/results`);
  }

  exportSurveyToCsv(id: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/export/csv`, { responseType: 'blob' });
  }
} 