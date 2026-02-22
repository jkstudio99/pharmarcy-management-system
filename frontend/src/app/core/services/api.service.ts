import { inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

/**
 * Base service providing typed HTTP helpers and a consistent URL builder.
 * All feature services extend this class instead of injecting HttpClient directly.
 *
 * URL building:
 *   this.url('medicines')          → /api/medicines
 *   this.url('medicines', 1)       → /api/medicines/1
 *   this.url('medicines', 1, 'image') → /api/medicines/1/image
 */
export abstract class ApiService {
  protected readonly http = inject(HttpClient);
  protected readonly baseUrl = environment.apiUrl;

  /** Build a URL from path segments, e.g. url('medicines', id, 'image') */
  protected url(...segments: (string | number)[]): string {
    return [this.baseUrl, ...segments].join('/');
  }

  protected get<T>(path: string, params?: HttpParams): Observable<T> {
    return this.http.get<T>(path, { params });
  }

  protected post<T>(path: string, body: unknown): Observable<T> {
    return this.http.post<T>(path, body);
  }

  protected put<T>(path: string, body: unknown): Observable<T> {
    return this.http.put<T>(path, body);
  }

  protected patch<T>(path: string, body: unknown): Observable<T> {
    return this.http.patch<T>(path, body);
  }

  protected delete<T>(path: string): Observable<T> {
    return this.http.delete<T>(path);
  }

  protected postForm<T>(path: string, formData: FormData): Observable<T> {
    return this.http.post<T>(path, formData);
  }

  /** Build HttpParams from a plain object, skipping null/undefined values */
  protected buildParams(obj: Record<string, string | number | boolean | null | undefined>): HttpParams {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(obj)) {
      if (value !== null && value !== undefined && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return params;
  }
}
