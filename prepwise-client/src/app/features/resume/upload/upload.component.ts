import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ResumeService } from '../../../core/services/resume.service';

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.css']
})
export class UploadComponent {
  targetRole: string = '';
  selectedFile: File | null = null;
  isDragging: boolean = false;
  isUploading: boolean = false;
  errorMessage: string = '';

  constructor(
    private resumeService: ResumeService,
    private router: Router
  ) {}

  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    this.isDragging = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragging = false;
    
    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      this.handleFile(event.dataTransfer.files[0]);
    }
  }

  onFileSelected(event: any) {
    if (event.target.files && event.target.files.length > 0) {
      this.handleFile(event.target.files[0]);
    }
  }

  handleFile(file: File) {
    this.errorMessage = '';
    
    if (file.type !== 'application/pdf') {
      this.errorMessage = 'Please upload a PDF file.';
      this.selectedFile = null;
      return;
    }
    
    if (file.size > 5 * 1024 * 1024) {
      this.errorMessage = 'File size must be under 5MB.';
      this.selectedFile = null;
      return;
    }

    this.selectedFile = file;
  }

  removeFile() {
    this.selectedFile = null;
  }

  async upload() {
    if (!this.selectedFile || !this.targetRole.trim()) {
      this.errorMessage = 'Please select a file and enter a target role.';
      return;
    }

    this.isUploading = true;
    this.errorMessage = '';

    this.resumeService.uploadAndAnalyze(this.selectedFile, this.targetRole).subscribe({
      next: (response) => {
        this.isUploading = false;
        // Navigate to report page
        this.router.navigate(['/resume/report', response.data.id]);
      },
      error: (err) => {
        this.isUploading = false;
        this.errorMessage = err.error?.message || 'An error occurred during analysis.';
      }
    });
  }
}
