export interface Survey {
  id?: number;
  title: string;
  description: string;
  createdBy?: string;
  createdAt?: Date;
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

export enum QuestionType {
  Text = 'text',
  Number = 'number',
  SingleChoice = 'single_choice',
  MultipleChoice = 'multiple_choice',
  Date = 'date',
  Rating = 'rating'
} 