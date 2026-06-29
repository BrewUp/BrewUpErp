import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import SalesPage from '@features/sales/components/SalesPage'
import * as useSalesOrdersHook from '@features/sales/hooks/useSalesOrders'
import * as useCustomersHook from '@features/sales/hooks/useCustomers'
import * as useBeersHook from '@features/sales/hooks/useBeers'
import * as salesApiService from '@features/sales/services/salesApiService'

vi.mock('@features/sales/hooks/useSalesOrders')
vi.mock('@features/sales/hooks/useCustomers')
vi.mock('@features/sales/hooks/useBeers')
vi.mock('@features/sales/services/salesApiService')

const mockUseSalesOrders = vi.mocked(useSalesOrdersHook.useSalesOrders)
const mockUseCustomers = vi.mocked(useCustomersHook.useCustomers)
const mockUseBeers = vi.mocked(useBeersHook.useBeers)
const mockCreateSalesOrder = vi.mocked(salesApiService.createSalesOrder)

const salesOrdersResult: useSalesOrdersHook.UseSalesOrdersResult = {
  data: [{ id: 'o1', orderNumber: 'ORD-001', orderDate: '2025-01-15T10:00:00Z', customerId: 'c1', customerName: 'Test Co', rows: [], status: 'Open' }],
  page: 1,
  totalRecords: 1,
  pageSize: 10,
  isLoading: false,
  error: null,
  goToNextPage: vi.fn(),
  goToPrevPage: vi.fn(),
}

const customersResult: useCustomersHook.UseCustomersResult = {
  customers: [{ customerId: 'c1', ragioneSociale: 'Test Co' }],
  isLoading: false,
  error: null,
}

const beersResult: useBeersHook.UseBeersResult = {
  beers: [{ beerId: 'b1', beerName: 'Pale Ale', price: { value: 3.5, currency: 'EUR' } }],
  isLoading: false,
  error: null,
}

describe('SalesPage integration', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseSalesOrders.mockReturnValue(salesOrdersResult)
    mockUseCustomers.mockReturnValue(customersResult)
    mockUseBeers.mockReturnValue(beersResult)
    mockCreateSalesOrder.mockResolvedValue(undefined)
  })

  it('rendersOrdersTableByDefault', () => {
    render(<SalesPage />)
    expect(screen.getByRole('table', { name: /sales orders/i })).toBeInTheDocument()
    expect(screen.queryByRole('form')).not.toBeInTheDocument()
  })

  it('switchesToCreateFormOnNewOrder', async () => {
    render(<SalesPage />)
    await userEvent.click(screen.getByRole('button', { name: /new order/i }))
    expect(screen.queryByRole('table', { name: /sales orders/i })).not.toBeInTheDocument()
    expect(screen.getByLabelText(/order number/i)).toBeInTheDocument()
  })

  it('returnsToListOnCancel', async () => {
    render(<SalesPage />)
    await userEvent.click(screen.getByRole('button', { name: /new order/i }))
    await userEvent.click(screen.getByRole('button', { name: /cancel/i }))
    expect(screen.getByRole('table', { name: /sales orders/i })).toBeInTheDocument()
  })

  it('returnsToListOnSuccess', async () => {
    render(<SalesPage />)
    await userEvent.click(screen.getByRole('button', { name: /new order/i }))
    await userEvent.type(screen.getByLabelText(/order number/i), 'ORD-NEW')
    await userEvent.selectOptions(screen.getByLabelText(/customer/i), 'c1')
    await userEvent.click(screen.getByRole('button', { name: /add row/i }))
    const selects = screen.getAllByRole('combobox')
    await userEvent.selectOptions(selects[selects.length - 1], 'b1')
    const qtyInputs = screen.getAllByPlaceholderText(/qty/i)
    await userEvent.type(qtyInputs[0], '5')
    await userEvent.click(screen.getByRole('button', { name: /create order/i }))
    await waitFor(() => expect(screen.getByRole('table', { name: /sales orders/i })).toBeInTheDocument())
  })

  it('tableDataLoadedFromMsw', () => {
    render(<SalesPage />)
    expect(screen.getByText('ORD-001')).toBeInTheDocument()
    expect(screen.getByText('Test Co')).toBeInTheDocument()
    expect(screen.getByText('Open')).toBeInTheDocument()
  })
})
