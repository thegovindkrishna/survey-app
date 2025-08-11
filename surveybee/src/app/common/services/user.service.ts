import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface User {
  id: number;
  email: string;
  role: string;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private baseUrl = 'https://localhost:7245/api/admin/users';

  constructor(private http: HttpClient) {}

  getAllUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.baseUrl);
  }

  promoteToAdmin(id: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/promote`, {});
  }

  deleteUser(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }
}
