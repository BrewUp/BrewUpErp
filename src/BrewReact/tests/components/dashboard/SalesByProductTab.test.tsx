import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { SalesByProductTab } from '@features/dashboard/components/SalesByProductTab'
import * as hook from '@features/dashboard/hooks/useSalesByProduct'
import type { UseSalesByProductResult } from '@features/dashboard/hooks/useSalesByProduct'

vi.mock('@features/dashboard/hooks/useSalesByProduct')

const defaultHookValue: UseSalesByProductResult = {
  data: [{ productId: 'p1', productName: 'Hoppy Futura IPA', year: '2025', totalSales: 9200.0, currency: 'EUR', quantity: 2300.5, unitOfMeasure: 'L' }],
  page: 1,
  totalRecords: 15,
  pageSize: 10,
  isLoading: false,
  error: null,
  goToNextPage: vi.fn(),
  goToPrevPage: vi.fn(),
}

beforeEach(() => {
  vi.mocked(hook.useSalesByProduct).mockReturnValue({ ...defaultHookValue, goToNextPage: vi.fn(), goToPrevPage: vi.fn() })
})

describe('SalesByProductTab', () => {
  it('rendersSalesByProductTable', () => {
    render(<SalesByProductTab refreshToken={0} />)
    expect(screen.getByRole('table', { name: /sales by product/i })).toBeInTheDocument()
  })

  it('rendersProductNameYearTotalSalesCurrencyQuantityUomColumns', () => {
    render(<SalesByProductTab refreshToken={0} />)
    expect(screen.getByText('Product Name')).toBeInTheDocument()
    expect(screen.getByText('Year')).toBeInTheDocument()
    expect(screen.getByText('Total Sales')).toBeInTheDocument()
    expect(screen.getByText('Currency')).toBeInTheDocument()
    expect(screen.getByText('Quantity')).toBeInTheDocument()
    expect(screen.getByText('UoM')).toBeInTheDocument()
  })

  it('formatsDecimalsToTwoPlaces', () => {
    render(<SalesByProductTab refreshToken={0} />)
    expect(screen.getByText('9,200.00')).toBeInTheDocument()
    expect(screen.getByText('2,300.50')).toBeInTheDocument()
  })

  it('showsLoadingState', () => {
    vi.mocked(hook.useSalesByProduct).mockReturnValue({ ...defaultHookValue, isLoading: true, data: [] })
    render(<SalesByProductTab refreshToken={0} />)
    expect(screen.getByText(/loading/i)).toBeInTheDocument()
  })

  it('showsErrorState', () => {
    vi.mocked(hook.useSalesByProduct).mockReturnValue({ ...defaultHookValue, error: 'Network error', data: [] })
    render(<SalesByProductTab refreshToken={0} />)
    expect(screen.getByText(/network error/i)).toBeInTheDocument()
  })

  it('showsEmptyState', () => {
    vi.mocked(hook.useSalesByProduct).mockReturnValue({ ...defaultHookValue, data: [], totalRecords: 0 })
    render(<SalesByProductTab refreshToken={0} />)
    expect(screen.getByText(/no data/i)).toBeInTheDocument()
  })

  it('paginationControlsWorkCorrectly', async () => {
    const goToNextPage = vi.fn()
    const goToPrevPage = vi.fn()
    vi.mocked(hook.useSalesByProduct).mockReturnValue({
      ...defaultHookValue,
      goToNextPage,
      goToPrevPage,
      page: 2,
      totalRecords: 25,
    })
    render(<SalesByProductTab refreshToken={0} />)
    await userEvent.click(screen.getByRole('button', { name: /next page/i }))
    expect(goToNextPage).toHaveBeenCalledOnce()
    await userEvent.click(screen.getByRole('button', { name: /previous page/i }))
    expect(goToPrevPage).toHaveBeenCalledOnce()
  })
})
