import { describe, it, expect, beforeEach, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ThemeProvider } from '@shared/theme/ThemeProvider'
import { ThemeToggle } from '@shared/theme/ThemeToggle'

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

function renderToggle() {
  return render(
    <ThemeProvider>
      <ThemeToggle />
    </ThemeProvider>,
  )
}

describe('ThemeToggle', () => {
  beforeEach(() => {
    localStorage.clear()
    document.documentElement.removeAttribute('data-theme')
    mockMatchMedia(false)
  })

  it('rendersMoonIconInLightMode', () => {
    renderToggle()
    // In light mode, shows moon icon (to switch to dark)
    // aria-pressed should be false in light mode
    const btn = screen.getByRole('button')
    expect(btn).toHaveAttribute('aria-pressed', 'false')
  })

  it('rendersSunIconInDarkMode', () => {
    localStorage.setItem('brewup-theme', 'dark')
    renderToggle()
    const btn = screen.getByRole('button')
    expect(btn).toHaveAttribute('aria-pressed', 'true')
  })

  it('hasCorrectAriaLabelInLightMode', () => {
    renderToggle()
    expect(screen.getByRole('button', { name: 'Switch to dark mode' })).toBeTruthy()
  })

  it('hasCorrectAriaLabelInDarkMode', () => {
    localStorage.setItem('brewup-theme', 'dark')
    renderToggle()
    expect(screen.getByRole('button', { name: 'Switch to light mode' })).toBeTruthy()
  })

  it('togglesThemeOnClick', async () => {
    const user = userEvent.setup()
    renderToggle()
    const btn = screen.getByRole('button')
    await user.click(btn)
    expect(document.documentElement.dataset.theme).toBe('dark')
    expect(btn).toHaveAttribute('aria-pressed', 'true')
  })

  it('togglesBackToLightOnSecondClick', async () => {
    const user = userEvent.setup()
    renderToggle()
    const btn = screen.getByRole('button')
    await user.click(btn)
    await user.click(btn)
    expect(document.documentElement.dataset.theme).toBe('light')
    expect(btn).toHaveAttribute('aria-pressed', 'false')
  })
})
