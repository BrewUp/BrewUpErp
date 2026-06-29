import { describe, it, expect, beforeEach, vi } from 'vitest'
import { act, renderHook } from '@testing-library/react'
import { ThemeProvider, useThemeValue, useThemeDispatch } from '@shared/theme/ThemeProvider'

function mockMatchMedia(prefersDark: boolean) {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: vi.fn().mockReturnValue({
      matches: prefersDark,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
    }),
  })
}

describe('ThemeProvider', () => {
  beforeEach(() => {
    localStorage.clear()
    document.documentElement.removeAttribute('data-theme')
    mockMatchMedia(false)
  })

  it('rendersLightModeByDefault', () => {
    renderHook(() => useThemeValue(), { wrapper: ThemeProvider })
    expect(document.documentElement.dataset.theme).toBe('light')
  })

  it('readsDarkPreferenceFromLocalStorage', () => {
    localStorage.setItem('brewup-theme', 'dark')
    const { result } = renderHook(() => useThemeValue(), { wrapper: ThemeProvider })
    expect(result.current).toBe('dark')
    expect(document.documentElement.dataset.theme).toBe('dark')
  })

  it('readsLightPreferenceFromLocalStorage', () => {
    mockMatchMedia(true) // OS prefers dark, but stored preference is light
    localStorage.setItem('brewup-theme', 'light')
    const { result } = renderHook(() => useThemeValue(), { wrapper: ThemeProvider })
    expect(result.current).toBe('light')
  })

  it('setsDataThemeAttributeOnMount', () => {
    renderHook(() => useThemeValue(), { wrapper: ThemeProvider })
    expect(document.documentElement.hasAttribute('data-theme')).toBe(true)
  })

  it('honoursPrefersColorSchemeDarkWhenNoStoredPreference', () => {
    mockMatchMedia(true)
    const { result } = renderHook(() => useThemeValue(), { wrapper: ThemeProvider })
    expect(result.current).toBe('dark')
    expect(document.documentElement.dataset.theme).toBe('dark')
  })

  it('togglesToDarkAndPersistsPreference', () => {
    const { result: dispatchResult } = renderHook(() => useThemeDispatch(), {
      wrapper: ThemeProvider,
    })
    act(() => dispatchResult.current())
    expect(localStorage.getItem('brewup-theme')).toBe('dark')
    expect(document.documentElement.dataset.theme).toBe('dark')
  })

  it('togglesBackToLightAfterTwoToggles', () => {
    const { result } = renderHook(() => useThemeDispatch(), { wrapper: ThemeProvider })
    act(() => result.current())
    act(() => result.current())
    expect(document.documentElement.dataset.theme).toBe('light')
    expect(localStorage.getItem('brewup-theme')).toBe('light')
  })

  it('toggleThemeReferenceIsStable', () => {
    const { result, rerender } = renderHook(() => useThemeDispatch(), {
      wrapper: ThemeProvider,
    })
    const first = result.current
    rerender()
    expect(result.current).toBe(first)
  })
})
