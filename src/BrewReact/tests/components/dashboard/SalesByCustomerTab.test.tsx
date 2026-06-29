import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { SalesByCustomerTab } from '@features/dashboard/components/SalesByCustomerTab'
import * as hook from '@features/dashboard/hooks/useSalesByCustomer'
import type { UseSalesByCustomerResult } from '@features/dashboard/hooks/useSalesByCustomer'

vi.mock('@features/dashboard/hooks/useSalesByCustomer')

const defaultHookValue: UseSalesByCustomerResult = {
  data: [{ customerId: 'c1', customerName: 'Birrificio Sud', year: '2025', totalSales: 18450.75, currency: 'EUR' }],
  page: 1,
  totalRecords: 15,
  pageSize: 10,
  isLoading: false,
  error: null,
  goToNextPage: vi.fn(),
  goToPrevPage: vi.fn(),
}

beforeEach(() => {
  vi.mocked(hook.useSalesByCustomer).mockReturnValue({ ...defaultHookValue, goToNextPage: vi.fn(), goToPrevPage: vi.fn() })
})

describe('SalesByCustomerTab', () => {
  it('rendersSalesByCustomerTable', () => {
    render(<SalesByCustomerTab refreshToken={0} />)
    expect(screen.getByRole('table', { name: /sales by customer/i })).toBeInTheDocument()
  })

  it('rendersCustomerNameYearTotalSalesCurrencyColumns', () => {
    render(<SalesByCustomerTab refreshToken={0} />)
    expect(screen.getByText('Customer Name')).toBeInTheDocument()
    expect(screen.getByText('Year')).toBeInTheDocument()
    expect(screen.getByText('Total Sales')).toBeInTheDocument()
    expect(screen.getByText('Currency')).toBeInTheDocument()
  })

  it('formatsDecimalsToTwoPlaces', () => {
    render(<SalesByCustomerTab refreshToken={0} />)
    expect(screen.getByText('18,450.75')).toBeInTheDocument()
  })

  it('showsLoadingState', () => {
    vi.mocked(hook.useSalesByCustomer).mockReturnValue({ ...defaultHookValue, isLoading: true, data: [] })
    render(<SalesByCustomerTab refreshToken={0} />)
    expect(screen.getByText(/loading/i)).toBeInTheDocument()
  })

  it('showsErrorState', () => {
    vi.mocked(hook.useSalesByCustomer).mockReturnValue({ ...defaultHookValue, error: 'Something went wrong', data: [] })
    render(<SalesByCustomerTab refreshToken={0} />)
    expect(screen.getByText(/something went wrong/i)).toBeInTheDocument()
  })

  it('showsEmptyState', () => {
    vi.mocked(hook.useSalesByCustomer).mockReturnValue({ ...defaultHookValue, data: [], totalRecords: 0 })
    render(<SalesByCustomerTab refreshToken={0} />)
    expect(screen.getByText(/no data/i)).toBeInTheDocument()
  })

  it('nextPageButtonDisabledOnLastPage', () => {
    // totalRecords=10 pageSize=10 page=1 → last page
    vi.mocked(hook.useSalesByCustomer).mockReturnValue({ ...defaultHookValue, page: 1, totalRecords: 10, pageSize: 10 })
    render(<SalesByCustomerTab refreshToken={0} />)
    expect(screen.getByRole('button', { name: /next page/i })).toBeDisabled()
  })

  it('prevPageButtonDisabledOnFirstPage', () => {
    vi.mocked(hook.useSalesByCustomer).mockReturnValue({ ...defaultHookValue, page: 1 })
    render(<SalesByCustomerTab refreshToken={0} />)
    expect(screen.getByRole('button', { name: /previous page/i })).toBeDisabled()
  })

  it('callsGoToNextPageWhenNextClicked', async () => {
    const goToNextPage = vi.fn()
    vi.mocked(hook.useSalesByCustomer).mockReturnValue({ ...defaultHookValue, goToNextPage, page: 1, totalRecords: 15 })
    render(<SalesByCustomerTab refreshToken={0} />)
    await userEvent.click(screen.getByRole('button', { name: /next page/i }))
    expect(goToNextPage).toHaveBeenCalledOnce()
  })

  it('callsGoToPrevPageWhenPrevClicked', async () => {
    const goToPrevPage = vi.fn()
    vi.mocked(hook.useSalesByCustomer).mockReturnValue({ ...defaultHookValue, goToPrevPage, page: 2, totalRecords: 15 })
    render(<SalesByCustomerTab refreshToken={0} />)
    await userEvent.click(screen.getByRole('button', { name: /previous page/i }))
    expect(goToPrevPage).toHaveBeenCalledOnce()
  })
})
