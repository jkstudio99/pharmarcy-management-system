import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResult } from '../dtos/responses';
import { TransactionResponse } from '../dtos/responses';

@Injectable({ providedIn: 'root' })
export class TransactionService extends ApiService {
  getAll(
    transType?: string,
    from?: string,
    to?: string,
    page = 1,
    pageSize = 20,
  ): Observable<ApiResponse<PagedResult<TransactionResponse>>> {
    const params = this.buildParams({ page, pageSize, transType, from, to });
    return this.get<ApiResponse<PagedResult<TransactionResponse>>>(
      this.url('transactions'),
      params,
    );
  }
}
