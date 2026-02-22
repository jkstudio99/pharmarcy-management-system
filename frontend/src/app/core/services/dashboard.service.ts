import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../dtos/responses';
import { DashboardSummaryResponse } from '../dtos/responses';

@Injectable({ providedIn: 'root' })
export class DashboardService extends ApiService {
  getSummary(): Observable<ApiResponse<DashboardSummaryResponse>> {
    return this.get<ApiResponse<DashboardSummaryResponse>>(this.url('dashboard'));
  }
}
