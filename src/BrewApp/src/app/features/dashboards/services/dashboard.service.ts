import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../chat/services/chat.service';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { SalesByCustomerJson, SalesByProductsJson } from '../models/dashboard.model';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getSalesByCustomer(pageNumber = 1, pageSize = 10): Observable<PagedResult<SalesByCustomerJson>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<PagedResult<SalesByCustomerJson>>(`${this.baseUrl}/v1/dashboards/customers`, { params });
  }

  getSalesByProducts(pageNumber = 1, pageSize = 10): Observable<PagedResult<SalesByProductsJson>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<PagedResult<SalesByProductsJson>>(`${this.baseUrl}/v1/dashboards/products`, { params });
  }
}
