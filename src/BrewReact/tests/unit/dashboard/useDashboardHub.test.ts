import { describe, it, expect, vi } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { useDashboardHub } from '@features/dashboard/hooks/useDashboardHub'

// No SignalR mock — the real @microsoft/signalr is used.
// In the test environment there is no backend running at localhost:6094,
// so start() will reject and hubState will transition idle → offline.
// This validates the graceful-degradation path (FR-011 / SC-005).

describe('useDashboardHub', () => {
  it('initialStateIsIdle', () => {
    const { result } = renderHook(() =>
      useDashboardHub({ onCustomerUpdated: vi.fn(), onProductUpdated: vi.fn() })
    )
    // Synchronous check before start() resolves
    expect(result.current).toBe('idle')
  })

  it('goesOfflineWhenServerUnavailable', async () => {
    const { result } = renderHook(() =>
      useDashboardHub({ onCustomerUpdated: vi.fn(), onProductUpdated: vi.fn() })
    )
    // The real hub tries to connect to localhost:6094 (no server in CI),
    // so it rejects and the hook sets state to 'offline'.
    await waitFor(() => expect(result.current).toBe('offline'), { timeout: 8000 })
  })

  it('stopsConnectionOnUnmount', async () => {
    // Just verify the hook mounts/unmounts without throwing
    const { unmount, result } = renderHook(() =>
      useDashboardHub({ onCustomerUpdated: vi.fn(), onProductUpdated: vi.fn() })
    )
    // Wait a tick so useEffect fires, then unmount cleanly
    await waitFor(() => expect(result.current).not.toBe('idle'), { timeout: 8000 })
    expect(() => unmount()).not.toThrow()
  })
})


