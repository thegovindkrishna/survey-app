import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { SurveyService } from '../../../shared/services/survey.service';
import { Survey, Question, QuestionType } from '../../../shared/models/survey.model';

@Component({
  selector: 'app-survey-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatCheckboxModule,
    MatIconModule
  ],
  templateUrl: './survey-form.component.html',
  styleUrls: ['./survey-form.component.css']
})
export class SurveyFormComponent implements OnInit {
  surveyForm: FormGroup;
  isEditMode = false;
  surveyId?: number;
  questionTypes = Object.values(QuestionType);

  constructor(
    private fb: FormBuilder,
    private surveyService: SurveyService,
    private route: ActivatedRoute,
    public router: Router
  ) {
    this.surveyForm = this.fb.group({
      title: ['', Validators.required],
      description: ['', Validators.required],
      startDate: [new Date(), Validators.required],
      endDate: [new Date(Date.now() + 30 * 24 * 60 * 60 * 1000), Validators.required],
      questions: this.fb.array([])
    });
  }

  ngOnInit(): void {
    this.surveyId = Number(this.route.snapshot.paramMap.get('id'));
    if (this.surveyId) {
      this.isEditMode = true;
      this.loadSurvey();
    }
  }

  private loadSurvey(): void {
    if (this.surveyId) {
      this.surveyService.getSurvey(this.surveyId).subscribe({
        next: (survey) => {
          this.surveyForm.patchValue({
            title: survey.title,
            description: survey.description,
            startDate: survey.startDate || new Date(),
            endDate: survey.endDate || new Date(Date.now() + 30 * 24 * 60 * 60 * 1000)
          });
          // TODO: Load questions
        },
        error: (error) => {
          console.error('Error loading survey:', error);
        }
      });
    }
  }

  onSubmit(): void {
    if (this.surveyForm.valid) {
      const survey: Survey = {
        ...this.surveyForm.value,
        questions: this.surveyForm.value.questions || []
      };
      
      if (this.isEditMode && this.surveyId) {
        this.surveyService.updateSurvey(this.surveyId, survey).subscribe({
          next: () => this.router.navigate(['/admin/dashboard']),
          error: (error) => console.error('Error updating survey:', error)
        });
      } else {
        this.surveyService.createSurvey(survey).subscribe({
          next: () => this.router.navigate(['/admin/dashboard']),
          error: (error) => console.error('Error creating survey:', error)
        });
      }
    }
  }
}
