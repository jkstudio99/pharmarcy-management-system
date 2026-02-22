import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResult } from '../dtos/responses';
import { AuditLogResponse } from '../dtos/responses';

@Injectable({ providedIn: 'root' })
export class AuditLogService extends ApiService {
  getAll(
    tableName?: string,
    action?: string,
    from?: string,
    to?: string,
    page = 1,
    pageSize = 20,
  ): Observable<ApiResponse<PagedResult<AuditLogResponse>>> {
    const params = this.buildParams({ page, pageSize, tableName, action, from, to });
    return this.get<ApiResponse<PagedResult<AuditLogResponse>>>(
      this.url('audit-logs'),
      params,
    );
  }
}
