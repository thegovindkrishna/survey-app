export interface Survey {
  id?: number;
  title: string;
  description: string;
  startDate?: Date;
  endDate?: Date;
  createdBy?: string;
  createdAt?: Date;
  shareLink?: string;
  questions?: Question[];
}

export interface Question {
  id?: number;
  text: string;
  type: QuestionType;
  required: boolean;
  options?: string[];
  maxRating?: number;
}

export interface QuestionResponse {
  questionId: number;
  response: string;
}

export interface SurveyResults {
  surveyId: number;
  surveyTitle: string;
  totalResponses: number;
  questionResults: QuestionResult[];
}

export interface QuestionResult {
  questionId: number;
  questionText: string;
  questionType: string;
  responseCounts: { [key: string]: number };
  averageRating?: number;
}

export enum QuestionType {
  Text = 'text',
  Number = 'number',
  SingleChoice = 'single_choice',
  MultipleChoice = 'multiple_choice',
  Date = 'date',
  Rating = 'rating'
} 