// src/app/surveys/app.component.ts
import { Component } from '@angular/core';

@Component({
  selector: 'app-surveys',
  standalone: true,
  template: `<h2>Welcome to Survey Dashboard</h2>`,
  styles: [`
    h2 {
      text-align: center;
      margin-top: 2rem;
      color: #333;
    }
  `]
})
export class AppComponent {}
