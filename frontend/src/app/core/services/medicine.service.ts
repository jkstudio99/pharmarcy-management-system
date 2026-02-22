import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResult } from '../dtos/responses';
import { MedicineResponse, MedicineDetailResponse } from '../dtos/responses';
import { MedicineRequest } from '../dtos/requests';

@Injectable({ providedIn: 'root' })
export class MedicineService extends ApiService {
  getAll(
    search?: string,
    categoryId?: number,
    page = 1,
    pageSize = 20,
  ): Observable<ApiResponse<PagedResult<MedicineResponse>>> {
    const params = this.buildParams({ page, pageSize, search, categoryId });
    return this.get<ApiResponse<PagedResult<MedicineResponse>>>(this.url('medicines'), params);
  }

  getById(id: number): Observable<ApiResponse<MedicineDetailResponse>> {
    return this.get<ApiResponse<MedicineDetailResponse>>(this.url('medicines', id));
  }

  create(req: MedicineRequest): Observable<ApiResponse<MedicineResponse>> {
    return this.post<ApiResponse<MedicineResponse>>(this.url('medicines'), req);
  }

  update(id: number, req: MedicineRequest): Observable<ApiResponse<MedicineResponse>> {
    return this.put<ApiResponse<MedicineResponse>>(this.url('medicines', id), req);
  }

  remove(id: number): Observable<ApiResponse<string>> {
    return this.delete<ApiResponse<string>>(this.url('medicines', id));
  }

  uploadImage(id: number, file: File): Observable<ApiResponse<string>> {
    const fd = new FormData();
    fd.append('file', file);
    return this.postForm<ApiResponse<string>>(this.url('medicines', id, 'image'), fd);
  }
}
