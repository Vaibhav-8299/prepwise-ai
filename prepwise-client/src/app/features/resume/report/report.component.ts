import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ResumeService } from '../../../core/services/resume.service';
import { ResumeReport } from '../../../core/models/interfaces';

@Component({
  selector: 'app-report',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.css']
})
export class ReportComponent implements OnInit {
  report: ResumeReport | null = null;
  isLoading: boolean = true;
  errorMessage: string = '';

  constructor(
    private route: ActivatedRoute,
    private resumeService: ResumeService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.loadReport(parseInt(idParam, 10));
    } else {
      this.errorMessage = 'No report ID provided.';
      this.isLoading = false;
    }
  }

  loadReport(id: number) {
    this.resumeService.getReportById(id).subscribe({
      next: (response) => {
        this.report = response.data;
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to load report.';
        this.isLoading = false;
      }
    });
  }

  getScoreColor(): string {
    if (!this.report) return '#8888a8';
    if (this.report.score >= 80) return '#4CAF50';
    if (this.report.score >= 60) return '#FF9800';
    return '#f44336';
  }
}
