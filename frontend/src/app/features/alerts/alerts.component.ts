import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { TranslateModule } from '@ngx-translate/core';
import { InventoryService } from '../../core/services/inventory.service';
import { AlertsResponse } from '../../core/dtos/responses';

@Component({
  selector: 'app-alerts',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule, SkeletonModule, TranslateModule],
  templateUrl: './alerts.component.html',
  styleUrl: './alerts.component.scss',
})
export class AlertsComponent implements OnInit {
  alerts = signal<AlertsResponse | null>(null);
  loading = signal(true);

  constructor(private inventoryService: InventoryService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.inventoryService.getAlerts().subscribe({
      next: (res) => {
        if (res.success) this.alerts.set(res.data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
