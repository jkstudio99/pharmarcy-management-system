import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { SupplierService } from '../../core/services/supplier.service';
import { SupplierResponse } from '../../core/dtos/responses';
import { SupplierRequest } from '../../core/dtos/requests';
import { AuthService } from '../../core/services/auth.service';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-suppliers',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    InputTextModule,
    TableModule,
    DialogModule,
    TextareaModule,
    ToastModule,
    ConfirmDialogModule,
    TranslateModule,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './suppliers.component.html',
  styleUrl: './suppliers.component.scss',
})
export class SuppliersComponent implements OnInit {
  readonly auth = inject(AuthService);

  suppliers = signal<SupplierResponse[]>([]);
  loading = signal(true);
  saving = signal(false);
  search = '';

  showDialog = signal(false);
  editMode = signal(false);
  editId = signal<number | null>(null);
  form: SupplierRequest = this.emptyForm();

  constructor(
    private supplierService: SupplierService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
  ) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.supplierService.getAll(this.search || undefined).subscribe({
      next: (res) => {
        if (res.success) this.suppliers.set(res.data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate() {
    this.form = this.emptyForm();
    this.editMode.set(false);
    this.editId.set(null);
    this.showDialog.set(true);
  }

  openEdit(s: SupplierResponse) {
    this.form = { supplierName: s.supplierName, contact: s.contact, address: s.address };
    this.editMode.set(true);
    this.editId.set(s.supplierId);
    this.showDialog.set(true);
  }

  save() {
    if (!this.form.supplierName) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation',
        detail: 'Supplier name is required.',
      });
      return;
    }
    this.saving.set(true);
    const obs = this.editMode()
      ? this.supplierService.update(this.editId()!, this.form)
      : this.supplierService.create(this.form);
    obs.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: res.message });
          this.showDialog.set(false);
          this.load();
        } else {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: res.message });
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.message ?? 'Failed.',
        });
      },
    });
  }

  confirmDelete(s: SupplierResponse) {
    this.confirmationService.confirm({
      message: `Deactivate "${s.supplierName}"?`,
      header: 'Confirm',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.supplierService.remove(s.supplierId).subscribe({
          next: (res) => {
            if (res.success) {
              this.messageService.add({
                severity: 'success',
                summary: 'Done',
                detail: res.message,
              });
              this.load();
            }
          },
          error: (err) =>
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: err.error?.message ?? 'Failed.',
            }),
        });
      },
    });
  }

  private emptyForm(): SupplierRequest {
    return { supplierName: '' };
  }
}
