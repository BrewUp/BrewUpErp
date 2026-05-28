import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../chat/services/chat.service';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { CustomerJson, CreateCustomerJson, EditCustomerJson } from '../models/customer.model';

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getCustomers(pageNumber = 1, pageSize = 10): Observable<PagedResult<CustomerJson>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<PagedResult<CustomerJson>>(`${this.baseUrl}/v1/masterdata/customers`, { params });
  }

  getCustomerById(id: string): Observable<CustomerJson> {
    return this.http.get<CustomerJson>(`${this.baseUrl}/v1/masterdata/customers/${id}`);
  }

  createCustomer(dto: CreateCustomerJson): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/v1/masterdata/customers`, dto);
  }

  updateCustomer(id: string, dto: EditCustomerJson): Observable<unknown> {
    return this.http.put(`${this.baseUrl}/v1/masterdata/customers/${id}`, dto);
  }

  deleteCustomer(id: string): Observable<unknown> {
    return this.http.delete(`${this.baseUrl}/v1/masterdata/customers/${id}`);
  }
}
