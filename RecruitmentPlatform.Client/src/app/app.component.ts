import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService, UserData } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  isMenuOpen = false;
  isDarkMode = false;
  user: UserData | null = null;

  constructor(public auth: AuthService) {
    this.auth.user$.subscribe(u => this.user = u);
    
    // Check localStorage for saved theme preference
    this.isDarkMode = localStorage.getItem('darkMode') === 'true';
    if (this.isDarkMode) {
      document.body.classList.add('dark-mode');
    }
  }

  toggleDarkMode(): void {
    this.isDarkMode = !this.isDarkMode;
    document.body.classList.toggle('dark-mode');
    localStorage.setItem('darkMode', String(this.isDarkMode));
  }

  logout(): void {
    this.auth.logout();
    this.isMenuOpen = false;
  }

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;
  }
}
