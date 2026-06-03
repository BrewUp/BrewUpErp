import { HttpClient } from '@angular/common/http';
import { inject, Injectable, InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { BeerCatalogItem, ChatRequest, ChatResponse } from '../models/chat.model';

export const API_BASE_URL = new InjectionToken<string>('http://localhost:6094/');

@Injectable({ providedIn: 'root' })
export class ChatService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getBeerCatalog(): Observable<BeerCatalogItem[]> {
    return this.http.get<BeerCatalogItem[]>(`${this.baseUrl}/chat/beers`);
  }

  askChat(request: ChatRequest): Observable<ChatResponse> {
    return this.http.post<ChatResponse>(`${this.baseUrl}/chat/sales`, request);
  }
}
