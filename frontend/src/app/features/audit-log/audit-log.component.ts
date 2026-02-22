import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { SkeletonModule } from 'primeng/skeleton';
import { TooltipModule } from 'primeng/tooltip';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { startWith, map } from 'rxjs';
import { AuditLogService } from '../../core/services/audit-log.service';
import { AuditLogResponse } from '../../core/dtos/responses';

@Component({
  selector: 'app-audit-log',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    TableModule,
    TagModule,
    SelectModule,
    DatePickerModule,
    DialogModule,
    SkeletonModule,
    TooltipModule,
    TranslateModule,
  ],
  templateUrl: './audit-log.component.html',
  styleUrl: './audit-log.component.scss',
})
export class AuditLogComponent implements OnInit {
  logs = signal<AuditLogResponse[]>([]);
  loading = signal(true);
  totalRecords = signal(0);
  pageSize = 20;

  filterTable = '';
  filterAction = '';
  filterFrom: Date | null = null;
  filterTo: Date | null = null;

  selectedLog = signal<AuditLogResponse | null>(null);
  showDetail = signal(false);

  private translateSvc = inject(TranslateService);
  private auditService = inject(AuditLogService);

  private _lang = toSignal(
    this.translateSvc.onLangChange.pipe(
      startWith(null),
      map(() => this.translateSvc.currentLang),
    ),
  );

  tableOptions = computed(() => {
    this._lang();
    return [
      { label: this.translateSvc.instant('AUDIT.ALL_TABLES'), value: '' },
      { label: 'MEDICINE', value: 'MEDICINE' },
      { label: 'INVENTORY_BATCH', value: 'INVENTORY_BATCH' },
      { label: 'SUPPLIER', value: 'SUPPLIER' },
      { label: 'CATEGORY', value: 'CATEGORY' },
      { label: 'EMPLOYEE', value: 'EMPLOYEE' },
      { label: 'SALES_ORDER', value: 'SALES_ORDER' },
    ];
  });

  actionOptions = computed(() => {
    this._lang();
    return [
      { label: this.translateSvc.instant('AUDIT.ALL_ACTIONS'), value: '' },
      { label: 'INSERT', value: 'INSERT' },
      { label: 'UPDATE', value: 'UPDATE' },
      { label: 'DELETE', value: 'DELETE' },
    ];
  });

  constructor() {}

  ngOnInit() {
    this.load();
  }

  load(page = 1) {
    this.loading.set(true);
    this.auditService
      .getAll(
        this.filterTable || undefined,
        this.filterAction || undefined,
        this.filterFrom ? this.filterFrom.toISOString() : undefined,
        this.filterTo ? this.filterTo.toISOString() : undefined,
        page,
        this.pageSize,
      )
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.logs.set(res.data.items);
            this.totalRecords.set(res.data.totalCount);
          }
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  onPageChange(event: any) {
    this.load(Math.floor(event.first / event.rows) + 1);
  }

  viewDetail(log: AuditLogResponse) {
    this.selectedLog.set(log);
    this.showDetail.set(true);
  }

  getActionSeverity(action: string): 'success' | 'warn' | 'danger' | 'info' {
    switch (action) {
      case 'INSERT':
        return 'success';
      case 'UPDATE':
        return 'warn';
      case 'DELETE':
        return 'danger';
      default:
        return 'info';
    }
  }

  formatJson(raw?: string): string {
    if (!raw) return '—';
    try {
      return JSON.stringify(JSON.parse(raw), null, 2);
    } catch {
      return raw;
    }
  }

  resetFilters() {
    this.filterTable = '';
    this.filterAction = '';
    this.filterFrom = null;
    this.filterTo = null;
    this.load();
  }
}
