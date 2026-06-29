import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, act, waitFor } from '@testing-library/react'
import { useSalesByProduct } from '@features/dashboard/hooks/useSalesByProduct'
import * as dashboardApiService from '@features/dashboard/services/dashboardApiService'
import type { PagedResult, SalesByProduct } from '@features/dashboard/types/dashboard.types'

vi.mock('@features/dashboard/services/dashboardApiService')

const mockProductPage: PagedResult<SalesByProduct> = {
  page: 0,
  pageSize: 10,
  totalRecords: 15,
  results: [{ productId: 'p1', productName: 'Test Beer', year: '2025', totalSales: 50.0, currency: 'EUR', quantity: 10.0, unitOfMeasure: 'L' }],
}

const emptyPage: PagedResult<SalesByProduct> = {
  page: 0,
  pageSize: 10,
  totalRecords: 0,
  results: [],
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useSalesByProduct', () => {
  it('initialStateIsLoadingWithNoData', async () => {
    vi.mocked(dashboardApiService.getSalesByProduct).mockResolvedValue(mockProductPage)
    const { result } = renderHook(() => useSalesByProduct(0))
    expect(result.current.isLoading).toBe(true)
    expect(result.current.data).toEqual([])
    await waitFor(() => expect(result.current.isLoading).toBe(false))
  })

  it('loadsProductDataOnMount', async () => {
    vi.mocked(dashboardApiService.getSalesByProduct).mockResolvedValue(mockProductPage)
    const { result } = renderHook(() => useSalesByProduct(0))
    await waitFor(() => expect(result.current.isLoading).toBe(false))
    expect(result.current.data).toHaveLength(1)
    expect(result.current.data[0].productName).toBe('Test Beer')
    expect(result.current.totalRecords).toBe(15)
  })

  it('updatesPageOnNextPage', async () => {
    vi.mocked(dashboardApiService.getSalesByProduct).mockResolvedValue(mockProductPage)
    const { result } = renderHook(() => useSalesByProduct(0))
    await waitFor(() => expect(result.current.isLoading).toBe(false))
    act(() => { result.current.goToNextPage() })
    expect(result.current.page).toBe(2)
    await waitFor(() => expect(dashboardApiService.getSalesByProduct).toHaveBeenCalledWith(2))
  })

  it('updatesPageOnPrevPage', async () => {
    vi.mocked(dashboardApiService.getSalesByProduct).mockResolvedValue(mockProductPage)
    const { result } = renderHook(() => useSalesByProduct(0))
    await waitFor(() => expect(result.current.isLoading).toBe(false))
    act(() => { result.current.goToNextPage() })
    await waitFor(() => expect(result.current.page).toBe(2))
    act(() => { result.current.goToPrevPage() })
    expect(result.current.page).toBe(1)
  })

  it('setsErrorOnApiFail', async () => {
    vi.mocked(dashboardApiService.getSalesByProduct).mockRejectedValue(new Error('API error'))
    const { result } = renderHook(() => useSalesByProduct(0))
    await waitFor(() => expect(result.current.isLoading).toBe(false))
    expect(result.current.error).toBe('API error')
  })

  it('showsEmptyStateWhenNoResults', async () => {
    vi.mocked(dashboardApiService.getSalesByProduct).mockResolvedValue(emptyPage)
    const { result } = renderHook(() => useSalesByProduct(0))
    await waitFor(() => expect(result.current.isLoading).toBe(false))
    expect(result.current.data).toEqual([])
    expect(result.current.totalRecords).toBe(0)
  })

  it('refreshesDataWhenRefreshTokenChanges', async () => {
    vi.mocked(dashboardApiService.getSalesByProduct).mockResolvedValue(mockProductPage)
    const { result, rerender } = renderHook(({ token }) => useSalesByProduct(token), {
      initialProps: { token: 0 },
    })
    await waitFor(() => expect(result.current.isLoading).toBe(false))
    const callCount = vi.mocked(dashboardApiService.getSalesByProduct).mock.calls.length
    rerender({ token: 1 })
    await waitFor(() =>
      expect(vi.mocked(dashboardApiService.getSalesByProduct).mock.calls.length).toBeGreaterThan(callCount)
    )
  })
})
