import { Component, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { ToastModule } from 'primeng/toast';
import { PasswordModule } from 'primeng/password';
import { MessageService } from 'primeng/api';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    InputTextModule,
    CardModule,
    TagModule,
    DividerModule,
    ToastModule,
    PasswordModule,
    TranslateModule,
  ],
  providers: [MessageService],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class ProfileComponent {
  user = computed(() => this.auth.user());

  showChangePassword = signal(false);
  changingPassword = signal(false);
  pwForm = { current: '', newPw: '', confirm: '' };

  private messageService = inject(MessageService);

  readonly roleColors: Record<string, 'success' | 'info' | 'warn'> = {
    Admin: 'success',
    Pharmacist: 'info',
    StockEmployee: 'warn',
  };

  constructor(readonly auth: AuthService) {}

  submitChangePassword() {
    if (!this.pwForm.current || !this.pwForm.newPw || !this.pwForm.confirm) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation',
        detail: 'All fields are required.',
      });
      return;
    }
    if (this.pwForm.newPw !== this.pwForm.confirm) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation',
        detail: 'New passwords do not match.',
      });
      return;
    }
    if (this.pwForm.newPw.length < 6) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation',
        detail: 'Password must be at least 6 characters.',
      });
      return;
    }
    this.changingPassword.set(true);
    this.auth.changePassword(this.pwForm.current, this.pwForm.newPw).subscribe({
      next: (res) => {
        this.changingPassword.set(false);
        if (res.success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Password changed successfully.',
          });
          this.pwForm = { current: '', newPw: '', confirm: '' };
          this.showChangePassword.set(false);
        } else {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: res.message });
        }
      },
      error: (err) => {
        this.changingPassword.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.message ?? 'Failed to change password.',
        });
      },
    });
  }

  getRoleColor(role: string): 'success' | 'info' | 'warn' {
    return this.roleColors[role] ?? 'info';
  }

  getInitials(name: string | undefined): string {
    if (!name) return '?';
    return name
      .split(' ')
      .map((w) => w.charAt(0))
      .slice(0, 2)
      .join('')
      .toUpperCase();
  }
}
