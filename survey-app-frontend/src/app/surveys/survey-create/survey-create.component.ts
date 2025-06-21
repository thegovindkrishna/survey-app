import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SurveyService } from '../../shared/services/survey.service';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-survey-create',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './survey-create.component.html',
  styleUrls: ['./survey-create.component.scss']
})
export class SurveyCreateComponent {
  surveyForm: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private surveyService: SurveyService,
    private router: Router
  ) {
    this.surveyForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.maxLength(500)]],
      startDate: [new Date(), Validators.required],
      endDate: [new Date(Date.now() + 30 * 24 * 60 * 60 * 1000), Validators.required]
    });
  }

  onSubmit() {
    if (this.surveyForm.invalid) return;
    this.isLoading = true;
    this.errorMessage = '';
    
    const survey = {
      title: this.surveyForm.value.title,
      description: this.surveyForm.value.description,
      startDate: this.surveyForm.value.startDate,
      endDate: this.surveyForm.value.endDate,
      questions: [] // Empty questions array for basic survey creation
    };
    
    this.surveyService.createSurvey(survey).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/admin/dashboard']);
      },
      error: () => {
        this.errorMessage = 'Failed to create survey.';
        this.isLoading = false;
      }
    });
  }

  public goToDashboard() {
    this.router.navigate(['/admin/surveys']);
  }
}
