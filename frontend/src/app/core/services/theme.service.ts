import { Injectable, signal, computed, effect } from '@angular/core';

export type ThemeMode = 'light' | 'dark' | 'system';

const STORAGE_KEY = 'pharma_theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly _mode = signal<ThemeMode>(this.loadSaved());

  readonly mode = this._mode.asReadonly();

  readonly isDark = computed(() => {
    const m = this._mode();
    if (m === 'system') return this.prefersDark();
    return m === 'dark';
  });

  constructor() {
    effect(() => {
      this.applyTheme(this.isDark());
    });
  }

  setMode(mode: ThemeMode): void {
    this._mode.set(mode);
    localStorage.setItem(STORAGE_KEY, mode);
  }

  toggle(): void {
    this.setMode(this.isDark() ? 'light' : 'dark');
  }

  private loadSaved(): ThemeMode {
    const saved = localStorage.getItem(STORAGE_KEY) as ThemeMode | null;
    return saved ?? 'system';
  }

  private prefersDark(): boolean {
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
  }

  private applyTheme(dark: boolean): void {
    const root = document.documentElement;
    if (dark) {
      root.classList.add('dark-mode');
      root.classList.remove('light-mode');
    } else {
      root.classList.add('light-mode');
      root.classList.remove('dark-mode');
    }
  }
}
