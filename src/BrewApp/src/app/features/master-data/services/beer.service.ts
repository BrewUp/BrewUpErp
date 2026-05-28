import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../chat/services/chat.service';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { BeerJson, CreateBeerJson } from '../models/beer.model';

@Injectable({ providedIn: 'root' })
export class BeerService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getBeers(pageNumber = 1, pageSize = 10): Observable<PagedResult<BeerJson>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<PagedResult<BeerJson>>(`${this.baseUrl}/v1/masterdata/beers`, { params });
  }

  createBeer(dto: CreateBeerJson): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/v1/masterdata/beers`, dto);
  }
}
