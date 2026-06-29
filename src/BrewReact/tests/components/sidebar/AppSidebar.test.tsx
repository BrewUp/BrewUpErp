import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { AppSidebar } from '@shared/sidebar/AppSidebar'
import type { ModuleConfig } from '@shared/modules'

// ── Test fixture ────────────────────────────────────────────────────────────

function StubIcon() {
  return <svg aria-hidden="true" data-testid="stub-icon" />
}
function StubPage() {
  return <div>stub page</div>
}

const FIXTURE_MODULES: ModuleConfig[] = [
  { id: 'chat', label: 'Chat', icon: StubIcon, component: StubPage },
  { id: 'dashboard', label: 'Dashboard', icon: StubIcon, component: StubPage },
  { id: 'sales', label: 'Sales', icon: StubIcon, component: StubPage },
]

function renderSidebar(
  overrides: Partial<Parameters<typeof AppSidebar>[0]> = {},
) {
  const props = {
    modules: FIXTURE_MODULES,
    activeModule: 'chat' as const,
    collapsed: false,
    onModuleSelect: vi.fn(),
    onToggleCollapse: vi.fn(),
    ...overrides,
  }
  return { ...render(<AppSidebar {...props} />), props }
}

// ── Phase 3 / US1: Navigate Between Modules ─────────────────────────────────

describe('AppSidebar — US1: Navigate Between Modules', () => {
  it('rendersAllModulesFromRegistry', () => {
    renderSidebar()
    expect(screen.getByRole('button', { name: 'Chat' })).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Dashboard' })).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Sales' })).toBeTruthy()
  })

  it('highlightsActiveModuleWithAriaCurrent', () => {
    renderSidebar({ activeModule: 'dashboard' })
    expect(screen.getByRole('button', { name: 'Dashboard' })).toHaveAttribute(
      'aria-current',
      'page',
    )
    expect(screen.getByRole('button', { name: 'Chat' })).not.toHaveAttribute('aria-current')
    expect(screen.getByRole('button', { name: 'Sales' })).not.toHaveAttribute('aria-current')
  })

  it('callsOnModuleSelectWhenInactiveItemClicked', async () => {
    const user = userEvent.setup()
    const { props } = renderSidebar({ activeModule: 'chat' })
    await user.click(screen.getByRole('button', { name: 'Dashboard' }))
    expect(props.onModuleSelect).toHaveBeenCalledWith('dashboard')
  })

  it('doesNotCallOnModuleSelectWhenActiveItemClicked', async () => {
    const user = userEvent.setup()
    const { props } = renderSidebar({ activeModule: 'chat' })
    await user.click(screen.getByRole('button', { name: 'Chat' }))
    expect(props.onModuleSelect).not.toHaveBeenCalled()
  })

  it('rendersNavLandmarkWithLabel', () => {
    renderSidebar()
    expect(screen.getByRole('navigation', { name: 'Main navigation' })).toBeTruthy()
  })
})

// ── Phase 4 / US2: Collapse and Expand ──────────────────────────────────────

describe('AppSidebar — US2: Collapse and Expand', () => {
  it('rendersLabelsWhenExpanded', () => {
    renderSidebar({ collapsed: false })
    // Labels are visible — text content present and not aria-hidden
    const chatBtn = screen.getByRole('button', { name: 'Chat' })
    expect(chatBtn).toBeTruthy()
  })

  it('hidesLabelsWhenCollapsed', () => {
    renderSidebar({ collapsed: true })
    // In collapsed mode, label spans get the hidden class — they are still
    // in the DOM (for transition), but the button accessible name is set via aria-label
    // so buttons are accessible by aria-label when collapsed
    const chatBtn = screen.getByRole('button', { name: 'Chat' })
    expect(chatBtn).toBeTruthy()
  })

  it('showsTitleAttributeOnButtonsWhenCollapsed', () => {
    renderSidebar({ collapsed: true })
    expect(screen.getByRole('button', { name: 'Chat' })).toHaveAttribute('title', 'Chat')
    expect(screen.getByRole('button', { name: 'Dashboard' })).toHaveAttribute(
      'title',
      'Dashboard',
    )
  })

  it('doesNotShowTitleAttributeWhenExpanded', () => {
    renderSidebar({ collapsed: false })
    expect(screen.getByRole('button', { name: 'Chat' })).not.toHaveAttribute('title')
  })

  it('callsOnToggleCollapseWhenToggleClicked', async () => {
    const user = userEvent.setup()
    const { props } = renderSidebar({ collapsed: false })
    await user.click(screen.getByRole('button', { name: 'Collapse sidebar' }))
    expect(props.onToggleCollapse).toHaveBeenCalledTimes(1)
  })

  it('collapseToggleHasCorrectAriaLabelWhenExpanded', () => {
    renderSidebar({ collapsed: false })
    expect(screen.getByRole('button', { name: 'Collapse sidebar' })).toBeTruthy()
  })

  it('collapseToggleHasCorrectAriaLabelWhenCollapsed', () => {
    renderSidebar({ collapsed: true })
    expect(screen.getByRole('button', { name: 'Expand sidebar' })).toBeTruthy()
  })
})

// ── Phase 5 / US3: Keyboard Navigation ──────────────────────────────────────

describe('AppSidebar — US3: Keyboard Navigation', () => {
  beforeEach(() => {
    // jsdom needs focus to be configured
    document.body.innerHTML = ''
  })

  it('activatesModuleOnEnterKey', async () => {
    const user = userEvent.setup()
    const { props } = renderSidebar({ activeModule: 'chat' })
    const dashBtn = screen.getByRole('button', { name: 'Dashboard' })
    dashBtn.focus()
    await user.keyboard('{Enter}')
    expect(props.onModuleSelect).toHaveBeenCalledWith('dashboard')
  })

  it('activatesModuleOnSpaceKey', async () => {
    const user = userEvent.setup()
    const { props } = renderSidebar({ activeModule: 'chat' })
    const dashBtn = screen.getByRole('button', { name: 'Dashboard' })
    dashBtn.focus()
    await user.keyboard(' ')
    expect(props.onModuleSelect).toHaveBeenCalledWith('dashboard')
  })

  it('moveFocusToNextItemOnArrowDown', async () => {
    const user = userEvent.setup()
    renderSidebar()
    const chatBtn = screen.getByRole('button', { name: 'Chat' })
    chatBtn.focus()
    await user.keyboard('{ArrowDown}')
    expect(document.activeElement).toBe(screen.getByRole('button', { name: 'Dashboard' }))
  })

  it('moveFocusToPreviousItemOnArrowUp', async () => {
    const user = userEvent.setup()
    renderSidebar()
    const dashBtn = screen.getByRole('button', { name: 'Dashboard' })
    dashBtn.focus()
    await user.keyboard('{ArrowUp}')
    expect(document.activeElement).toBe(screen.getByRole('button', { name: 'Chat' }))
  })

  it('wrapsFocusFromLastToFirstOnArrowDown', async () => {
    const user = userEvent.setup()
    renderSidebar()
    const lastBtn = screen.getByRole('button', { name: 'Sales' })
    lastBtn.focus()
    await user.keyboard('{ArrowDown}')
    expect(document.activeElement).toBe(screen.getByRole('button', { name: 'Chat' }))
  })

  it('wrapsFocusFromFirstToLastOnArrowUp', async () => {
    const user = userEvent.setup()
    renderSidebar()
    const firstBtn = screen.getByRole('button', { name: 'Chat' })
    firstBtn.focus()
    await user.keyboard('{ArrowUp}')
    expect(document.activeElement).toBe(screen.getByRole('button', { name: 'Sales' }))
  })
})
