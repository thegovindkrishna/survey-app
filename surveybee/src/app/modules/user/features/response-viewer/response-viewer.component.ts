import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ResponseViewerData {
  surveyTitle: string;
  surveyDescription: string;
  submissionDate: string;
  responses: Array<{
    questionId: number;
    response: string;
    questionText?: string;
    questionType?: string;
  }>;
}

@Component({
  selector: 'app-response-viewer',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="response-viewer-modal">
      <!-- Header -->
      <div class="modal-header">
        <div class="header-content">
          <div class="header-icon">📋</div>
          <div class="header-text">
            <h2>{{ data.surveyTitle }}</h2>
            <p>{{ data.surveyDescription }}</p>
          </div>
        </div>
        
      </div>

      <!-- Submission Info -->
      <div class="submission-info">
        <div class="info-card">
          <div class="info-icon">📅</div>
          <div class="info-content">
            <span class="info-label">Submitted on</span>
            <span class="info-value">{{ data.submissionDate | date:'full' }}</span>
          </div>
        </div>
        <div class="info-card">
          <div class="info-icon">📝</div>
          <div class="info-content">
            <span class="info-label">Total Responses</span>
            <span class="info-value">{{ data.responses.length }}</span>
          </div>
        </div>
      </div>

      <!-- Responses Content -->
      <div class="modal-content">
        <div class="responses-container">
          <div class="response-item" *ngFor="let response of data.responses; let i = index">
            <div class="question-header">
              <span class="question-number">Q{{ i + 1 }}</span>
              <span class="question-text">{{ response.questionText || 'Question ' + (i + 1) }}</span>
            </div>
            <div class="response-content">
              <div class="response-value" [ngClass]="getResponseClass(response.questionType)">
                {{ formatResponse(response.response) }}
              </div>
            </div>
          </div>

          <div class="empty-state" *ngIf="data.responses.length === 0">
            <div class="empty-icon">📭</div>
            <h3>No Responses Found</h3>
            <p>This survey submission contains no responses.</p>
          </div>
        </div>
      </div>

      <!-- Footer -->
      <div class="modal-footer">
        <button class="action-btn secondary" (click)="close()">
          
          Close
        </button>
        <button class="action-btn primary" (click)="exportResponse()">
          Export Response
        </button>
      </div>
    </div>
  `,
  styleUrls: ['./response-viewer.component.css']
})
export class ResponseViewerComponent {
  constructor(
    public dialogRef: MatDialogRef<ResponseViewerComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ResponseViewerData
  ) {}

  close(): void {
    this.dialogRef.close();
  }

  formatResponse(response: string): string {
    if (!response) return 'No response provided';
    
    // Handle comma-separated multiple choice responses
    if (response.includes(',')) {
      return response.split(',').map(r => r.trim()).join(', ');
    }
    
    return response;
  }

  getResponseClass(questionType?: string): string {
    switch (questionType?.toLowerCase()) {
      case 'rating':
        return 'response-rating';
      case 'checkbox':
        return 'response-multiple';
      case 'radio':
        return 'response-single';
      case 'textarea':
        return 'response-long';
      default:
        return 'response-text';
    }
  }

  exportResponse(): void {
    const exportData = {
      survey: this.data.surveyTitle,
      description: this.data.surveyDescription,
      submittedOn: this.data.submissionDate,
      responses: this.data.responses.map((r, i) => ({
        question: r.questionText || `Question ${i + 1}`,
        answer: r.response
      }))
    };

    const dataStr = JSON.stringify(exportData, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(dataBlob);
    
    const link = document.createElement('a');
    link.href = url;
    link.download = `${this.data.surveyTitle.replace(/[^a-z0-9]/gi, '_').toLowerCase()}_response.json`;
    link.click();
    
    URL.revokeObjectURL(url);
  }
}
