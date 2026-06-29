import { describe, it, expect } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import DashboardPage from '@features/dashboard/components/DashboardPage'

// No SignalR mock — the real @microsoft/signalr is used.
// The hub will fail to connect (no backend in test env) and the badge
// will show "Offline". REST data is served by MSW (handlers.ts).

describe('DashboardPage (integration)', () => {
  it('rendersDashboardWithCustomerTabByDefault', async () => {
    render(<DashboardPage />)
    await waitFor(() =>
      expect(screen.getByRole('table', { name: /sales by customer/i })).toBeInTheDocument()
    )
  })

  it('switchesToProductTabOnClick', async () => {
    render(<DashboardPage />)
    // Wait for initial customer data to load
    await waitFor(() =>
      expect(screen.getByRole('table', { name: /sales by customer/i })).toBeInTheDocument()
    )
    await userEvent.click(screen.getByRole('tab', { name: /sales by product/i }))
    await waitFor(() =>
      expect(screen.getByRole('table', { name: /sales by product/i })).toBeInTheDocument()
    )
  })

  it('dashboardRemainsFunctionalWhenHubOffline', async () => {
    render(<DashboardPage />)
    // REST data loads despite hub failing
    await waitFor(() =>
      expect(screen.getByRole('table', { name: /sales by customer/i })).toBeInTheDocument()
    )
    // Badge eventually shows offline (hub fails to connect)
    await waitFor(
      () => expect(screen.getByRole('status')).toHaveTextContent('Offline'),
      { timeout: 8000 }
    )
  })

  it('customerTabShowsDataFromApi', async () => {
    render(<DashboardPage />)
    await waitFor(() => expect(screen.getByText('Test Customer')).toBeInTheDocument())
    expect(screen.getByText('2025')).toBeInTheDocument()
    expect(screen.getByText('100.50')).toBeInTheDocument()
  })

  it('productTabShowsDataFromApi', async () => {
    render(<DashboardPage />)
    await waitFor(() =>
      expect(screen.getByRole('table', { name: /sales by customer/i })).toBeInTheDocument()
    )
    await userEvent.click(screen.getByRole('tab', { name: /sales by product/i }))
    await waitFor(() => expect(screen.getByText('Test Beer')).toBeInTheDocument())
    expect(screen.getByText('L')).toBeInTheDocument()
  })
})
