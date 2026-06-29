import { describe, it, expect, beforeAll, afterEach, afterAll } from 'vitest'
import { setupServer } from 'msw/node'
import { http, HttpResponse } from 'msw'
import { getSalesByCustomer, getSalesByProduct } from '@features/dashboard/services/dashboardApiService'
import type { PagedResult, SalesByCustomer, SalesByProduct } from '@features/dashboard/types/dashboard.types'

const BASE_URL = 'http://localhost:6094'

const customerResponse: PagedResult<SalesByCustomer> = {
  page: 0,
  pageSize: 10,
  totalRecords: 2,
  results: [{ customerId: 'c1', customerName: 'Test Customer', year: '2025', totalSales: 100.5, currency: 'EUR' }],
}

const productResponse: PagedResult<SalesByProduct> = {
  page: 0,
  pageSize: 10,
  totalRecords: 1,
  results: [{ productId: 'p1', productName: 'Test Beer', year: '2025', totalSales: 50.0, currency: 'EUR', quantity: 10.0, unitOfMeasure: 'L' }],
}

const server = setupServer(
  http.get(`${BASE_URL}/v1/dashboards/customers`, ({ request }) => {
    const url = new URL(request.url)
    const page = Number(url.searchParams.get('pageNumber') ?? '1') - 1
    return HttpResponse.json({ ...customerResponse, page })
  }),
  http.get(`${BASE_URL}/v1/dashboards/products`, ({ request }) => {
    const url = new URL(request.url)
    const page = Number(url.searchParams.get('pageNumber') ?? '1') - 1
    return HttpResponse.json({ ...productResponse, page })
  }),
)

beforeAll(() => server.listen({ onUnhandledRequest: 'error' }))
afterEach(() => server.resetHandlers())
afterAll(() => server.close())

describe('dashboardApiService', () => {
  describe('getSalesByCustomer', () => {
    it('getSalesByCustomerReturnsPagedData', async () => {
      const result = await getSalesByCustomer(1)
      expect(result.results).toHaveLength(1)
      expect(result.results[0].customerName).toBe('Test Customer')
      expect(result.totalRecords).toBe(2)
    })

    it('getSalesByCustomerPassesPageNumberParam', async () => {
      let capturedPage: string | null = null
      server.use(
        http.get(`${BASE_URL}/v1/dashboards/customers`, ({ request }) => {
          capturedPage = new URL(request.url).searchParams.get('pageNumber')
          return HttpResponse.json(customerResponse)
        }),
      )
      await getSalesByCustomer(3)
      expect(capturedPage).toBe('3')
    })

    it('getSalesByCustomerThrowsOnApiError', async () => {
      server.use(
        http.get(`${BASE_URL}/v1/dashboards/customers`, () =>
          HttpResponse.json({ title: 'Server Error', status: 500 }, { status: 500 })
        ),
      )
      await expect(getSalesByCustomer(1)).rejects.toThrow()
    })
  })

  describe('getSalesByProduct', () => {
    it('getSalesByProductReturnsPagedData', async () => {
      const result = await getSalesByProduct(1)
      expect(result.results).toHaveLength(1)
      expect(result.results[0].productName).toBe('Test Beer')
      expect(result.totalRecords).toBe(1)
    })

    it('getSalesByProductPassesPageNumberParam', async () => {
      let capturedPage: string | null = null
      server.use(
        http.get(`${BASE_URL}/v1/dashboards/products`, ({ request }) => {
          capturedPage = new URL(request.url).searchParams.get('pageNumber')
          return HttpResponse.json(productResponse)
        }),
      )
      await getSalesByProduct(2)
      expect(capturedPage).toBe('2')
    })

    it('getSalesByProductThrowsOnApiError', async () => {
      server.use(
        http.get(`${BASE_URL}/v1/dashboards/products`, () =>
          HttpResponse.json({ title: 'Server Error', status: 500 }, { status: 500 })
        ),
      )
      await expect(getSalesByProduct(1)).rejects.toThrow()
    })
  })
})
