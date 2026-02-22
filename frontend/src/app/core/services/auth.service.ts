import { Injectable, signal, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../dtos/responses';
import { AuthResponse } from '../dtos/responses';
import { LoginRequest, RegisterRequest } from '../dtos/requests';

const TOKEN_KEY = 'pharma_token';
const USER_KEY = 'pharma_user';

@Injectable({ providedIn: 'root' })
export class AuthService extends ApiService {
  private readonly router = inject(Router);

  private readonly _user = signal<AuthResponse | null>(this.loadUser());

  readonly user = this._user.asReadonly();
  readonly isLoggedIn = computed(() => !!this._user());
  readonly isAdmin = computed(() => this._user()?.roles.includes('Admin') ?? false);
  readonly isPharmacist = computed(() => this._user()?.roles.includes('Pharmacist') ?? false);
  readonly isPharmacistUp = computed(
    () => this._user()?.roles.some((r) => r === 'Admin' || r === 'Pharmacist') ?? false,
  );

  login(req: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.post<ApiResponse<AuthResponse>>(this.url('auth', 'login'), req).pipe(
      tap((res) => {
        if (res.success) this.saveSession(res.data);
      }),
    );
  }

  register(req: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.post<ApiResponse<AuthResponse>>(this.url('auth', 'register'), req);
  }

  changePassword(currentPassword: string, newPassword: string): Observable<ApiResponse<string>> {
    return this.post<ApiResponse<string>>(this.url('auth', 'change-password'), {
      currentPassword,
      newPassword,
    });
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this._user.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  private saveSession(user: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, user.token);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this._user.set(user);
  }

  private loadUser(): AuthResponse | null {
    try {
      const raw = localStorage.getItem(USER_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }
}
