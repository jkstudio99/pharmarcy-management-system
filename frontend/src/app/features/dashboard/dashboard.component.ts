import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ChartModule } from 'primeng/chart';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { DashboardService } from '../../core/services/dashboard.service';
import { DashboardSummaryResponse } from '../../core/dtos/responses';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    ChartModule,
    SkeletonModule,
    TagModule,
    RouterLink,
    TranslateModule,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  summary = signal<DashboardSummaryResponse | null>(null);
  loading = signal(true);
  chartData = signal<any>(null);
  chartOptions = signal<any>(null);

  constructor(private dashboardService: DashboardService) {}

  ngOnInit() {
    this.dashboardService.getSummary().subscribe({
      next: (res) => {
        if (res.success) {
          this.summary.set(res.data);
          this.buildChart(res.data);
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  private buildChart(data: DashboardSummaryResponse) {
    const sales = [...data.monthlySales]
      .sort((a, b) => (a.year !== b.year ? a.year - b.year : a.month - b.month))
      .slice(-6);

    const monthNames = [
      'Jan',
      'Feb',
      'Mar',
      'Apr',
      'May',
      'Jun',
      'Jul',
      'Aug',
      'Sep',
      'Oct',
      'Nov',
      'Dec',
    ];

    this.chartData.set({
      labels: sales.map((s) => `${monthNames[s.month - 1]} ${s.year}`),
      datasets: [
        {
          label: 'Monthly Sales (฿)',
          data: sales.map((s) => s.totalAmount),
          backgroundColor: 'rgba(0, 199, 129, 0.15)',
          borderColor: '#00C781',
          borderWidth: 2,
          fill: true,
          tension: 0.4,
          pointBackgroundColor: '#00C781',
          pointRadius: 4,
        },
      ],
    });

    this.chartOptions.set({
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: {
          callbacks: {
            label: (ctx: any) => `฿${ctx.raw.toLocaleString()}`,
          },
        },
      },
      scales: {
        x: { grid: { display: false }, ticks: { font: { size: 12 } } },
        y: {
          grid: { color: 'rgba(0,0,0,0.05)' },
          ticks: {
            font: { size: 12 },
            callback: (v: number) => `฿${v.toLocaleString()}`,
          },
        },
      },
    });
  }
}
