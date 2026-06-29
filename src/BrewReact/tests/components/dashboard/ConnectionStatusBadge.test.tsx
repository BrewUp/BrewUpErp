import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { ConnectionStatusBadge } from '@features/dashboard/components/ConnectionStatusBadge'

describe('ConnectionStatusBadge', () => {
  it('showsLiveBadgeWhenConnected', () => {
    render(<ConnectionStatusBadge hubState="connected" />)
    expect(screen.getByRole('status')).toHaveTextContent('Live')
    expect(screen.getByRole('status')).toHaveClass('connection-badge--connected')
  })

  it('showsReconnectingBadgeWhenReconnecting', () => {
    render(<ConnectionStatusBadge hubState="reconnecting" />)
    expect(screen.getByRole('status')).toHaveTextContent('Reconnecting…')
    expect(screen.getByRole('status')).toHaveClass('connection-badge--reconnecting')
  })

  it('showsOfflineBadgeWhenOffline', () => {
    render(<ConnectionStatusBadge hubState="offline" />)
    expect(screen.getByRole('status')).toHaveTextContent('Offline')
    expect(screen.getByRole('status')).toHaveClass('connection-badge--offline')
  })

  it('showsNothingWhenIdle', () => {
    const { container } = render(<ConnectionStatusBadge hubState="idle" />)
    expect(container.firstChild).toBeNull()
  })

  it('hasRoleStatusForLiveRegion', () => {
    render(<ConnectionStatusBadge hubState="connected" />)
    expect(screen.getByRole('status')).toBeInTheDocument()
  })
})
