import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

export interface Question {
  QuestionText: string;
  type: string;
  required: boolean;
  options?: string[];
  maxRating?: number;
}

export interface Survey {
  id?: number;
  title: string;
  description: string;
  questions: Question[];
}

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

  createSurvey(survey: Survey): Observable<Survey> {
    console.log('Creating survey:', survey);
    return this.http.post<Survey>(this.apiUrl, survey).pipe(
      map(response => {
        console.log('Create response:', response);
        return {
          ...response,
          id: response.id
        };
      }),
      catchError(this.handleError)
    );
  }

  getSurveys(): Observable<Survey[]> {
    return this.http.get<Survey[]>(this.apiUrl).pipe(
      map(response => {
        console.log('Received surveys:', response);
        return response.map(survey => ({
          ...survey,
          id: survey.id
        }));
      }),
      catchError(this.handleError)
    );
  }

  getSurvey(id: number): Observable<Survey> {
    console.log('Getting survey:', id);
    return this.http.get<Survey>(`${this.apiUrl}/${id}`).pipe(
      map(response => {
        console.log('Received survey:', response);
        return {
          ...response,
          id: response.id
        };
      }),
      catchError(this.handleError)
    );
  }

  updateSurvey(id: number, survey: Survey): Observable<Survey> {
    console.log('Updating survey:', id, survey);
    return this.http.put<any>(`${this.apiUrl}/${id}`, { ...survey, id }).pipe(
      map(response => {
        console.log('Raw update response:', response);
        // If the response is a string, it's an error message
        if (typeof response === 'string') {
          throw new Error(response);
        }
        // Ensure we have a proper Survey object
        const updatedSurvey: Survey = {
          id: response.id,
          title: response.title,
          description: response.description,
          questions: response.questions || []
        };
        console.log('Processed survey response:', updatedSurvey);
        return updatedSurvey;
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
} 