import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../dtos/responses';
import { EmployeeResponse } from '../dtos/responses';
import { UpdateEmployeeRequest } from '../dtos/requests';

@Injectable({ providedIn: 'root' })
export class EmployeeService extends ApiService {
  getAll(): Observable<ApiResponse<EmployeeResponse[]>> {
    return this.get<ApiResponse<EmployeeResponse[]>>(this.url('employees'));
  }

  getById(id: number): Observable<ApiResponse<EmployeeResponse>> {
    return this.get<ApiResponse<EmployeeResponse>>(this.url('employees', id));
  }

  update(id: number, req: UpdateEmployeeRequest): Observable<ApiResponse<EmployeeResponse>> {
    return this.put<ApiResponse<EmployeeResponse>>(this.url('employees', id), req);
  }

  remove(id: number): Observable<ApiResponse<string>> {
    return this.delete<ApiResponse<string>>(this.url('employees', id));
  }
}
