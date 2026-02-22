import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResult } from '../dtos/responses';
import { InventoryBatchResponse, AlertsResponse } from '../dtos/responses';
import { StockInRequest, StockAdjustRequest, StockOutFefoRequest } from '../dtos/requests';

@Injectable({ providedIn: 'root' })
export class InventoryService extends ApiService {
  getAll(
    drugId?: number,
    supplierId?: number,
    page = 1,
    pageSize = 20,
  ): Observable<ApiResponse<PagedResult<InventoryBatchResponse>>> {
    const params = this.buildParams({ page, pageSize, drugId, supplierId });
    return this.get<ApiResponse<PagedResult<InventoryBatchResponse>>>(
      this.url('inventory'),
      params,
    );
  }

  getById(id: number): Observable<ApiResponse<InventoryBatchResponse>> {
    return this.get<ApiResponse<InventoryBatchResponse>>(this.url('inventory', id));
  }

  stockIn(req: StockInRequest): Observable<ApiResponse<InventoryBatchResponse>> {
    return this.post<ApiResponse<InventoryBatchResponse>>(this.url('inventory', 'stock-in'), req);
  }

  stockOutFefo(req: StockOutFefoRequest): Observable<ApiResponse<string>> {
    return this.post<ApiResponse<string>>(this.url('inventory', 'stock-out-fefo'), req);
  }

  adjust(req: StockAdjustRequest): Observable<ApiResponse<string>> {
    return this.post<ApiResponse<string>>(this.url('inventory', 'adjust'), req);
  }

  getAlerts(): Observable<ApiResponse<AlertsResponse>> {
    return this.get<ApiResponse<AlertsResponse>>(this.url('inventory', 'alerts'));
  }
}
