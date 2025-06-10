import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { CdkDragDrop, moveItemInArray, DragDropModule } from '@angular/cdk/drag-drop';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSliderModule } from '@angular/material/slider';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatRadioModule } from '@angular/material/radio';
import { MatDividerModule } from '@angular/material/divider';
import { SurveyService } from '../../shared/services/survey.service';
import { Survey, Question, QuestionType } from '../../shared/models/survey.model';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-survey-builder',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    DragDropModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatSnackBarModule,
    MatSliderModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatRadioModule,
    MatDividerModule
  ],
  template: `
    <div class="survey-builder-root">
      <h2>{{ isEditing ? 'Edit Survey' : 'Create Survey' }}</h2>
      <form [formGroup]="surveyForm" (ngSubmit)="onSubmit()">
        <mat-form-field appearance="outline" class="survey-title-field">
          <mat-label>Survey Title</mat-label>
          <input matInput formControlName="title" required>
        </mat-form-field>
        <mat-form-field appearance="outline" class="survey-desc-field">
          <mat-label>Description</mat-label>
          <textarea matInput formControlName="description" rows="2"></textarea>
        </mat-form-field>

        <div formArrayName="questions">
          <div *ngFor="let q of questions.controls; let i = index" [formGroupName]="i" class="question-card">
            <mat-card>
              <div class="question-header">
                <mat-form-field appearance="outline" class="question-type-field">
                  <mat-label>Type</mat-label>
                  <mat-select formControlName="type" (selectionChange)="onTypeChange(i)">
                    <mat-option [value]="QuestionType.Text"><mat-icon>short_text</mat-icon> Short Answer</mat-option>
                    <mat-option [value]="QuestionType.SingleChoice"><mat-icon>radio_button_checked</mat-icon> Multiple Choice</mat-option>
                    <mat-option [value]="QuestionType.MultipleChoice"><mat-icon>check_box</mat-icon> Checkboxes</mat-option>
                    <mat-option [value]="QuestionType.Rating"><mat-icon>star</mat-icon> Rating</mat-option>
                    <mat-option [value]="QuestionType.Date"><mat-icon>event</mat-icon> Date</mat-option>
                  </mat-select>
                </mat-form-field>
                <button mat-icon-button color="warn" (click)="removeQuestion(i)" *ngIf="questions.length > 1">
                  <mat-icon>delete</mat-icon>
                </button>
              </div>
              <mat-form-field appearance="outline" class="question-text-field">
                <mat-label>Question</mat-label>
                <input matInput formControlName="text" required>
              </mat-form-field>
              <mat-checkbox formControlName="required">Required</mat-checkbox>

              <!-- Dynamic fields -->
              <ng-container [ngSwitch]="q.get('type')?.value">
                <!-- Multiple Choice -->
                <div *ngSwitchCase="QuestionType.SingleChoice" formArrayName="options" class="options-list">
                  <div *ngFor="let opt of getOptions(i).controls; let j = index" [formGroupName]="j" class="option-row">
                    <mat-form-field appearance="outline" class="option-field">
                      <mat-label>Option {{j + 1}}</mat-label>
                      <input matInput [formControlName]="'text'" required>
                    </mat-form-field>
                    <button mat-icon-button color="warn" (click)="removeOption(i, j)" *ngIf="getOptions(i).length > 1">
                      <mat-icon>delete</mat-icon>
                    </button>
                  </div>
                  <button mat-button color="primary" (click)="addOption(i)"><mat-icon>add</mat-icon> Add Option</button>
                </div>
                <!-- Checkbox -->
                <div *ngSwitchCase="QuestionType.MultipleChoice" formArrayName="options" class="options-list">
                  <div *ngFor="let opt of getOptions(i).controls; let j = index" [formGroupName]="j" class="option-row">
                    <mat-form-field appearance="outline" class="option-field">
                      <mat-label>Option {{j + 1}}</mat-label>
                      <input matInput [formControlName]="'text'" required>
                    </mat-form-field>
                    <button mat-icon-button color="warn" (click)="removeOption(i, j)" *ngIf="getOptions(i).length > 1">
                      <mat-icon>delete</mat-icon>
                    </button>
                  </div>
                  <button mat-button color="primary" (click)="addOption(i)"><mat-icon>add</mat-icon> Add Option</button>
                </div>
                <!-- Rating -->
                <div *ngSwitchCase="QuestionType.Rating" class="rating-row">
                  <span>Rating:</span>
                  <ng-container *ngFor="let star of [1,2,3,4,5]">
                    <mat-icon [class.filled]="star <= (q.get('maxRating')?.value || 5)">star</mat-icon>
                  </ng-container>
                  <mat-form-field appearance="outline" class="rating-field">
                    <mat-label>Max Stars</mat-label>
                    <input matInput type="number" formControlName="maxRating" min="1" max="10">
                  </mat-form-field>
                </div>
                <!-- Date -->
                <div *ngSwitchCase="QuestionType.Date" class="date-row">
                  <mat-form-field appearance="outline">
                    <mat-label>Date</mat-label>
                    <input matInput [matDatepicker]="picker">
                    <mat-datepicker-toggle matSuffix [for]="picker"></mat-datepicker-toggle>
                    <mat-datepicker #picker></mat-datepicker>
                  </mat-form-field>
                </div>
              </ng-container>
            </mat-card>
          </div>
        </div>
        <button mat-stroked-button color="primary" type="button" (click)="addQuestion()" class="add-question-btn">
          <mat-icon>add</mat-icon> Add Question
        </button>
        <button mat-raised-button color="accent" type="submit" [disabled]="!surveyForm.valid" class="save-btn">
          Save Survey
        </button>
      </form>
    </div>
  `,
  styles: [`
    .survey-builder-root { max-width: 700px; margin: 0 auto; padding: 32px 0; }
    .survey-title-field, .survey-desc-field { width: 100%; margin-bottom: 16px; }
    .question-card { margin-bottom: 24px; }
    .question-header { display: flex; align-items: center; gap: 16px; }
    .question-type-field { width: 200px; }
    .question-text-field { width: 100%; margin-bottom: 8px; }
    .options-list { margin: 12px 0 0 0; }
    .option-row { display: flex; align-items: center; gap: 8px; margin-bottom: 8px; }
    .option-field { width: 300px; }
    .add-question-btn { margin: 24px 0 0 0; display: block; }
    .save-btn { margin: 32px 0 0 0; float: right; }
    .rating-row { display: flex; align-items: center; gap: 8px; margin: 12px 0; }
    .rating-field { width: 100px; }
    .date-row { margin: 12px 0; }
    mat-card { padding: 24px; }
    mat-icon.filled { color: #FFD600; }
  `]
})
export class SurveyBuilderComponent implements OnInit {
  surveyForm: FormGroup;
  isEditing = false;
  surveyId: number | null = null;
  QuestionType = QuestionType; // Make enum available in template

