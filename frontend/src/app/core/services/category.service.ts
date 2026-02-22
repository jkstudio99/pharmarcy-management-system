import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../dtos/responses';
import { CategoryResponse } from '../dtos/responses';
import { CategoryRequest } from '../dtos/requests';

@Injectable({ providedIn: 'root' })
export class CategoryService extends ApiService {
  getAll(page = 1, pageSize = 100): Observable<ApiResponse<CategoryResponse[]>> {
    const params = this.buildParams({ page, pageSize });
    return this.get<ApiResponse<CategoryResponse[]>>(this.url('categories'), params);
  }

  create(req: CategoryRequest): Observable<ApiResponse<CategoryResponse>> {
    return this.post<ApiResponse<CategoryResponse>>(this.url('categories'), req);
  }

  update(id: number, req: CategoryRequest): Observable<ApiResponse<CategoryResponse>> {
    return this.put<ApiResponse<CategoryResponse>>(this.url('categories', id), req);
  }

  remove(id: number): Observable<ApiResponse<string>> {
    return this.delete<ApiResponse<string>>(this.url('categories', id));
  }
}
