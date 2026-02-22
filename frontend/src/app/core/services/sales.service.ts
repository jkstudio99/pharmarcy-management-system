import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResult } from '../dtos/responses';
import { SalesOrderResponse, SalesOrderDetailResponse } from '../dtos/responses';
import { CreateSalesOrderRequest } from '../dtos/requests';

@Injectable({ providedIn: 'root' })
export class SalesService extends ApiService {
  getAll(
    page = 1,
    pageSize = 20,
    from?: string,
    to?: string,
  ): Observable<ApiResponse<PagedResult<SalesOrderResponse>>> {
    const params = this.buildParams({ page, pageSize, from, to });
    return this.get<ApiResponse<PagedResult<SalesOrderResponse>>>(this.url('sales'), params);
  }

  getById(id: number): Observable<ApiResponse<SalesOrderDetailResponse>> {
    return this.get<ApiResponse<SalesOrderDetailResponse>>(this.url('sales', id));
  }

  create(req: CreateSalesOrderRequest): Observable<ApiResponse<SalesOrderDetailResponse>> {
    return this.post<ApiResponse<SalesOrderDetailResponse>>(this.url('sales'), req);
  }
}
