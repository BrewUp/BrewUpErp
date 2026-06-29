import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, act } from '@testing-library/react'
import { useCustomers } from '@features/sales/hooks/useCustomers'
import * as salesApiService from '@features/sales/services/salesApiService'

vi.mock('@features/sales/services/salesApiService')

const mockGetCustomers = vi.mocked(salesApiService.getCustomers)

describe('useCustomers', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('loadsCustomers', async () => {
    mockGetCustomers.mockResolvedValue({
      page: 0, pageSize: 100, totalRecords: 2,
      results: [
        { customerId: 'c1', ragioneSociale: 'Birrificio Rossi SRL' },
        { customerId: 'c2', ragioneSociale: 'Brasserie Dupont' },
      ],
    })
    const { result } = renderHook(() => useCustomers())
    await act(async () => {})
    expect(result.current.customers).toHaveLength(2)
    expect(result.current.customers[0].ragioneSociale).toBe('Birrificio Rossi SRL')
    expect(result.current.isLoading).toBe(false)
    expect(result.current.error).toBeNull()
  })

  it('showsLoadingState', () => {
    mockGetCustomers.mockReturnValue(new Promise(() => {}))
    const { result } = renderHook(() => useCustomers())
    expect(result.current.isLoading).toBe(true)
  })

  it('showsErrorOnFailure', async () => {
    mockGetCustomers.mockRejectedValue(new Error('Customers load failed'))
    const { result } = renderHook(() => useCustomers())
    await act(async () => {})
    expect(result.current.error).toBe('Customers load failed')
    expect(result.current.customers).toHaveLength(0)
    expect(result.current.isLoading).toBe(false)
  })

  it('returnsEmptyArrayOnEmpty', async () => {
    mockGetCustomers.mockResolvedValue({ page: 0, pageSize: 100, totalRecords: 0, results: [] })
    const { result } = renderHook(() => useCustomers())
    await act(async () => {})
    expect(result.current.customers).toHaveLength(0)
    expect(result.current.error).toBeNull()
  })
})
