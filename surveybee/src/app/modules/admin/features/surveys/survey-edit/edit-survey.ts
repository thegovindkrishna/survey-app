import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { SurveyService, SurveyDto, SurveyCreateDto } from '../../../../../common/services/survey.service';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-edit-survey',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './edit-survey.html',
  styleUrls: ['./edit-survey.css']
})
export class EditSurveyComponent implements OnInit {
  surveyId!: number;
  survey: SurveyDto | null = null;
  form!: FormGroup;
  isLoading = true;
  error: string | null = null;
  success: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private surveyService: SurveyService,
    private fb: FormBuilder,
    public router: Router // <-- public
  ) {}

  ngOnInit() {
    this.surveyId = Number(this.route.snapshot.paramMap.get('id'));
    this.fetchSurvey();
  }

  fetchSurvey() {
    this.isLoading = true;
    this.surveyService.getSurveyById(this.surveyId).subscribe({
      next: (data) => {
        // Accept both { result: SurveyDto } and SurveyDto directly
        const surveyData: any = data as any;
        this.survey = surveyData.result ? surveyData.result : surveyData;
        this.initForm();
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Failed to load survey.';
        this.isLoading = false;
      }
    });
  }

  initForm() {
    const questions = Array.isArray(this.survey?.questions) ? this.survey!.questions : [];
    // Format dates for input[type=date]
    const formatDate = (date: string | Date | undefined) => {
      if (!date) return '';
      const d = new Date(date);
      // Pad month and day
      const mm = String(d.getMonth() + 1).padStart(2, '0');
      const dd = String(d.getDate()).padStart(2, '0');
      return `${d.getFullYear()}-${mm}-${dd}`;
    };
    this.form = this.fb.group({
      title: [this.survey?.title, Validators.required],
      description: [this.survey?.description],
      startDate: [formatDate(this.survey?.startDate), Validators.required],
      endDate: [formatDate(this.survey?.endDate), Validators.required],
      questions: this.fb.array(questions.map(q => this.fb.group({
        id: [q.id],
        questionText: [q.questionText, Validators.required],
        type: [q.type, Validators.required],
        required: [q.required],
        options: [Array.isArray(q.options) ? q.options : []],
        maxRating: [q.type === 'rating' ? q.maxRating : null]
      })) )
    });
  }

  // Fix: Strongly type questions as FormArray<FormGroup>
  get questions(): FormArray<FormGroup> {
    return this.form.get('questions') as FormArray<FormGroup>;
  }

  addQuestion() {
    this.questions.push(this.fb.group({
      id: [null],
      questionText: ['', Validators.required],
      type: ['', Validators.required],
      required: [false],
      options: [[]],
      maxRating: [null]
    }));
  }

  removeQuestion(i: number) {
    this.questions.removeAt(i);
  }

  // Option management for multiple-choice questions
  addOption(qIndex: number) {
    const q = this.questions.at(qIndex);
    const options = q.get('options');
    if (options && Array.isArray(options.value)) {
      options.setValue([...options.value, '']);
      options.markAsDirty();
    } else if (options) {
      options.setValue(['']);
      options.markAsDirty();
    }
  }

  removeOption(qIndex: number, oIndex: number) {
    const q = this.questions.at(qIndex);
    const options = q.get('options');
    if (options && Array.isArray(options.value)) {
      options.setValue(options.value.filter((_: any, idx: number) => idx !== oIndex));
      options.markAsDirty();
    }
  }

  onOptionChange(qIndex: number, oIndex: number, value: string) {
    const q = this.questions.at(qIndex);
    const options = q.get('options');
    if (options && Array.isArray(options.value)) {
      const newOptions = [...options.value];
      newOptions[oIndex] = value;
      options.setValue(newOptions);
      options.markAsDirty();
    }
  }

  onSubmit() {
    if (this.form.invalid) return;
    // Map form value to SurveyUpdateDto shape
    const formValue = this.form.value;
    const updateDto = {
      title: formValue.title,
      description: formValue.description,
      startDate: formValue.startDate,
      endDate: formValue.endDate,
      questions: formValue.questions.map((q: any) => ({
        questionText: q.questionText,
        type: q.type,
        required: q.required,
        options: Array.isArray(q.options) ? q.options : [],
        maxRating: q.type === 'rating' ? q.maxRating : null
      }))
    };
    this.surveyService.updateSurvey(this.surveyId, updateDto).subscribe({
      next: () => {
        this.success = 'Survey updated successfully!';
        setTimeout(() => this.router.navigate(['/admin/surveys']), 1200);
      },
      error: (err) => {
        this.error = 'Failed to update survey.';
      }
    });
  }
}
