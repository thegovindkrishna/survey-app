import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatRadioModule } from '@angular/material/radio';
import { MatSliderModule } from '@angular/material/slider';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';

import { UserService } from '../../shared/services/user.service';
import { Survey } from '../../shared/models/survey.model';
import { Question } from '../../shared/models/survey.model';

@Component({
  selector: 'app-survey-response',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatRadioModule,
    MatSliderModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatDividerModule
  ],
  template: `
    <div class="survey-response-container">
      <div *ngIf="isLoading" class="loading-container">
        <mat-spinner diameter="50"></mat-spinner>
        <p>Loading survey...</p>
      </div>

      <div *ngIf="errorMessage" class="error-container">
        <mat-icon>error</mat-icon>
        <h2>Error</h2>
        <p>{{ errorMessage }}</p>
        <button mat-raised-button color="primary" (click)="goBack()">Go Back</button>
      </div>

      <div *ngIf="!isLoading && !errorMessage && survey" class="survey-content">
        <mat-card class="survey-header">
          <mat-card-header>
            <mat-card-title>{{ survey.title }}</mat-card-title>
            <mat-card-subtitle>{{ survey.description }}</mat-card-subtitle>
          </mat-card-header>
        </mat-card>

        <form [formGroup]="responseForm" (ngSubmit)="onSubmit()" class="survey-form">
          <div *ngFor="let question of survey.questions; let i = index" class="question-container">
            <mat-card class="question-card">
              <mat-card-content>
                <h3 class="question-title">
                  {{ i + 1 }}. {{ question.text }}
                  <span *ngIf="question.required" class="required">*</span>
                </h3>

                <!-- Text Input -->
                <mat-form-field *ngIf="question.type === 'text'" appearance="outline" class="full-width">
                  <input matInput 
                         [formControlName]="'question_' + question.id"
                         [placeholder]="'Enter your answer'"
                         [required]="question.required">
                </mat-form-field>

                <!-- Number Input -->
                <mat-form-field *ngIf="question.type === 'number'" appearance="outline" class="full-width">
                  <input matInput 
                         type="number"
                         [formControlName]="'question_' + question.id"
                         [placeholder]="'Enter a number'"
                         [required]="question.required">
                </mat-form-field>

                <!-- Single Choice -->
                <mat-radio-group *ngIf="question.type === 'single_choice'" 
                                [formControlName]="'question_' + question.id"
                                [required]="question.required"
                                class="radio-group">
                  <mat-radio-button *ngFor="let option of question.options" 
                                   [value]="option"
                                   class="radio-button">
                    {{ option }}
                  </mat-radio-button>
                </mat-radio-group>

                <!-- Multiple Choice -->
                <div *ngIf="question.type === 'multiple_choice'" class="checkbox-group">
                  <mat-checkbox *ngFor="let option of question.options"
                               [formControlName]="'question_' + question.id + '_' + option"
                               class="checkbox-option">
                    {{ option }}
                  </mat-checkbox>
                </div>

                <!-- Date Input -->
                <mat-form-field *ngIf="question.type === 'date'" appearance="outline" class="full-width">
                  <input matInput 
                         [matDatepicker]="picker"
                         [formControlName]="'question_' + question.id"
                         [placeholder]="'Choose a date'"
                         [required]="question.required">
                  <mat-datepicker-toggle matSuffix [for]="picker"></mat-datepicker-toggle>
                  <mat-datepicker #picker></mat-datepicker>
                </mat-form-field>

                <!-- Rating -->
                <div *ngIf="question.type === 'rating'" class="rating-container">
                  <p class="rating-label">Rate from 1 to {{ question.maxRating || 10 }}</p>
                  <mat-slider [min]="1" 
                             [max]="question.maxRating || 10" 
                             [step]="1"
                             discrete
                             class="rating-slider">
                    <input matSliderThumb [formControlName]="'question_' + question.id">
                  </mat-slider>
                </div>
              </mat-card-content>
            </mat-card>
          </div>

          <div class="form-actions">
            <button mat-button type="button" (click)="goBack()">Cancel</button>
            <button mat-raised-button 
                    color="primary" 
                    type="submit" 
                    [disabled]="isSubmitting || !responseForm.valid">
              <mat-spinner diameter="20" *ngIf="isSubmitting"></mat-spinner>
              <span *ngIf="!isSubmitting">Submit Response</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .survey-response-container {
      max-width: 800px;
      margin: 0 auto;
      padding: 20px;
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 60px 20px;
      gap: 16px;
    }

    .error-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 60px 20px;
      text-align: center;
      gap: 16px;
    }

    .error-container mat-icon {
      font-size: 4rem;
      width: 4rem;
      height: 4rem;
      color: #f44336;
    }

    .survey-header {
      margin-bottom: 24px;
    }

    .survey-form {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .question-container {
      margin-bottom: 16px;
    }

    .question-card {
      border: 1px solid #e0e0e0;
    }

    .question-title {
      margin: 0 0 16px 0;
      color: #333;
      font-size: 1.1rem;
    }

    .required {
      color: #f44336;
      margin-left: 4px;
    }

    .full-width {
      width: 100%;
    }

    .radio-group {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .radio-button {
      margin-bottom: 8px;
    }

    .checkbox-group {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .checkbox-option {
      margin-bottom: 8px;
    }

    .rating-container {
      display: flex;
      flex-direction: column;
      gap: 8px;
      align-items: center;
    }

    .rating-label {
      margin: 0;
      color: #666;
    }

    .rating-slider {
      width: 100%;
      max-width: 300px;
    }

    .form-actions {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: 32px;
      padding-top: 24px;
      border-top: 1px solid #e0e0e0;
    }

    @media (max-width: 768px) {
      .survey-response-container {
        padding: 16px;
      }

      .form-actions {
        flex-direction: column;
        gap: 16px;
      }

      .form-actions button {
        width: 100%;
      }
    }
  `]
})
export class SurveyResponseComponent implements OnInit {
  survey: Survey | null = null;
  responseForm: FormGroup;
  isLoading = false;
  isSubmitting = false;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService,
    private formBuilder: FormBuilder,
    private snackBar: MatSnackBar
  ) {
    this.responseForm = this.formBuilder.group({});
  }

  ngOnInit() {
    const surveyId = this.route.snapshot.paramMap.get('id');
    if (surveyId) {
      this.loadSurvey(+surveyId);
    } else {
      this.errorMessage = 'Survey ID not provided';
    }
  }

  loadSurvey(surveyId: number) {
    this.isLoading = true;
    this.errorMessage = '';

    this.userService.getSurvey(surveyId).subscribe({
      next: (survey) => {
        this.survey = survey;
        this.buildForm();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading survey:', error);
        this.errorMessage = error.error?.message || 'Failed to load survey';
        this.isLoading = false;
      }
    });
  }

  buildForm() {
    if (!this.survey?.questions) return;

    const formControls: any = {};

    this.survey.questions.forEach(question => {
      if (question.type === 'multiple_choice' && question.options) {
        // For multiple choice questions, create individual checkboxes
        question.options.forEach(option => {
          formControls[`question_${question.id}_${option}`] = [false];
        });
      } else {
        // For other question types
        const isRating = question.type === 'rating';
        const initialValue = isRating ? 1 : ''; // Default rating to 1

        if (question.required) {
          formControls[`question_${question.id}`] = [initialValue, Validators.required];
        } else {
          formControls[`question_${question.id}`] = [initialValue];
        }
      }
    });

    this.responseForm = this.formBuilder.group(formControls);
  }

  onSubmit() {
    if (this.responseForm.invalid || !this.survey) {
      this.snackBar.open('Please fill in all required fields', 'Close', { duration: 3000 });
      return;
    }

    this.isSubmitting = true;

    // Prepare response data
    const responses: any[] = [];
    this.survey.questions?.forEach(question => {
      let response = '';

      if (question.type === 'multiple_choice' && question.options) {
        // Handle multiple choice - collect all selected options
        const selectedOptions = question.options.filter(option => 
          this.responseForm.get(`question_${question.id}_${option}`)?.value
        );
        response = selectedOptions.join(', ');
      } else {
        // Handle other question types
        const control = this.responseForm.get(`question_${question.id}`);
        if (control?.value) {
          if (question.type === 'date') {
            response = new Date(control.value).toISOString();
          } else {
            response = control.value.toString();
          }
        }
      }

      if (response) {
        responses.push({
          questionId: question.id,
          response: response
        });
      }
    });

    const surveyResponse = {
      responses: responses
    };

    this.userService.submitResponse(this.survey.id!, surveyResponse).subscribe({
      next: () => {
        this.snackBar.open('Survey submitted successfully!', 'Close', { duration: 3000 });
        this.router.navigate(['/user/dashboard']);
      },
      error: (error) => {
        console.error('Error submitting survey:', error);
        this.snackBar.open(error.error?.message || 'Failed to submit survey', 'Close', { duration: 5000 });
        this.isSubmitting = false;
      }
    });
  }

  goBack() {
    this.router.navigate(['/user/dashboard']);
  }
} 