import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, act } from '@testing-library/react'
import { useBeers } from '@features/sales/hooks/useBeers'
import * as salesApiService from '@features/sales/services/salesApiService'

vi.mock('@features/sales/services/salesApiService')

const mockGetBeers = vi.mocked(salesApiService.getBeers)

describe('useBeers', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('loadsBeers', async () => {
    mockGetBeers.mockResolvedValue({
      page: 0, pageSize: 100, totalRecords: 2,
      results: [
        { beerId: 'b1', beerName: 'Pale Ale', price: { value: 3.5, currency: 'EUR' } },
        { beerId: 'b2', beerName: 'Stout', price: { value: 4.0, currency: 'EUR' } },
      ],
    })
    const { result } = renderHook(() => useBeers())
    await act(async () => {})
    expect(result.current.beers).toHaveLength(2)
    expect(result.current.beers[0].beerName).toBe('Pale Ale')
    expect(result.current.isLoading).toBe(false)
    expect(result.current.error).toBeNull()
  })

  it('showsLoadingState', () => {
    mockGetBeers.mockReturnValue(new Promise(() => {}))
    const { result } = renderHook(() => useBeers())
    expect(result.current.isLoading).toBe(true)
  })

  it('showsErrorOnFailure', async () => {
    mockGetBeers.mockRejectedValue(new Error('Beers load failed'))
    const { result } = renderHook(() => useBeers())
    await act(async () => {})
    expect(result.current.error).toBe('Beers load failed')
    expect(result.current.beers).toHaveLength(0)
    expect(result.current.isLoading).toBe(false)
  })

  it('returnsEmptyArrayOnEmpty', async () => {
    mockGetBeers.mockResolvedValue({ page: 0, pageSize: 100, totalRecords: 0, results: [] })
    const { result } = renderHook(() => useBeers())
    await act(async () => {})
    expect(result.current.beers).toHaveLength(0)
    expect(result.current.error).toBeNull()
  })
})
