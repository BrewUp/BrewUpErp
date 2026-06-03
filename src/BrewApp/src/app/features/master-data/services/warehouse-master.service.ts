import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../chat/services/chat.service';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { WarehouseJson, CreateWarehouseJson } from '../models/warehouse.model';

@Injectable({ providedIn: 'root' })
export class WarehouseMasterService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  // ⚠️ Warehouse uses 0-based 'page' param (not 'pageNumber')
  getWarehouses(page = 0, pageSize = 10): Observable<PagedResult<WarehouseJson>> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    return this.http.get<PagedResult<WarehouseJson>>(`${this.baseUrl}/v1/masterdata/warehouse`, { params });
  }

  createWarehouse(dto: CreateWarehouseJson): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/v1/masterdata/warehouse`, dto);
  }
}
