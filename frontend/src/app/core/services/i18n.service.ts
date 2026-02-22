import { Injectable, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

export type Lang = 'th' | 'en' | 'ja';

const STORAGE_KEY = 'pharma_lang';

export interface LangOption {
  code: Lang;
  label: string;
  flag: string;
}

export const LANGUAGES: LangOption[] = [
  { code: 'th', label: 'ภาษาไทย', flag: '🇹🇭' },
  { code: 'en', label: 'English', flag: '🇬🇧' },
  { code: 'ja', label: '日本語', flag: '🇯🇵' },
];

@Injectable({ providedIn: 'root' })
export class I18nService {
  private readonly _lang = signal<Lang>(this.loadSaved());

  readonly lang = this._lang.asReadonly();
  readonly languages = LANGUAGES;

  get selectedOption(): LangOption {
    return LANGUAGES.find((l) => l.code === this._lang()) ?? LANGUAGES[1];
  }

  constructor(private translate: TranslateService) {
    translate.addLangs(['th', 'en', 'ja']);
    this.applyLang(this._lang());
  }

  setLang(lang: Lang): void {
    this._lang.set(lang);
    localStorage.setItem(STORAGE_KEY, lang);
    this.applyLang(lang);
  }

  setOption(option: LangOption | null): void {
    if (option) this.setLang(option.code);
  }

  private loadSaved(): Lang {
    const saved = localStorage.getItem(STORAGE_KEY) as Lang | null;
    if (saved && ['th', 'en', 'ja'].includes(saved)) return saved;
    const browser = navigator.language.slice(0, 2) as Lang;
    return ['th', 'en', 'ja'].includes(browser) ? browser : 'en';
  }

  private applyLang(lang: Lang): void {
    this.translate.use(lang);
    document.documentElement.setAttribute('lang', lang);
  }
}
