import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, ResumeReport } from '../models/interfaces';

@Injectable({
  providedIn: 'root'
})
export class ResumeService {
  private apiUrl = `${environment.apiUrl}/resume`;

  constructor(private http: HttpClient) {}

  uploadAndAnalyze(file: File, targetRole: string): Observable<ApiResponse<ResumeReport>> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('targetRole', targetRole);

    return this.http.post<ApiResponse<ResumeReport>>(`${this.apiUrl}/upload-and-analyze`, formData);
  }

  getReports(): Observable<ApiResponse<ResumeReport[]>> {
    return this.http.get<ApiResponse<ResumeReport[]>>(`${this.apiUrl}/reports`);
  }

  getReportById(id: number): Observable<ApiResponse<ResumeReport>> {
    return this.http.get<ApiResponse<ResumeReport>>(`${this.apiUrl}/report/${id}`);
  }
}
