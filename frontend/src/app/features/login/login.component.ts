import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { MessageModule } from 'primeng/message';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService } from '../../core/services/theme.service';
import { I18nService, LANGUAGES } from '../../core/services/i18n.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    InputTextModule,
    PasswordModule,
    MessageModule,
    SelectModule,
    TooltipModule,
    TranslateModule,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  email = '';
  password = '';
  loading = signal(false);
  error = signal('');

  readonly languages = LANGUAGES;

  constructor(
    private auth: AuthService,
    private router: Router,
    readonly theme: ThemeService,
    readonly i18n: I18nService,
    private translate: TranslateService,
  ) {}

  submit() {
    if (!this.email || !this.password) {
      this.error.set(this.translate.instant('AUTH.VALIDATION_REQUIRED'));
      return;
    }
    this.loading.set(true);
    this.error.set('');
    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) this.router.navigate(['/dashboard']);
        else this.error.set(res.message);
      },
      error: (err) => {
        this.loading.set(false);
        const msg = err.error?.message ?? this.translate.instant('AUTH.LOGIN_FAILED');
        this.error.set(msg);
      },
    });
  }
}
