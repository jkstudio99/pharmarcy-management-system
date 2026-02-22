import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { SkeletonModule } from 'primeng/skeleton';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { MedicineService } from '../../core/services/medicine.service';
import { CategoryService } from '../../core/services/category.service';
import { MedicineResponse } from '../../core/dtos/responses';
import { CategoryResponse } from '../../core/dtos/responses';
import { MedicineRequest } from '../../core/dtos/requests';
import { AuthService } from '../../core/services/auth.service';
import { TranslateModule } from '@ngx-translate/core';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-medicines',
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
    TagModule,
    ToastModule,
    ConfirmDialogModule,
    SkeletonModule,
    TooltipModule,
    TranslateModule,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './medicines.component.html',
  styleUrl: './medicines.component.scss',
})
export class MedicinesComponent implements OnInit {
  medicines = signal<MedicineResponse[]>([]);
  categories = signal<CategoryResponse[]>([]);
  loading = signal(true);
  saving = signal(false);

  totalRecords = signal(0);
  page = 1;
  pageSize = 20;
  search = '';
  selectedCategoryId: number | null = null;

  showDialog = signal(false);
  editMode = signal(false);
  editId = signal<number | null>(null);

  showImageDialog = signal(false);
  imageTarget = signal<MedicineResponse | null>(null);
  uploadingImage = signal(false);
  selectedFile: File | null = null;
  imagePreview: string | null = null;

  private readonly apiOrigin = environment.apiUrl.replace(/\/api\/?$/, '');

  form: MedicineRequest = this.emptyForm();

  readonly auth = inject(AuthService);
  readonly isPharmacistUp = this.auth.isPharmacistUp;

  constructor(
    private medicineService: MedicineService,
    private categoryService: CategoryService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
  ) {}

  ngOnInit() {
    this.loadMedicines();
    this.loadCategories();
  }

  resolveImageUrl(path?: string | null): string | null {
    if (!path) return null;
    if (path.startsWith('http://') || path.startsWith('https://')) return path;
    if (!path.startsWith('/')) return `${this.apiOrigin}/${path}`;
    return `${this.apiOrigin}${path}`;
  }

  onThumbError(event: Event) {
    const el = event.target as HTMLImageElement;
    el.style.display = 'none';
  }

  loadMedicines() {
    this.loading.set(true);
    this.medicineService
      .getAll(
        this.search || undefined,
        this.selectedCategoryId ?? undefined,
        this.page,
        this.pageSize,
      )
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.medicines.set(res.data.items);
            this.totalRecords.set(res.data.totalCount);
          }
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  loadCategories() {
    this.categoryService.getAll().subscribe({
      next: (res) => {
        if (res.success) this.categories.set(res.data);
      },
    });
  }

  onSearch() {
    this.page = 1;
    this.loadMedicines();
  }

  onPageChange(event: any) {
    this.page = Math.floor(event.first / event.rows) + 1;
    this.pageSize = event.rows;
    this.loadMedicines();
  }

  openCreate() {
    this.form = this.emptyForm();
    this.editMode.set(false);
    this.editId.set(null);
    this.showDialog.set(true);
  }

  openEdit(m: MedicineResponse) {
    this.form = {
      barcode: m.barcode,
      drugName: m.drugName,
      genericName: m.genericName,
      unit: m.unit,
      categoryId: m.categoryId,
      reorderLevel: m.reorderLevel,
      imageUrl: m.imageUrl,
    };
    this.editMode.set(true);
    this.editId.set(m.drugId);
    this.showDialog.set(true);
  }

  save() {
    if (!this.form.drugName) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation',
        detail: 'Drug name is required.',
      });
      return;
    }
    this.saving.set(true);
    const obs = this.editMode()
      ? this.medicineService.update(this.editId()!, this.form)
      : this.medicineService.create(this.form);

    obs.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: res.message });
          this.showDialog.set(false);
          this.loadMedicines();
        } else {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: res.message });
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.message ?? 'Operation failed.',
        });
      },
    });
  }

  confirmDelete(m: MedicineResponse) {
    this.confirmationService.confirm({
      message: `Deactivate "${m.drugName}"?`,
      header: 'Confirm Deactivate',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.medicineService.remove(m.drugId).subscribe({
          next: (res) => {
            if (res.success) {
              this.messageService.add({
                severity: 'success',
                summary: 'Deactivated',
                detail: res.message,
              });
              this.loadMedicines();
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

  openImageUpload(m: MedicineResponse) {
    this.imageTarget.set(m);
    this.selectedFile = null;
    this.imagePreview = this.resolveImageUrl(m.imageUrl);
    this.showImageDialog.set(true);
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.selectedFile = input.files[0];
      const reader = new FileReader();
      reader.onload = (e) => {
        this.imagePreview = e.target?.result as string;
      };
      reader.readAsDataURL(this.selectedFile);
    }
  }

  uploadImage() {
    if (!this.selectedFile || !this.imageTarget()) return;
    this.uploadingImage.set(true);
    this.medicineService.uploadImage(this.imageTarget()!.drugId, this.selectedFile).subscribe({
      next: (res) => {
        this.uploadingImage.set(false);
        if (res.success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Image uploaded.',
          });
          this.showImageDialog.set(false);
          this.loadMedicines();
        } else {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: res.message });
        }
      },
      error: (err) => {
        this.uploadingImage.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.message ?? 'Upload failed.',
        });
      },
    });
  }

  categoryOptions() {
    return this.categories().map((c) => ({ label: c.categoryName, value: c.categoryId }));
  }

  private emptyForm(): MedicineRequest {
    return { drugName: '', reorderLevel: 10 };
  }
}
