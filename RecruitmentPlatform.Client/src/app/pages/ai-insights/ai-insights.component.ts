import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-ai-insights',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './ai-insights.component.html'
})
export class AiInsightsComponent implements OnInit {
  isLoggedIn = false;

  constructor(public auth: AuthService) {}

  ngOnInit(): void {
    this.isLoggedIn = this.auth.isLoggedIn();
  }
}
