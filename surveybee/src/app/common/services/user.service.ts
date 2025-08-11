import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { SurveyDto } from './survey.service';

export interface User {
  id: number;
  email: string;
  role: string;
}

export interface UserResponseDto {
  surveyId: number;
  surveyTitle: string;
  surveyDescription: string;
  submissionDate: string;
  responseId: number;
  responses: QuestionResponseDto[];
}

export interface QuestionResponseDto {
  questionId: number;
  response: string;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private baseUrl = 'https://localhost:7245/api/admin/users';
  private userApiUrl = 'https://localhost:7245/api/user';

  constructor(private http: HttpClient) {}

  // Admin methods
  getAllUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.baseUrl);
  }

  promoteToAdmin(id: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/promote`, {});
  }

  deleteUser(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }

  // User methods
  getAvailableSurveys(): Observable<SurveyDto[]> {
    return this.http.get<SurveyDto[]>(`${this.userApiUrl}/surveys`).pipe(
      catchError((error: any) => {
        console.error('Error fetching available surveys:', error);
        // Return empty array as fallback
        return of([]);
      })
    );
  }

  getUserResponses(): Observable<UserResponseDto[]> {
    return this.http.get<UserResponseDto[]>(`${this.userApiUrl}/responses`).pipe(
      catchError((error: any) => {
        console.error('Error fetching user responses:', error);
        // Return empty array as fallback
        return of([]);
      })
    );
  }

  getSurveyById(id: number): Observable<SurveyDto> {
    return this.http.get<SurveyDto>(`${this.userApiUrl}/surveys/${id}`).pipe(
      catchError((error: any) => {
        console.error('Error fetching survey by ID:', error);
        throw error; // Re-throw for proper error handling in component
      })
    );
  }
}