  get questions() {
    return this.surveyForm.get('questions') as FormArray;
  }

  constructor(
    private fb: FormBuilder,
    private surveyService: SurveyService,
    private snackBar: MatSnackBar,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.surveyForm = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      questions: this.fb.array([])
    });
  }

  ngOnInit() {
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.surveyId = +params['id'];
        this.isEditing = true;
        this.loadSurvey(this.surveyId);
      } else {
        this.addQuestion(); // Add initial question for new survey
      }
    });
  }

  loadSurvey(id: number) {
    console.log('Loading survey with ID:', id);
    this.surveyService.getSurvey(id).subscribe({
      next: (survey) => {
        console.log('Received survey data:', survey);
        this.surveyForm.patchValue({
          title: survey.title,
          description: survey.description
        });

        // Clear existing questions
        while (this.questions.length) {
          this.questions.removeAt(0);
        }

        // Add questions from the survey
        if (survey.questions && survey.questions.length > 0) {
          console.log('Processing questions:', survey.questions);
          survey.questions.forEach((q, index) => {
            console.log(`Processing question ${index}:`, q);
            const questionGroup = this.fb.group({
              text: [q.text, Validators.required],
              type: [q.type, Validators.required],
              required: [q.required],
              options: this.fb.array([]),
              maxRating: [q.maxRating || 5]
            });

            if (q.type === QuestionType.SingleChoice || q.type === QuestionType.MultipleChoice) {
              console.log(`Adding options for question ${index}:`, q.options);
              q.options?.forEach(opt => {
                (questionGroup.get('options') as FormArray).push(
                  this.fb.group({ text: [opt, Validators.required] })
                );
              });
            }

            this.questions.push(questionGroup);
          });
        } else {
          console.log('No questions found in survey');
        }
      },
      error: (error) => {
        console.error('Error loading survey:', error);
        this.snackBar.open('Error loading survey: ' + error.message, 'Close', { duration: 5000 });
        this.router.navigate(['/admin/dashboard']);
      }
    });
  }

  getOptions(index: number): FormArray {
    return this.questions.at(index).get('options') as FormArray;
  }

  addQuestion() {
    this.questions.push(this.fb.group({
      text: ['', Validators.required],
      type: [QuestionType.Text, Validators.required],
      required: [false],
      options: this.fb.array([this.fb.group({ text: ['', Validators.required] })]),
      maxRating: [5]
    }));
  }

  removeQuestion(index: number) {
    this.questions.removeAt(index);
  }

  addOption(questionIndex: number) {
    this.getOptions(questionIndex).push(
      this.fb.group({ text: ['', Validators.required] })
    );
  }

  removeOption(questionIndex: number, optionIndex: number) {
    this.getOptions(questionIndex).removeAt(optionIndex);
  }

  onTypeChange(qIndex: number) {
    const q = this.questions.at(qIndex);
    const type = q.get('type')?.value;
    // Reset options for new type
    if (type === QuestionType.SingleChoice || type === QuestionType.MultipleChoice) {
      if (this.getOptions(qIndex).length === 0) {
        this.addOption(qIndex);
        this.addOption(qIndex);
      }
    } else {
      // Remove all options for other types
      while (this.getOptions(qIndex).length > 0) {
        this.getOptions(qIndex).removeAt(0);
      }
    }
    if (type === QuestionType.Rating) {
      q.get('maxRating')?.setValue(5);
    }
  }

  onSubmit() {
    if (this.surveyForm.valid) {
      const survey: Survey = {
        title: this.surveyForm.value.title,
        description: this.surveyForm.value.description,
        questions: this.surveyForm.value.questions.map((q: any) => {
          const question: Question = {
            text: q.text,
            type: q.type,
            required: q.required
          };
          if (q.type === QuestionType.SingleChoice || q.type === QuestionType.MultipleChoice) {
            question.options = q.options.map((o: any) => o.text);
          } else if (q.type === QuestionType.Rating) {
            question.maxRating = q.maxRating;
          }
          return question;
        })
      };

      console.log('Submitting survey:', survey);

      const request = this.isEditing && this.surveyId
        ? this.surveyService.updateSurvey(this.surveyId, survey)
        : this.surveyService.createSurvey(survey);

      request.subscribe({
        next: (response) => {
          console.log('Survey response:', response);
          if (response) {
            this.snackBar.open(
              `Survey ${this.isEditing ? 'updated' : 'created'} successfully!`,
              'Close',
              { duration: 3000 }
            );
            this.router.navigate(['/admin/dashboard']);
          } else {
            throw new Error('No response received from server');
          }
        },
        error: (error) => {
          console.error('Error saving survey:', error);
          let errorMessage = 'An error occurred while saving the survey.';
          if (error.error?.message) {
            errorMessage = error.error.message;
          } else if (error.message) {
            errorMessage = error.message;
          }
          this.snackBar.open(
            `Error ${this.isEditing ? 'updating' : 'creating'} survey: ${errorMessage}`,
            'Close',
            { duration: 5000 }
          );
        }
      });
    } else {
      console.log('Form is invalid:', this.surveyForm.errors);
      this.snackBar.open('Please fill in all required fields', 'Close', { duration: 3000 });
    }
  }
} 