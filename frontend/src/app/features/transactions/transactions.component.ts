import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { SkeletonModule } from 'primeng/skeleton';
import { TooltipModule } from 'primeng/tooltip';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { startWith, map } from 'rxjs';
import { TransactionService } from '../../core/services/transaction.service';
import { TransactionResponse } from '../../core/dtos/responses';

@Component({
  selector: 'app-transactions',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    TableModule,
    TagModule,
    SelectModule,
    DatePickerModule,
    SkeletonModule,
    TooltipModule,
    TranslateModule,
  ],
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.scss',
})
export class TransactionsComponent implements OnInit {
  transactions = signal<TransactionResponse[]>([]);
  loading = signal(true);
  totalRecords = signal(0);
  pageSize = 20;

  filterType = '';
  filterFrom: Date | null = null;
  filterTo: Date | null = null;

  private translateSvc = inject(TranslateService);
  private txService = inject(TransactionService);

  private _lang = toSignal(
    this.translateSvc.onLangChange.pipe(
      startWith(null),
      map(() => this.translateSvc.currentLang),
    ),
  );

  typeOptions = computed(() => {
    this._lang();
    return [
      { label: this.translateSvc.instant('TRANSACTIONS.ALL_TYPES'), value: '' },
      { label: 'IN', value: 'IN' },
      { label: 'OUT', value: 'OUT' },
      { label: 'ADJUST', value: 'ADJUST' },
      { label: 'EXPIRED', value: 'EXPIRED' },
    ];
  });

  constructor() {}

  ngOnInit() {
    this.load();
  }

  load(page = 1) {
    this.loading.set(true);
    this.txService
      .getAll(
        this.filterType || undefined,
        this.filterFrom ? this.filterFrom.toISOString() : undefined,
        this.filterTo ? this.filterTo.toISOString() : undefined,
        page,
        this.pageSize,
      )
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.transactions.set(res.data.items);
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

  getTypeSeverity(type: string): 'success' | 'warn' | 'danger' | 'info' | 'secondary' {
    switch (type) {
      case 'IN':
        return 'success';
      case 'OUT':
        return 'info';
      case 'ADJUST':
        return 'warn';
      case 'EXPIRED':
        return 'danger';
      default:
        return 'secondary';
    }
  }

  resetFilters() {
    this.filterType = '';
    this.filterFrom = null;
    this.filterTo = null;
    this.load();
  }
}
