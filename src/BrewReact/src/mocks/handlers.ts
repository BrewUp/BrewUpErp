import { http, HttpResponse } from 'msw'
import type { BeerCatalogItem, ChatRequest, ChatResponse } from '@features/chat/types/chat.types'
import type { PagedResult, SalesByCustomer, SalesByProduct } from '@features/dashboard/types/dashboard.types'
import type { SalesOrder, Customer, Beer } from '@features/sales/types/sales.types'

const BASE_URL = 'http://localhost:6094'

export const handlers = [
  http.get(`${BASE_URL}/chat/beers`, () => {
    return HttpResponse.json<BeerCatalogItem[]>([
      {
        beerId: 'beer-1',
        name: 'Pale Ale',
        style: 'American Pale Ale',
        alcoholByVolume: 5.5,
        isActive: true,
      },
      {
        beerId: 'beer-2',
        name: 'Stout',
        style: 'Irish Stout',
        alcoholByVolume: null,
        isActive: false,
      },
    ])
  }),

  http.post(`${BASE_URL}/chat/`, async ({ request }) => {
    const body = await request.json() as ChatRequest
    return HttpResponse.json<ChatResponse>({
      answer: `Echo: ${body.message}`,
      conversationId: 'mock-conversation-id',
    })
  }),

  http.get(`${BASE_URL}/v1/dashboards/customers`, ({ request }) => {
    const url = new URL(request.url)
    const page = Number(url.searchParams.get('pageNumber') ?? '1') - 1
    return HttpResponse.json<PagedResult<SalesByCustomer>>({
      page,
      pageSize: 10,
      totalRecords: 2,
      results: [
        { customerId: 'c1', customerName: 'Test Customer', year: '2025', totalSales: 100.5, currency: 'EUR' },
      ],
    })
  }),

  http.get(`${BASE_URL}/v1/sales`, ({ request }) => {
    const url = new URL(request.url)
    const page = Number(url.searchParams.get('pageNumber') ?? '1') - 1
    return HttpResponse.json<PagedResult<SalesOrder>>({
      page,
      pageSize: 10,
      totalRecords: 12,
      results: [
        {
          id: 'order-1',
          orderNumber: 'ORD-2025-001',
          orderDate: '2025-01-15T10:00:00Z',
          deliveryDate: '2025-01-20T10:00:00Z',
          customerId: 'customer-1',
          customerName: 'Birrificio Rossi SRL',
          rows: [],
          status: 'Open',
        },
        {
          id: 'order-2',
          orderNumber: 'ORD-2025-002',
          orderDate: '2025-01-10T09:00:00Z',
          deliveryDate: undefined,
          customerId: 'customer-2',
          customerName: 'Brasserie Dupont',
          rows: [],
          status: 'Closed',
        },
      ],
    })
  }),

  http.post(`${BASE_URL}/v1/sales`, () =>
    new HttpResponse(null, {
      status: 201,
      headers: { Location: '/v1/sales/order-new' },
    })
  ),

  http.get(`${BASE_URL}/v1/masterdata/customers`, () =>
    HttpResponse.json<PagedResult<Customer>>({
      page: 0,
      pageSize: 100,
      totalRecords: 2,
      results: [
        { customerId: 'customer-1', ragioneSociale: 'Birrificio Rossi SRL' },
        { customerId: 'customer-2', ragioneSociale: 'Brasserie Dupont' },
      ],
    })
  ),

  http.get(`${BASE_URL}/v1/masterdata/beers`, () =>
    HttpResponse.json<PagedResult<Beer>>({
      page: 0,
      pageSize: 100,
      totalRecords: 2,
      results: [
        { beerId: 'beer-1', beerName: 'Pale Ale', price: { value: 3.50, currency: 'EUR' } },
        { beerId: 'beer-2', beerName: 'Stout', price: { value: 4.00, currency: 'EUR' } },
      ],
    })
  ),

  http.get(`${BASE_URL}/v1/dashboards/products`, () =>
    HttpResponse.json<PagedResult<SalesByProduct>>({
      page: 0,
      pageSize: 10,
      totalRecords: 1,
      results: [
        { productId: 'p1', productName: 'Test Beer', year: '2025', totalSales: 50.0, currency: 'EUR', quantity: 10.0, unitOfMeasure: 'L' },
      ],
    })
  ),
]
