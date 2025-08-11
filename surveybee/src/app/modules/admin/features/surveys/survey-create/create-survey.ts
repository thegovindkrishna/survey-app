import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { SurveyService, SurveyCreateDto } from '../../../../../common/services/survey.service';
import { ToastService } from '../../../../../common/services/toast.service';

@Component({
  selector: 'app-create-survey',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-survey.html',
  styleUrls: ['./create-survey.css']
})
export class CreateSurveyComponent {
  surveyForm: FormGroup;
  isSubmitting = false;

  constructor(
    private router: Router,
    private fb: FormBuilder,
    private surveyService: SurveyService,
    private toastService: ToastService
  ) {
    this.surveyForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(1000)]],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      questions: this.fb.array([])
    });
    if (this.questions.length === 0) this.addQuestion();
  }

  get questions(): FormArray {
    return this.surveyForm.get('questions') as FormArray;
  }

  addQuestion() {
    this.questions.push(this.fb.group({
      questionText: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(500)]],
      type: ['text', Validators.required],
      required: [false],
      options: this.fb.array([]),
      maxRating: []
    }));
  }

  removeQuestion(index: number) {
    this.questions.removeAt(index);
  }

  addOption(qIndex: number) {
    const options = (this.questions.at(qIndex).get('options') as FormArray);
    options.push(new FormControl(''));
  }

  removeOption(qIndex: number, oIndex: number) {
    const options = (this.questions.at(qIndex).get('options') as FormArray);
    options.removeAt(oIndex);
  }

  getQuestionFormGroup(index: number) {
    return this.questions.at(index) as FormGroup;
  }
  getOptionsFormArray(qIndex: number) {
    return (this.questions.at(qIndex).get('options') as FormArray);
  }
  getOptionFormControl(qIndex: number, oIndex: number) {
    return this.getOptionsFormArray(qIndex).at(oIndex) as FormControl;
  }

  submitSurvey() {
    if (this.surveyForm.invalid) {
      this.surveyForm.markAllAsTouched();
      return;
    }
    this.isSubmitting = true;
    // Prepare survey object for backend
    const rawSurvey = this.surveyForm.value;
    // Convert dates to ISO string (date only)
    const startDate = rawSurvey.startDate ? new Date(rawSurvey.startDate).toISOString() : '';
    const endDate = rawSurvey.endDate ? new Date(rawSurvey.endDate).toISOString() : '';
    // Clean up questions array
    const questions = rawSurvey.questions.map((q: any) => {
      const cleaned: any = {
        questionText: q.questionText,
        type: q.type,
        required: q.required
      };
      if (q.type === 'multiple-choice') {
        cleaned.options = (q.options || []).filter((opt: string) => !!opt && opt.trim() !== '');
      } else {
        cleaned.options = null;
      }
      if (q.type === 'rating') {
        cleaned.maxRating = q.maxRating ? Number(q.maxRating) : null;
      } else {
        cleaned.maxRating = null;
      }
      return cleaned;
    });
    const survey: SurveyCreateDto = {
      title: rawSurvey.title,
      description: rawSurvey.description,
      startDate: startDate,
      endDate: endDate,
      questions: questions
    };
    this.surveyService.createSurvey(survey).subscribe({
      next: () => {
        this.toastService.success('Survey created successfully!', 'Success');
        this.router.navigate(['/admin']);
        this.isSubmitting = false;
      },
      error: (err) => {
        this.toastService.error('Failed to create survey. ' + (err?.error?.message || ''), 'Error');
        this.isSubmitting = false;
      }
    });
  }

  cancel() {
    this.router.navigate(['/admin']);
  }
}
