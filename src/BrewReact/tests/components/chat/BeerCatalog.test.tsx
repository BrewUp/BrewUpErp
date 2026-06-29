import { describe, it, expect } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import BeerCatalog from '@features/chat/components/BeerCatalog'

describe('BeerCatalog', () => {
  it('renders the table with column headers Name, Style, ABV, Status', async () => {
    render(<BeerCatalog />)
    await waitFor(() => {
      expect(screen.getByText(/birra|nome|name/i)).toBeInTheDocument()
    })
    expect(screen.getByText(/stile/i)).toBeInTheDocument()
    expect(screen.getByText(/abv/i)).toBeInTheDocument()
    expect(screen.getByText(/attivo|status/i)).toBeInTheDocument()
  })

  it('renders beer rows from the API', async () => {
    render(<BeerCatalog />)
    await waitFor(() => {
      expect(screen.getByText('Pale Ale')).toBeInTheDocument()
    })
  })

  it('renders "—" for null alcoholByVolume', async () => {
    render(<BeerCatalog />)
    await waitFor(() => {
      expect(screen.getByText('Stout')).toBeInTheDocument()
    })
    // The Stout beer has null ABV in mock handlers
    const rows = screen.getAllByRole('row')
    const stoutRow = rows.find((r) => r.textContent?.includes('Stout'))
    expect(stoutRow?.textContent).toContain('—')
  })

  it('shows the correct status for isActive: false', async () => {
    render(<BeerCatalog />)
    await waitFor(() => {
      expect(screen.getByText('Stout')).toBeInTheDocument()
    })
    const rows = screen.getAllByRole('row')
    const stoutRow = rows.find((r) => r.textContent?.includes('Stout'))
    expect(stoutRow?.textContent).toMatch(/no|inattivo|false/i)
  })

  it('shows "Nessun elemento trovato" when the catalog is empty', async () => {
    const { http, HttpResponse } = await import('msw')
    const { server } = await import('../../../src/mocks/server')

    server.use(
      http.get('http://localhost:6094/chat/beers', () => {
        return HttpResponse.json([])
      }),
    )

    render(<BeerCatalog />)
    await waitFor(() => {
      expect(screen.getByText(/nessun elemento trovato/i)).toBeInTheDocument()
    })
  })
})
