import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../dtos/responses';
import { SupplierResponse } from '../dtos/responses';
import { SupplierRequest } from '../dtos/requests';

@Injectable({ providedIn: 'root' })
export class SupplierService extends ApiService {
  getAll(search?: string): Observable<ApiResponse<SupplierResponse[]>> {
    const params = this.buildParams({ search });
    return this.get<ApiResponse<SupplierResponse[]>>(this.url('suppliers'), params);
  }

  getById(id: number): Observable<ApiResponse<SupplierResponse>> {
    return this.get<ApiResponse<SupplierResponse>>(this.url('suppliers', id));
  }

  create(req: SupplierRequest): Observable<ApiResponse<SupplierResponse>> {
    return this.post<ApiResponse<SupplierResponse>>(this.url('suppliers'), req);
  }

  update(id: number, req: SupplierRequest): Observable<ApiResponse<SupplierResponse>> {
    return this.put<ApiResponse<SupplierResponse>>(this.url('suppliers', id), req);
  }

  remove(id: number): Observable<ApiResponse<string>> {
    return this.delete<ApiResponse<string>>(this.url('suppliers', id));
  }
}
