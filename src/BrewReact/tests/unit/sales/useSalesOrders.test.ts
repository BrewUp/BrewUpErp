import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, act } from '@testing-library/react'
import { useSalesOrders } from '@features/sales/hooks/useSalesOrders'
import * as salesApiService from '@features/sales/services/salesApiService'
import type { PagedResult, SalesOrder } from '@features/sales/types/sales.types'

vi.mock('@features/sales/services/salesApiService')

const mockGetSalesOrders = vi.mocked(salesApiService.getSalesOrders)

function makePagedResult(page = 0, totalRecords = 12): PagedResult<SalesOrder> {
  return {
    page,
    pageSize: 10,
    totalRecords,
    results: [
      {
        id: `order-${page + 1}`,
        orderNumber: `ORD-00${page + 1}`,
        orderDate: '2025-01-15T10:00:00Z',
        customerId: 'c1',
        customerName: 'Test Customer',
        rows: [],
        status: 'Open',
      },
    ],
  }
}

describe('useSalesOrders', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('loadsFirstPage', async () => {
    mockGetSalesOrders.mockResolvedValue(makePagedResult(0, 2))
    const { result } = renderHook(() => useSalesOrders())
    await act(async () => {})
    expect(result.current.data).toHaveLength(1)
    expect(result.current.data[0].orderNumber).toBe('ORD-001')
    expect(result.current.page).toBe(1)
    expect(result.current.totalRecords).toBe(2)
  })

  it('showsLoadingState', () => {
    mockGetSalesOrders.mockReturnValue(new Promise(() => {}))
    const { result } = renderHook(() => useSalesOrders())
    expect(result.current.isLoading).toBe(true)
  })

  it('showsErrorOnFailure', async () => {
    mockGetSalesOrders.mockRejectedValue(new Error('Load failed'))
    const { result } = renderHook(() => useSalesOrders())
    await act(async () => {})
    expect(result.current.error).toBe('Load failed')
    expect(result.current.data).toHaveLength(0)
    expect(result.current.isLoading).toBe(false)
  })

  it('goToNextPage', async () => {
    mockGetSalesOrders.mockResolvedValue(makePagedResult(0, 12))
    const { result } = renderHook(() => useSalesOrders())
    await act(async () => {})
    expect(result.current.page).toBe(1)
    await act(async () => {
      result.current.goToNextPage()
    })
    expect(result.current.page).toBe(2)
    expect(mockGetSalesOrders).toHaveBeenCalledWith(2)
  })

  it('goToPrevPage', async () => {
    mockGetSalesOrders.mockResolvedValue(makePagedResult(1, 12))
    const { result } = renderHook(() => useSalesOrders())
    await act(async () => {})
    await act(async () => { result.current.goToNextPage() })
    await act(async () => { result.current.goToPrevPage() })
    expect(result.current.page).toBe(1)
  })

  it('disablesPrevOnFirstPage', async () => {
    mockGetSalesOrders.mockResolvedValue(makePagedResult(0, 2))
    const { result } = renderHook(() => useSalesOrders())
    await act(async () => {})
    const prevPage = result.current.page
    await act(async () => { result.current.goToPrevPage() })
    expect(result.current.page).toBe(prevPage) // stays at 1
  })

  it('disablesNextOnLastPage', async () => {
    // totalRecords=5, pageSize=10 → 1 page only → page * pageSize >= totalRecords
    mockGetSalesOrders.mockResolvedValue({ page: 0, pageSize: 10, totalRecords: 5, results: [] })
    const { result } = renderHook(() => useSalesOrders())
    await act(async () => {})
    expect(result.current.page * result.current.pageSize >= result.current.totalRecords).toBe(true)
  })
})
