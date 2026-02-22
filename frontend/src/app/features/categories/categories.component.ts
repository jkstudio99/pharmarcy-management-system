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
import { CategoryService } from '../../core/services/category.service';
import { CategoryResponse } from '../../core/dtos/responses';
import { CategoryRequest } from '../../core/dtos/requests';
import { AuthService } from '../../core/services/auth.service';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-categories',
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
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss',
})
export class CategoriesComponent implements OnInit {
  readonly auth = inject(AuthService);

  categories = signal<CategoryResponse[]>([]);
  loading = signal(true);
  saving = signal(false);

  showDialog = signal(false);
  editMode = signal(false);
  editId = signal<number | null>(null);
  form: CategoryRequest = { categoryName: '' };

  constructor(
    private categoryService: CategoryService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
  ) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.categoryService.getAll().subscribe({
      next: (res) => {
        if (res.success) this.categories.set(res.data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate() {
    this.form = { categoryName: '' };
    this.editMode.set(false);
    this.editId.set(null);
    this.showDialog.set(true);
  }

  openEdit(c: CategoryResponse) {
    this.form = { categoryName: c.categoryName, description: c.description };
    this.editMode.set(true);
    this.editId.set(c.categoryId);
    this.showDialog.set(true);
  }

  save() {
    if (!this.form.categoryName) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation',
        detail: 'Category name is required.',
      });
      return;
    }
    this.saving.set(true);
    const obs = this.editMode()
      ? this.categoryService.update(this.editId()!, this.form)
      : this.categoryService.create(this.form);
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

  confirmDelete(c: CategoryResponse) {
    this.confirmationService.confirm({
      message: `Deactivate category "${c.categoryName}"?`,
      header: 'Confirm',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.categoryService.remove(c.categoryId).subscribe({
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
}
