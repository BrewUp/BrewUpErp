import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../chat/services/chat.service';
import { CreatePurchaseOrderJson } from '../models/purchase-order.model';

@Injectable({ providedIn: 'root' })
export class PurchaseService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  createPurchaseOrder(dto: CreatePurchaseOrderJson): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/v1/purchases`, dto);
  }
}
