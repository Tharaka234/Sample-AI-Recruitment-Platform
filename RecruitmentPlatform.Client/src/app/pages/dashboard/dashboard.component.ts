import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService, UserData } from '../../services/auth.service';
import { ApiService, DashboardStats } from '../../services/api.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  user: UserData | null = null;
  stats: DashboardStats | null = null;

  candidateActions = [
    { title: 'Browse Jobs', desc: 'Search thousands of positions matched to your skills', color: 'from-emerald-500 to-cyan-500', link: '/jobs' },
    { title: 'My Applications', desc: 'Track the status of your submitted applications', color: 'from-cyan-500 to-blue-500', link: '/my-applications' },
    { title: 'AI Insights', desc: 'Get personalized career advice and market trends', color: 'from-teal-500 to-emerald-600', link: '/insights' },
  ];

  recruiterActions = [
    { title: 'Manage Jobs', desc: 'Post new jobs and review candidates', color: 'from-emerald-500 to-cyan-500', link: '/recruiters' },
    { title: 'Analytics', desc: 'Track hiring pipeline metrics and performance', color: 'from-teal-500 to-emerald-600', link: '/insights' },
  ];

  hiringManagerActions = [
    { title: 'Hiring Dashboard', desc: 'Review shortlisted candidates and evaluate', color: 'from-blue-500 to-cyan-500', link: '/hiring-manager' },
    { title: 'AI Insights', desc: 'Predictive hiring analytics and trends', color: 'from-indigo-500 to-blue-600', link: '/insights' },
  ];

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
    this.api.getStats().subscribe({
      next: (data) => this.stats = data,
      error: () => {}
    });
  }

  get actions() {
    if (this.user?.role === 'Recruiter') return this.recruiterActions;
    if (this.user?.role === 'HiringManager') return this.hiringManagerActions;
    return this.candidateActions;
  }

  get statCards() {
    if (this.user?.role === 'Recruiter' || this.user?.role === 'HiringManager') {
      return [
        { label: 'Active Jobs', value: this.stats?.totalJobs || 0 },
        { label: 'Total Applications', value: this.stats?.totalApplications || 0 },
        { label: 'Shortlisted', value: this.stats?.shortlisted || 0 },
        { label: 'Hired', value: this.stats?.hired || 0 },
      ];
    }
    return [
      { label: 'Total Applications', value: this.stats?.totalApplications || 0 },
      { label: 'Shortlisted', value: this.stats?.shortlisted || 0 },
      { label: 'Interviewed', value: this.stats?.interviewed || 0 },
      { label: 'Avg Match', value: this.stats ? this.stats.avgMatchScore + '%' : '0%' },
    ];
  }

  navigate(link: string): void {
    this.router.navigate([link]);
  }
}
