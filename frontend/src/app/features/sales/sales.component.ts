import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { ToastModule } from 'primeng/toast';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { SalesService } from '../../core/services/sales.service';
import { MedicineService } from '../../core/services/medicine.service';
import { SalesOrderResponse, SalesOrderDetailResponse } from '../../core/dtos/responses';
import { MedicineResponse } from '../../core/dtos/responses';
import { CreateSalesOrderRequest, SalesOrderItemRequest } from '../../core/dtos/requests';
import { AuthService } from '../../core/services/auth.service';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-sales',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    InputTextModule,
    TableModule,
    DialogModule,
    SelectModule,
    InputNumberModule,
    ToastModule,
    TagModule,
    TooltipModule,
    TranslateModule,
  ],
  providers: [MessageService],
  templateUrl: './sales.component.html',
  styleUrl: './sales.component.scss',
})
export class SalesComponent implements OnInit {
  readonly auth = inject(AuthService);

  orders = signal<SalesOrderResponse[]>([]);
  medicines = signal<MedicineResponse[]>([]);
  loading = signal(true);
  saving = signal(false);

  totalRecords = signal(0);
  page = 1;
  pageSize = 20;

  showCreate = signal(false);
  showDetail = signal(false);
  selectedOrder = signal<SalesOrderDetailResponse | null>(null);

  form: CreateSalesOrderRequest = this.emptyForm();
  items: SalesOrderItemRequest[] = [{ drugId: 0, quantity: 1 }];

  paymentOptions = [
    { label: 'Cash', value: 'Cash' },
    { label: 'QR PromptPay', value: 'QR PromptPay' },
    { label: 'Credit Card', value: 'Credit Card' },
  ];

  constructor(
    private salesService: SalesService,
    private medicineService: MedicineService,
    private messageService: MessageService,
  ) {}

  ngOnInit() {
    this.loadOrders();
    this.medicineService.getAll(undefined, undefined, 1, 200).subscribe({
      next: (res) => {
        if (res.success) this.medicines.set(res.data.items);
      },
    });
  }

  loadOrders() {
    this.loading.set(true);
    this.salesService.getAll(this.page, this.pageSize).subscribe({
      next: (res) => {
        if (res.success) {
          this.orders.set(res.data.items);
          this.totalRecords.set(res.data.totalCount);
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onPageChange(event: any) {
    this.page = Math.floor(event.first / event.rows) + 1;
    this.pageSize = event.rows;
    this.loadOrders();
  }

  openCreate() {
    this.form = this.emptyForm();
    this.items = [{ drugId: 0, quantity: 1 }];
    this.showCreate.set(true);
  }

  addItem() {
    this.items.push({ drugId: 0, quantity: 1 });
  }
  removeItem(i: number) {
    if (this.items.length > 1) this.items.splice(i, 1);
  }

  viewDetail(order: SalesOrderResponse) {
    this.salesService.getById(order.orderId).subscribe({
      next: (res) => {
        if (res.success) {
          this.selectedOrder.set(res.data);
          this.showDetail.set(true);
        }
      },
    });
  }

  create() {
    if (!this.form.paymentMethod) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation',
        detail: 'Payment method is required.',
      });
      return;
    }
    if (this.items.some((i) => !i.drugId || i.quantity < 1)) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation',
        detail: 'All items must have a drug and quantity ≥ 1.',
      });
      return;
    }
    this.saving.set(true);
    const req: CreateSalesOrderRequest = { ...this.form, items: this.items };
    this.salesService.create(req).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Order Created',
            detail: `Ref: ${res.data.referenceNo}`,
          });
          this.showCreate.set(false);
          this.loadOrders();
        } else {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: res.message });
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.message ?? 'Failed to create order.',
        });
      },
    });
  }

  medicineOptions() {
    return this.medicines().map((m) => ({
      label: `${m.drugName} (Stock: ${m.totalStock})`,
      value: m.drugId,
    }));
  }

  private emptyForm(): CreateSalesOrderRequest {
    return { customerInfo: '', paymentMethod: 'Cash', items: [] };
  }
}
