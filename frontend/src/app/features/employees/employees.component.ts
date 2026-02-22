import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { MultiSelectModule } from 'primeng/multiselect';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TranslateModule } from '@ngx-translate/core';
import { EmployeeService } from '../../core/services/employee.service';
import { EmployeeResponse } from '../../core/dtos/responses';
import { UpdateEmployeeRequest } from '../../core/dtos/requests';

@Component({
  selector: 'app-employees',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    InputTextModule,
    TableModule,
    DialogModule,
    MultiSelectModule,
    ToggleSwitchModule,
    ToastModule,
    ConfirmDialogModule,
    TagModule,
    TooltipModule,
    TranslateModule,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './employees.component.html',
  styleUrl: './employees.component.scss',
})
export class EmployeesComponent implements OnInit {
  employees = signal<EmployeeResponse[]>([]);
  loading = signal(true);
  saving = signal(false);

  showDialog = signal(false);
  editId = signal<number | null>(null);
  form: UpdateEmployeeRequest = this.emptyForm();

  roleOptions = [
    { label: 'Admin', value: 1 },
    { label: 'Pharmacist', value: 2 },
    { label: 'Stock Employee', value: 3 },
  ];

  constructor(
    private employeeService: EmployeeService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
  ) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.employeeService.getAll().subscribe({
      next: (res) => {
        if (res.success) this.employees.set(res.data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  openEdit(e: EmployeeResponse) {
    this.form = {
      empName: e.empName,
      email: e.email,
      isActive: e.isActive,
      roleIds: e.roles
        .map((r) => this.roleOptions.find((o) => o.label === r)?.value ?? 0)
        .filter((v) => v > 0),
    };
    this.editId.set(e.eId);
    this.showDialog.set(true);
  }

  save() {
    if (!this.form.empName || !this.form.email) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation',
        detail: 'Name and email are required.',
      });
      return;
    }
    this.saving.set(true);
    this.employeeService.update(this.editId()!, this.form).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.messageService.add({ severity: 'success', summary: 'Updated', detail: res.message });
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

  confirmDeactivate(e: EmployeeResponse) {
    this.confirmationService.confirm({
      message: `Deactivate "${e.empName}"?`,
      header: 'Confirm',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.employeeService.remove(e.eId).subscribe({
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

  getRoleBadge(role: string): string {
    if (role === 'Admin') return 'badge-error';
    if (role === 'Pharmacist') return 'badge-info';
    return 'badge-neutral';
  }

  private emptyForm(): UpdateEmployeeRequest {
    return { empName: '', email: '', isActive: true, roleIds: [] };
  }
}
