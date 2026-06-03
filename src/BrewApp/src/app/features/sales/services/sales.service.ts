import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../chat/services/chat.service';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { AddBeersToCartJson, CreateSalesOrderJson, SalesOrderJson } from '../models/sales-order.model';

@Injectable({ providedIn: 'root' })
export class SalesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getSalesOrders(pageNumber = 1, pageSize = 10): Observable<PagedResult<SalesOrderJson>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<PagedResult<SalesOrderJson>>(`${this.baseUrl}/v1/sales`, { params });
  }

  getSalesOrderById(id: string): Observable<SalesOrderJson> {
    return this.http.get<SalesOrderJson>(`${this.baseUrl}/v1/sales/${id}`);
  }

  createSalesOrder(dto: CreateSalesOrderJson): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/v1/sales`, dto);
  }

  closeSalesOrder(orderId: string): Observable<unknown> {
    return this.http.put(`${this.baseUrl}/v1/sales/${orderId}`, null);
  }

  addBeersToOrder(orderId: string, dto: AddBeersToCartJson): Observable<unknown> {
    return this.http.patch(`${this.baseUrl}/v1/sales/rows/${orderId}`, dto);
  }
}
