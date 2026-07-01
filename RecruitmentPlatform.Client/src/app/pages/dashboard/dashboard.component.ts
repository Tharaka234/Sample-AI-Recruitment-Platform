import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService, UserData } from '../../services/auth.service';
import { ApiService, DashboardStats } from '../../services/api.service';

interface DashboardAction {
  title: string;
  desc: string;
  color: string;
  link: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  user: UserData | null = null;
  stats: DashboardStats | null = null;

  title: string = '';
  subtitle: string = '';
  actions: DashboardAction[] = [];

  constructor(
    private auth: AuthService,
    private api: ApiService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.user = this.auth.getUser();
    if (!this.user) {
      this.router.navigate(['/signin']);
      return;
    }
    
    this.setupDashboard();

    this.api.getStats().subscribe({
      next: (data) => this.stats = data,
      error: () => {}
    });
  }

  private setupDashboard(): void {
    const name = this.user?.firstName || 'User';
    switch (this.user?.role) {
      case 'Candidate':
        this.title = 'Candidate Dashboard';
        this.subtitle = `Welcome back, ${name}! Track your applications and discover new opportunities.`;
        this.actions = [
          { title: 'Browse Jobs', desc: 'Search thousands of positions matched to your skills', color: 'from-blue-600 to-purple-600', link: '/jobs' },
          { title: 'My Applications', desc: 'Track the status of your submitted applications', color: 'from-purple-600 to-cyan-500', link: '/my-applications' },
          { title: 'AI Insights', desc: 'Get personalized career advice and market trends', color: 'from-cyan-500 to-blue-600', link: '/insights' },
        ];
        break;
      case 'Recruiter':
        this.title = 'Recruiter Dashboard';
        this.subtitle = `Welcome back, ${name}! Manage your active job postings and review AI-scored candidates.`;
        this.actions = [
          { title: 'Manage Jobs', desc: 'Post new positions and edit existing ones', color: 'from-blue-600 to-purple-600', link: '/recruiters' },
          { title: 'Review Candidates', desc: 'View AI matches and manage candidate pipelines', color: 'from-purple-600 to-cyan-500', link: '/recruiters' },
        ];
        break;
      case 'HiringManager':
        this.title = 'Hiring Manager Dashboard';
        this.subtitle = `Welcome back, ${name}! Review shortlisted candidates and manage interviews.`;
        this.actions = [
          { title: 'Review Shortlist', desc: 'See top candidates recommended by recruiters', color: 'from-blue-600 to-purple-600', link: '/hiring-manager' },
          { title: 'Schedule Interviews', desc: 'Coordinate interview times with candidates', color: 'from-purple-600 to-cyan-500', link: '/hiring-manager' },
        ];
        break;
      case 'Admin':
        this.title = 'Admin Dashboard';
        this.subtitle = `Welcome back, ${name}! Monitor platform metrics and manage system settings.`;
        this.actions = [
          { title: 'User Management', desc: 'Manage roles and system access', color: 'from-blue-600 to-purple-600', link: '/admin' },
          { title: 'Platform Analytics', desc: 'View overall system performance and metrics', color: 'from-purple-600 to-cyan-500', link: '/admin' },
          { title: 'System Settings', desc: 'Configure application parameters', color: 'from-cyan-500 to-blue-600', link: '/admin' },
        ];
        break;
      default:
        this.title = 'Dashboard';
        this.subtitle = `Welcome back, ${name}!`;
        this.actions = [];
    }
  }

  get statCards() {
    if (this.user?.role === 'Candidate') {
      return [
        { label: 'Total Applications', value: this.stats?.totalApplications || 0 },
        { label: 'Shortlisted', value: this.stats?.shortlisted || 0 },
        { label: 'Interviewed', value: this.stats?.interviewed || 0 },
        { label: 'Avg Match', value: this.stats ? this.stats.avgMatchScore + '%' : '0%' },
      ];
    } else if (this.user?.role === 'Recruiter') {
      return [
        { label: 'Active Jobs', value: 12 },
        { label: 'Total Candidates', value: 145 },
        { label: 'Shortlisted', value: 24 },
        { label: 'Avg Time to Hire', value: '18 Days' },
      ];
    } else if (this.user?.role === 'HiringManager') {
      return [
        { label: 'Open Requisitions', value: 3 },
        { label: 'Pending Interviews', value: 5 },
        { label: 'Offers Extended', value: 2 },
        { label: 'Hires This Quarter', value: 7 },
      ];
    } else { // Admin
      return [
        { label: 'Total Users', value: 1240 },
        { label: 'Active Jobs', value: 342 },
        { label: 'Platform Uptime', value: '99.9%' },
        { label: 'System Alerts', value: 0 },
      ];
    }
  }

  navigate(link: string): void {
    this.router.navigate([link]);
  }
}
