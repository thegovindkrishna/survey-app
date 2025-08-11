import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { SurveyService, SurveyDto } from '../../../../common/services/survey.service';
import { UserService } from '../../../../common/services/user.service';
import { ResponseService, SubmitResponseDto } from '../../../../common/services/response.service';
import { DashboardRefreshService } from '../../../../common/services/dashboard-refresh.service';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-attend-survey',
  standalone: true,
  imports: [CommonModule, MatProgressSpinnerModule, MatIconModule, MatButtonModule, FormsModule],
  templateUrl: './attend-survey.component.html',
  styleUrls: ['./attend-survey.component.css']
})
export class AttendSurveyComponent implements OnInit {
  surveyId!: number;
  survey?: SurveyDto;
  isLoading = true;
  isSubmitting = false;
  errorMessage = '';
  answers: any[] = [];

  constructor(
    private route: ActivatedRoute, 
    private router: Router,
    private surveyService: SurveyService,
    private userService: UserService,
    private responseService: ResponseService,
    private dashboardRefreshService: DashboardRefreshService
  ) {}

  ngOnInit() {
    this.surveyId = Number(this.route.snapshot.paramMap.get('id'));
    console.log('Survey ID from route:', this.surveyId);
    if (this.surveyId) {
      // Use SurveyService instead of UserService to get complete survey data with questions
      this.surveyService.getSurveyById(this.surveyId).subscribe({
        next: (response: any) => {
          console.log('Raw survey response:', response);
          console.log('Response type:', typeof response);
          console.log('Response keys:', Object.keys(response || {}));
          
          // Handle different response structures from backend
          let survey = null;
          if (response && response.result) {
            survey = response.result;
            console.log('Using response.result:', survey);
          } else if (response && response.data) {
            survey = response.data;
            console.log('Using response.data:', survey);
          } else if (response && typeof response === 'object') {
            survey = response;
            console.log('Using direct response:', survey);
          }
          
          if (survey) {
            this.survey = survey;
            console.log('Final survey object:', this.survey);
            console.log('Survey properties:', Object.keys(this.survey || {}));
            console.log('Survey questions:', this.survey?.questions);
            console.log('Questions type:', typeof this.survey?.questions);
            console.log('Questions is array:', Array.isArray(this.survey?.questions));
            
            // Initialize answers array - check for both camelCase and PascalCase properties
            const questions = (this.survey?.questions || (this.survey as any)?.Questions || []) as any[];
            console.log('Extracted questions:', questions);
            console.log('Questions count:', questions.length);
            
            if (Array.isArray(questions) && questions.length > 0) {
              this.answers = new Array(questions.length);
              console.log('Initialized answers array with length:', this.answers.length);
              console.log('Questions details:', questions.map((q: any, i: number) => ({
                index: i,
                id: q.id || q.Id,
                questionText: q.questionText || q.QuestionText,
                type: q.type || q.Type,
                required: q.required || q.Required,
                options: q.options || q.Options
              })));
              
              // Normalize questions to ensure consistent property names
              if (this.survey) {
                this.survey.questions = questions.map((q: any) => ({
                  id: q.id || q.Id,
                  questionText: q.questionText || q.QuestionText,
                  type: q.type || q.Type,
                  required: q.required || q.Required,
                  options: q.options || q.Options,
                  maxRating: q.maxRating || q.MaxRating
                }));
                
                console.log('Normalized questions:', this.survey.questions);
              }
            } else {
              console.warn('Survey questions validation failed:', {
                hasQuestions: !!(this.survey?.questions),
                hasPascalQuestions: !!((this.survey as any)?.Questions),
                questionsLength: questions.length,
                isArray: Array.isArray(questions),
                questionsValue: questions
              });
              this.errorMessage = 'This survey has no questions or questions could not be loaded. Please contact support.';
            }
          } else {
            console.error('No valid survey data found in response');
            this.errorMessage = 'Invalid survey data received from server.';
          }
          
          this.isLoading = false;
        },
        error: (err) => {
          console.error('Failed to load survey:', err);
          console.error('Error details:', {
            status: err.status,
            statusText: err.statusText,
            message: err.message,
            error: err.error
          });
          this.errorMessage = `Failed to load survey: ${err.message || 'Unknown error'}. Please try again or contact support.`;
          this.isLoading = false;
        }
      });
    } else {
      this.errorMessage = 'Invalid survey ID.';
      this.isLoading = false;
    }
  }

  onCheckboxChange(questionIndex: number, option: string, event: any) {
    if (!this.answers[questionIndex]) {
      this.answers[questionIndex] = [];
    }
    
    if (event.target.checked) {
      this.answers[questionIndex].push(option);
    } else {
      const index = this.answers[questionIndex].indexOf(option);
      if (index > -1) {
        this.answers[questionIndex].splice(index, 1);
      }
    }
  }

  onSubmit() {
    if (!this.survey?.questions || !Array.isArray(this.survey.questions) || this.survey.questions.length === 0) {
      console.error('Survey validation failed:', {
        survey: this.survey,
        questions: this.survey?.questions,
        questionsLength: this.survey?.questions?.length,
        isArray: Array.isArray(this.survey?.questions)
      });
      alert('No questions found to submit. Please refresh the page and try again.');
      return;
    }

    this.isSubmitting = true;
    console.log('Survey answers:', this.answers);
    console.log('Survey ID:', this.surveyId);
    console.log('Survey questions:', this.survey.questions);
    
    // Validate that we have answers for required questions
    const incompleteQuestions = [];
    for (let i = 0; i < this.survey.questions.length; i++) {
      const question = this.survey.questions[i];
      const answer = this.answers[i];
      
      if (question.required && (!answer || (Array.isArray(answer) && answer.length === 0) || answer.toString().trim() === '')) {
        incompleteQuestions.push(question.questionText);
      }
    }
    
    if (incompleteQuestions.length > 0) {
      alert(`Please answer the following required questions:\n- ${incompleteQuestions.join('\n- ')}`);
      this.isSubmitting = false;
      return;
    }
    
    // Create response object matching backend DTO
    const submitResponseDto: SubmitResponseDto = {
      responses: this.survey.questions.map((question, index) => ({
        questionId: question.id,
        response: Array.isArray(this.answers[index]) 
          ? this.answers[index].join(',') 
          : (this.answers[index] || '').toString()
      }))
    };
    
    console.log('Submitting survey response:', submitResponseDto);
    
    this.responseService.submitResponse(this.surveyId, submitResponseDto).subscribe({
      next: (response) => {
        console.log('Survey submitted successfully:', response);
        console.log('Response data:', JSON.stringify(response, null, 2));
        alert('Survey submitted successfully! Thank you for your participation.');
        
        // Trigger dashboard refresh to immediately update available/completed surveys
        this.dashboardRefreshService.triggerRefresh();
        
        // Navigate back to dashboard
        this.router.navigate(['/user/dashboard']);
      },
      error: (err) => {
        console.error('Failed to submit survey:', err);
        console.error('Error details:', {
          status: err.status,
          statusText: err.statusText,
          error: err.error,
          message: err.message
        });
        this.errorMessage = `Failed to submit survey: ${err.error?.message || err.message || 'Unknown error'}. Please try again.`;
        this.isSubmitting = false;
      }
    });
  }
}
