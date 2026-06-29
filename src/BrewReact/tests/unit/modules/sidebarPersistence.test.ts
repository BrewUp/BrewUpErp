import { describe, it, expect, beforeEach } from 'vitest'
import { act, renderHook } from '@testing-library/react'
import { useSidebarCollapsed } from '@shared/sidebar/useSidebarCollapsed'

const STORAGE_KEY = 'brewup-sidebar-collapsed'

describe('useSidebarCollapsed — US2: Collapse Persistence', () => {
  beforeEach(() => {
    localStorage.clear()
  })

  it('readsFalseWhenKeyMissing', () => {
    const { result } = renderHook(() => useSidebarCollapsed())
    expect(result.current[0]).toBe(false)
  })

  it('readsTrueWhenKeyIsTrue', () => {
    localStorage.setItem(STORAGE_KEY, 'true')
    const { result } = renderHook(() => useSidebarCollapsed())
    expect(result.current[0]).toBe(true)
  })

  it('readsFalseForAnyNonTrueValue', () => {
    for (const val of ['false', '1', 'yes', '', 'True']) {
      localStorage.setItem(STORAGE_KEY, val)
      const { result } = renderHook(() => useSidebarCollapsed())
      expect(result.current[0]).toBe(false)
    }
  })

  it('writesToLocalStorageOnToggle', () => {
    const { result } = renderHook(() => useSidebarCollapsed())
    act(() => result.current[1]())
    expect(localStorage.getItem(STORAGE_KEY)).toBe('true')
  })

  it('restoredCollapsedStateOnMount', () => {
    localStorage.setItem(STORAGE_KEY, 'true')
    const { result } = renderHook(() => useSidebarCollapsed())
    expect(result.current[0]).toBe(true)
    act(() => result.current[1]())
    expect(result.current[0]).toBe(false)
    expect(localStorage.getItem(STORAGE_KEY)).toBe('false')
  })
})
