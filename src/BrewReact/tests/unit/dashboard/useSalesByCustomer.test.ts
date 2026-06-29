import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, act, waitFor } from '@testing-library/react'
import { useSalesByCustomer } from '@features/dashboard/hooks/useSalesByCustomer'
import * as dashboardApiService from '@features/dashboard/services/dashboardApiService'
import type { PagedResult, SalesByCustomer } from '@features/dashboard/types/dashboard.types'

vi.mock('@features/dashboard/services/dashboardApiService')

const mockCustomerPage: PagedResult<SalesByCustomer> = {
  page: 0,
  pageSize: 10,
  totalRecords: 15,
  results: [{ customerId: 'c1', customerName: 'Test Customer', year: '2025', totalSales: 100.5, currency: 'EUR' }],
}

const emptyPage: PagedResult<SalesByCustomer> = {
  page: 0,
  pageSize: 10,
  totalRecords: 0,
  results: [],
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useSalesByCustomer', () => {
  it('initialStateIsLoadingWithNoData', async () => {
    vi.mocked(dashboardApiService.getSalesByCustomer).mockResolvedValue(mockCustomerPage)
    const { result } = renderHook(() => useSalesByCustomer(0))
    expect(result.current.isLoading).toBe(true)
    expect(result.current.data).toEqual([])
    await waitFor(() => expect(result.current.isLoading).toBe(false))
  })

  it('loadsCustomerDataOnMount', async () => {
    vi.mocked(dashboardApiService.getSalesByCustomer).mockResolvedValue(mockCustomerPage)
    const { result } = renderHook(() => useSalesByCustomer(0))
    await waitFor(() => expect(result.current.isLoading).toBe(false))
    expect(result.current.data).toHaveLength(1)
    expect(result.current.data[0].customerName).toBe('Test Customer')
    expect(result.current.totalRecords).toBe(15)
  })

  it('updatesPageOnNextPage', async () => {
    vi.mocked(dashboardApiService.getSalesByCustomer).mockResolvedValue(mockCustomerPage)
    const { result } = renderHook(() => useSalesByCustomer(0))
    await waitFor(() => expect(result.current.isLoading).toBe(false))
    act(() => { result.current.goToNextPage() })
    expect(result.current.page).toBe(2)
    await waitFor(() => expect(dashboardApiService.getSalesByCustomer).toHaveBeenCalledWith(2))
  })

  it('updatesPageOnPrevPage', async () => {
    vi.mocked(dashboardApiService.getSalesByCustomer).mockResolvedValue(mockCustomerPage)
    const { result } = renderHook(() => useSalesByCustomer(0))
    await waitFor(() => expect(result.current.isLoading).toBe(false))
    act(() => { result.current.goToNextPage() })
    await waitFor(() => expect(result.current.page).toBe(2))
    act(() => { result.current.goToPrevPage() })
    expect(result.current.page).toBe(1)
  })

  it('setsErrorOnApiFail', async () => {
    vi.mocked(dashboardApiService.getSalesByCustomer).mockRejectedValue(new Error('API error'))
    const { result } = renderHook(() => useSalesByCustomer(0))
    await waitFor(() => expect(result.current.isLoading).toBe(false))
    expect(result.current.error).toBe('API error')
  })

  it('showsEmptyStateWhenNoResults', async () => {
    vi.mocked(dashboardApiService.getSalesByCustomer).mockResolvedValue(emptyPage)
    const { result } = renderHook(() => useSalesByCustomer(0))
    await waitFor(() => expect(result.current.isLoading).toBe(false))
    expect(result.current.data).toEqual([])
    expect(result.current.totalRecords).toBe(0)
  })

  it('refreshesDataWhenRefreshTokenChanges', async () => {
    vi.mocked(dashboardApiService.getSalesByCustomer).mockResolvedValue(mockCustomerPage)
    const { result, rerender } = renderHook(({ token }) => useSalesByCustomer(token), {
      initialProps: { token: 0 },
    })
    await waitFor(() => expect(result.current.isLoading).toBe(false))
    const callCount = vi.mocked(dashboardApiService.getSalesByCustomer).mock.calls.length
    rerender({ token: 1 })
    await waitFor(() =>
      expect(vi.mocked(dashboardApiService.getSalesByCustomer).mock.calls.length).toBeGreaterThan(callCount)
    )
  })
})
