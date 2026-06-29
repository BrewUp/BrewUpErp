import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { SalesOrdersTable } from '@features/sales/components/SalesOrdersTable'
import * as useSalesOrdersHook from '@features/sales/hooks/useSalesOrders'

vi.mock('@features/sales/hooks/useSalesOrders')

const mockUseSalesOrders = vi.mocked(useSalesOrdersHook.useSalesOrders)

const defaultResult: useSalesOrdersHook.UseSalesOrdersResult = {
  data: [
    {
      id: 'o1',
      orderNumber: 'ORD-001',
      orderDate: '2025-01-15T10:00:00Z',
      deliveryDate: '2025-01-20T10:00:00Z',
      customerId: 'c1',
      customerName: 'Test Customer',
      rows: [],
      status: 'Open',
    },
    {
      id: 'o2',
      orderNumber: 'ORD-002',
      orderDate: '2025-01-10T09:00:00Z',
      customerId: 'c2',
      customerName: 'Another Co',
      rows: [],
      status: 'Closed',
    },
  ],
  page: 1,
  totalRecords: 12,
  pageSize: 10,
  isLoading: false,
  error: null,
  goToNextPage: vi.fn(),
  goToPrevPage: vi.fn(),
}

describe('SalesOrdersTable', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseSalesOrders.mockReturnValue(defaultResult)
  })

  it('rendersTableWithHeaders', () => {
    render(<SalesOrdersTable onNewOrder={vi.fn()} />)
    expect(screen.getByRole('table', { name: /sales orders/i })).toBeInTheDocument()
    expect(screen.getByText('Order Number')).toBeInTheDocument()
    expect(screen.getByText('Customer')).toBeInTheDocument()
    expect(screen.getByText('Order Date')).toBeInTheDocument()
    expect(screen.getByText('Delivery Date')).toBeInTheDocument()
    expect(screen.getByText('Status')).toBeInTheDocument()
  })

  it('rendersOrderRows', () => {
    render(<SalesOrdersTable onNewOrder={vi.fn()} />)
    expect(screen.getByText('ORD-001')).toBeInTheDocument()
    expect(screen.getByText('Test Customer')).toBeInTheDocument()
    expect(screen.getByText('ORD-002')).toBeInTheDocument()
    expect(screen.getByText('Another Co')).toBeInTheDocument()
    expect(screen.getByText('Open')).toBeInTheDocument()
    expect(screen.getByText('Closed')).toBeInTheDocument()
  })

  it('showsLoadingState', () => {
    mockUseSalesOrders.mockReturnValue({ ...defaultResult, isLoading: true, data: [] })
    render(<SalesOrdersTable onNewOrder={vi.fn()} />)
    expect(screen.getByText(/loading/i)).toBeInTheDocument()
  })

  it('showsErrorMessage', () => {
    mockUseSalesOrders.mockReturnValue({ ...defaultResult, error: 'Load failed', data: [] })
    render(<SalesOrdersTable onNewOrder={vi.fn()} />)
    expect(screen.getByText('Load failed')).toBeInTheDocument()
  })

  it('showsEmptyState', () => {
    mockUseSalesOrders.mockReturnValue({ ...defaultResult, data: [], totalRecords: 0 })
    render(<SalesOrdersTable onNewOrder={vi.fn()} />)
    expect(screen.getByText(/no sales orders/i)).toBeInTheDocument()
  })

  it('prevButtonDisabledOnFirstPage', () => {
    mockUseSalesOrders.mockReturnValue({ ...defaultResult, page: 1 })
    render(<SalesOrdersTable onNewOrder={vi.fn()} />)
    expect(screen.getByRole('button', { name: /previous/i })).toBeDisabled()
  })

  it('nextButtonDisabledOnLastPage', () => {
    mockUseSalesOrders.mockReturnValue({ ...defaultResult, page: 1, totalRecords: 5, pageSize: 10 })
    render(<SalesOrdersTable onNewOrder={vi.fn()} />)
    expect(screen.getByRole('button', { name: /next/i })).toBeDisabled()
  })

  it('paginationNextWorks', async () => {
    const goToNextPage = vi.fn()
    mockUseSalesOrders.mockReturnValue({ ...defaultResult, goToNextPage })
    render(<SalesOrdersTable onNewOrder={vi.fn()} />)
    await userEvent.click(screen.getByRole('button', { name: /next/i }))
    expect(goToNextPage).toHaveBeenCalledOnce()
  })

  it('showsNewOrderButton', async () => {
    const onNewOrder = vi.fn()
    render(<SalesOrdersTable onNewOrder={onNewOrder} />)
    const btn = screen.getByRole('button', { name: /new order/i })
    expect(btn).toBeInTheDocument()
    await userEvent.click(btn)
    expect(onNewOrder).toHaveBeenCalledOnce()
  })
})
