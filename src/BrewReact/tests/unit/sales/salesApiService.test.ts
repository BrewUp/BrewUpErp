import { describe, it, expect, beforeAll, afterEach, afterAll } from 'vitest'
import { setupServer } from 'msw/node'
import { http, HttpResponse } from 'msw'
import { getSalesOrders, createSalesOrder, getCustomers, getBeers } from '@features/sales/services/salesApiService'
import type { CreateSalesOrderPayload } from '@features/sales/types/sales.types'

const BASE_URL = 'http://localhost:6094'

const server = setupServer(
  http.get(`${BASE_URL}/v1/sales`, ({ request }) => {
    const url = new URL(request.url)
    const pageNumber = Number(url.searchParams.get('pageNumber') ?? '1')
    return HttpResponse.json({
      page: pageNumber - 1,
      pageSize: 10,
      totalRecords: 2,
      results: [{ id: 'o1', orderNumber: 'ORD-001', orderDate: '2025-01-15T10:00:00Z', customerId: 'c1', customerName: 'Test Co', rows: [], status: 'Open' }],
    })
  }),
  http.post(`${BASE_URL}/v1/sales`, () =>
    new HttpResponse(null, { status: 201, headers: { Location: '/v1/sales/o-new' } })
  ),
  http.get(`${BASE_URL}/v1/masterdata/customers`, () =>
    HttpResponse.json({
      page: 0, pageSize: 100, totalRecords: 1,
      results: [{ customerId: 'c1', ragioneSociale: 'Test Co' }],
    })
  ),
  http.get(`${BASE_URL}/v1/masterdata/beers`, () =>
    HttpResponse.json({
      page: 0, pageSize: 100, totalRecords: 1,
      results: [{ beerId: 'b1', beerName: 'Pale Ale', price: { value: 3.5, currency: 'EUR' } }],
    })
  ),
  http.get(`${BASE_URL}/v1/sales/error`, () =>
    HttpResponse.json({ title: 'Server Error', detail: 'Something went wrong', status: 500 }, { status: 500 })
  ),
)

beforeAll(() => server.listen())
afterEach(() => server.resetHandlers())
afterAll(() => server.close())

describe('salesApiService', () => {
  it('getSalesOrdersReturnsPagedResult', async () => {
    const result = await getSalesOrders(1)
    expect(result.results).toHaveLength(1)
    expect(result.results[0].orderNumber).toBe('ORD-001')
    expect(result.page).toBe(0)
  })

  it('getSalesOrdersPassesPageNumber', async () => {
    const result = await getSalesOrders(2)
    expect(result.page).toBe(1)
  })

  it('getSalesOrdersThrowsOnError', async () => {
    server.use(
      http.get(`${BASE_URL}/v1/sales`, () =>
        HttpResponse.json({ detail: 'Load failed', status: 500 }, { status: 500 })
      )
    )
    await expect(getSalesOrders(1)).rejects.toThrow('Load failed')
  })

  it('createSalesOrderSucceeds', async () => {
    const payload: CreateSalesOrderPayload = {
      orderNumber: 'ORD-001',
      orderDate: '2025-01-15T10:00:00Z',
      customerId: 'c1',
      customerName: 'Test Co',
      rows: [{ beerId: 'b1', beerName: 'Pale Ale', quantity: { value: 10, unitOfMeasure: 'L' }, price: { value: 3.5, currency: 'EUR' } }],
    }
    await expect(createSalesOrder(payload)).resolves.toBeUndefined()
  })

  it('createSalesOrderThrowsOnError', async () => {
    server.use(
      http.post(`${BASE_URL}/v1/sales`, () =>
        HttpResponse.json({ detail: 'Create failed', status: 500 }, { status: 500 })
      )
    )
    const payload: CreateSalesOrderPayload = {
      orderNumber: 'X', orderDate: '2025-01-15T10:00:00Z', customerId: 'c1', customerName: 'Co', rows: [],
    }
    await expect(createSalesOrder(payload)).rejects.toThrow('Create failed')
  })

  it('getCustomersReturnsResults', async () => {
    const result = await getCustomers()
    expect(result.results).toHaveLength(1)
    expect(result.results[0].ragioneSociale).toBe('Test Co')
  })

  it('getBeersReturnsResults', async () => {
    const result = await getBeers()
    expect(result.results).toHaveLength(1)
    expect(result.results[0].beerName).toBe('Pale Ale')
    expect(result.results[0].price.value).toBe(3.5)
  })

  it('getBeersThrowsOnError', async () => {
    server.use(
      http.get(`${BASE_URL}/v1/masterdata/beers`, () =>
        HttpResponse.json({ title: 'Beer load failed', status: 500 }, { status: 500 })
      )
    )
    await expect(getBeers()).rejects.toThrow('Beer load failed')
  })
})
