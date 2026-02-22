import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { DatePickerModule } from 'primeng/datepicker';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { InventoryService } from '../../core/services/inventory.service';
import { MedicineService } from '../../core/services/medicine.service';
import { SupplierService } from '../../core/services/supplier.service';
import { InventoryBatchResponse } from '../../core/dtos/responses';
import { MedicineResponse } from '../../core/dtos/responses';
import { SupplierResponse } from '../../core/dtos/responses';
import { StockInRequest, StockOutFefoRequest, StockAdjustRequest } from '../../core/dtos/requests';
import { AuthService } from '../../core/services/auth.service';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-inventory',
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
    DatePickerModule,
    ToastModule,
    ConfirmDialogModule,
    TagModule,
    TooltipModule,
    TranslateModule,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './inventory.component.html',
  styleUrl: './inventory.component.scss',
})
export class InventoryComponent implements OnInit {
  readonly auth = inject(AuthService);

  batches = signal<InventoryBatchResponse[]>([]);
  medicines = signal<MedicineResponse[]>([]);
  suppliers = signal<SupplierResponse[]>([]);
  loading = signal(true);
  saving = signal(false);

  totalRecords = signal(0);
  page = 1;
  pageSize = 20;

  showStockIn = signal(false);
  showStockOut = signal(false);
  showAdjust = signal(false);

  form: StockInRequest = this.emptyForm();
  stockOutForm: StockOutFefoRequest = { drugId: 0, quantity: 1 };
  adjustForm: StockAdjustRequest = { batchId: 0, newQuantity: 0, reason: '' };
  selectedBatch: InventoryBatchResponse | null = null;

  constructor(
    private inventoryService: InventoryService,
    private medicineService: MedicineService,
    private supplierService: SupplierService,
    private messageService: MessageService,
  ) {}

  ngOnInit() {
    this.loadBatches();
    this.loadDropdowns();
  }

  loadBatches() {
    this.loading.set(true);
    this.inventoryService.getAll(undefined, undefined, this.page, this.pageSize).subscribe({
      next: (res) => {
        if (res.success) {
          this.batches.set(res.data.items);
          this.totalRecords.set(res.data.totalCount);
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  loadDropdowns() {
    this.medicineService.getAll(undefined, undefined, 1, 200).subscribe({
      next: (res) => {
        if (res.success) this.medicines.set(res.data.items);
      },
    });
    this.supplierService.getAll().subscribe({
      next: (res) => {
        if (res.success) this.suppliers.set(res.data);
      },
    });
  }

  onPageChange(event: any) {
    this.page = Math.floor(event.first / event.rows) + 1;
    this.pageSize = event.rows;
    this.loadBatches();
  }

  openStockIn() {
    this.form = this.emptyForm();
    this.showStockIn.set(true);
  }

  openStockOut() {
    this.stockOutForm = { drugId: 0, quantity: 1 };
    this.showStockOut.set(true);
  }

  openAdjust(batch: InventoryBatchResponse) {
    this.selectedBatch = batch;
    this.adjustForm = { batchId: batch.batchId, newQuantity: batch.quantityInStock, reason: '' };
    this.showAdjust.set(true);
  }

  stockOut() {
    if (!this.stockOutForm.drugId || this.stockOutForm.quantity < 1) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation',
        detail: 'Drug and quantity are required.',
      });
      return;
    }
    this.saving.set(true);
    this.inventoryService.stockOutFefo(this.stockOutForm).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Stock Out',
            detail: 'Stock deducted (FEFO).',
          });
          this.showStockOut.set(false);
          this.loadBatches();
        } else {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: res.message });
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.message ?? 'Stock out failed.',
        });
      },
    });
  }

  adjust() {
    if (this.adjustForm.newQuantity < 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation',
        detail: 'Quantity cannot be negative.',
      });
      return;
    }
    this.saving.set(true);
    this.inventoryService.adjust(this.adjustForm).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.messageService.add({ severity: 'success', summary: 'Adjusted', detail: res.data });
          this.showAdjust.set(false);
          this.loadBatches();
        } else {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: res.message });
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.message ?? 'Adjustment failed.',
        });
      },
    });
  }

  stockIn() {
    if (!this.form.drugId || !this.form.batchNumber || this.form.quantity < 1) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation',
        detail: 'Drug, batch number and quantity are required.',
      });
      return;
    }
    this.saving.set(true);
    this.inventoryService.stockIn(this.form).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Stock In',
            detail: 'Batch added successfully.',
          });
          this.showStockIn.set(false);
          this.loadBatches();
        } else {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: res.message });
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.message ?? 'Stock in failed.',
        });
      },
    });
  }

  medicineOptions() {
    return this.medicines().map((m) => ({ label: m.drugName, value: m.drugId }));
  }

  supplierOptions() {
    return this.suppliers().map((s) => ({ label: s.supplierName, value: s.supplierId }));
  }

  isExpiringSoon(batch: InventoryBatchResponse): boolean {
    return batch.isExpiringSoon;
  }

  private emptyForm(): StockInRequest {
    return { drugId: 0, batchNumber: '', quantity: 1 };
  }
}
