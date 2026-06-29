import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { CreateOrderForm } from '@features/sales/components/CreateOrderForm'
import * as useCustomersHook from '@features/sales/hooks/useCustomers'
import * as useBeersHook from '@features/sales/hooks/useBeers'
import * as salesApiService from '@features/sales/services/salesApiService'

vi.mock('@features/sales/hooks/useCustomers')
vi.mock('@features/sales/hooks/useBeers')
vi.mock('@features/sales/services/salesApiService')

const mockUseCustomers = vi.mocked(useCustomersHook.useCustomers)
const mockUseBeers = vi.mocked(useBeersHook.useBeers)
const mockCreateSalesOrder = vi.mocked(salesApiService.createSalesOrder)

const defaultCustomers: useCustomersHook.UseCustomersResult = {
  customers: [
    { customerId: 'c1', ragioneSociale: 'Birrificio Rossi SRL' },
    { customerId: 'c2', ragioneSociale: 'Brasserie Dupont' },
  ],
  isLoading: false,
  error: null,
}

const defaultBeers: useBeersHook.UseBeersResult = {
  beers: [
    { beerId: 'b1', beerName: 'Pale Ale', price: { value: 3.5, currency: 'EUR' } },
    { beerId: 'b2', beerName: 'Stout', price: { value: 4.0, currency: 'EUR' } },
  ],
  isLoading: false,
  error: null,
}

describe('CreateOrderForm', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseCustomers.mockReturnValue(defaultCustomers)
    mockUseBeers.mockReturnValue(defaultBeers)
    mockCreateSalesOrder.mockResolvedValue(undefined)
  })

  it('rendersAllFormFields', () => {
    render(<CreateOrderForm onSuccess={vi.fn()} onCancel={vi.fn()} />)
    expect(screen.getByLabelText(/order number/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/order date/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/delivery date/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/customer/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /add row/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /submit|create|save/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument()
  })

  it('orderDateDefaultsToToday', () => {
    render(<CreateOrderForm onSuccess={vi.fn()} onCancel={vi.fn()} />)
    const today = new Date().toISOString().slice(0, 16)
    const orderDateInput = screen.getByLabelText(/order date/i) as HTMLInputElement
    expect(orderDateInput.value).toBe(today)
  })

  it('customerSelectorLoadsCustomers', () => {
    render(<CreateOrderForm onSuccess={vi.fn()} onCancel={vi.fn()} />)
    const select = screen.getByLabelText(/customer/i)
    expect(within(select as HTMLElement).getByText('Birrificio Rossi SRL')).toBeInTheDocument()
    expect(within(select as HTMLElement).getByText('Brasserie Dupont')).toBeInTheDocument()
  })

  it('beerSelectorLoadsBeers', async () => {
    render(<CreateOrderForm onSuccess={vi.fn()} onCancel={vi.fn()} />)
    await userEvent.click(screen.getByRole('button', { name: /add row/i }))
    const beerSelect = screen.getAllByRole('combobox').find(el =>
      within(el as HTMLElement).queryByText('Pale Ale') !== null ||
      (el as HTMLSelectElement).options[1]?.text === 'Pale Ale'
    )
    expect(beerSelect).toBeDefined()
  })

  it('selectingBeerPreFillsPrice', async () => {
    render(<CreateOrderForm onSuccess={vi.fn()} onCancel={vi.fn()} />)
    await userEvent.click(screen.getByRole('button', { name: /add row/i }))
    const selects = screen.getAllByRole('combobox')
    const beerSelect = selects[selects.length - 1]
    await userEvent.selectOptions(beerSelect, 'b1')
    const priceInputs = screen.getAllByDisplayValue('3.5')
    expect(priceInputs.length).toBeGreaterThan(0)
  })

  it('addRowButtonAddsRow', async () => {
    render(<CreateOrderForm onSuccess={vi.fn()} onCancel={vi.fn()} />)
    expect(screen.queryAllByRole('row').filter(r => r.closest('tbody'))).toHaveLength(0)
    await userEvent.click(screen.getByRole('button', { name: /add row/i }))
    expect(screen.getAllByRole('button', { name: /remove/i })).toHaveLength(1)
    await userEvent.click(screen.getByRole('button', { name: /add row/i }))
    expect(screen.getAllByRole('button', { name: /remove/i })).toHaveLength(2)
  })

  it('removeRowButtonRemovesRow', async () => {
    render(<CreateOrderForm onSuccess={vi.fn()} onCancel={vi.fn()} />)
    await userEvent.click(screen.getByRole('button', { name: /add row/i }))
    await userEvent.click(screen.getByRole('button', { name: /add row/i }))
    expect(screen.getAllByRole('button', { name: /remove/i })).toHaveLength(2)
    await userEvent.click(screen.getAllByRole('button', { name: /remove/i })[0])
    expect(screen.getAllByRole('button', { name: /remove/i })).toHaveLength(1)
  })

  it('cancelNavigatesBackToList', async () => {
    const onCancel = vi.fn()
    render(<CreateOrderForm onSuccess={vi.fn()} onCancel={onCancel} />)
    await userEvent.click(screen.getByRole('button', { name: /cancel/i }))
    expect(onCancel).toHaveBeenCalledOnce()
    expect(mockCreateSalesOrder).not.toHaveBeenCalled()
  })

  it('submitDisabledWhenZeroRows', () => {
    render(<CreateOrderForm onSuccess={vi.fn()} onCancel={vi.fn()} />)
    const submitBtn = screen.getByRole('button', { name: /submit|create|save/i })
    expect(submitBtn).toBeDisabled()
  })

  it('submitsValidForm', async () => {
    const onSuccess = vi.fn()
    render(<CreateOrderForm onSuccess={onSuccess} onCancel={vi.fn()} />)

    await userEvent.type(screen.getByLabelText(/order number/i), 'ORD-TEST-001')
    await userEvent.selectOptions(screen.getByLabelText(/customer/i), 'c1')
    await userEvent.click(screen.getByRole('button', { name: /add row/i }))
    const selects = screen.getAllByRole('combobox')
    const beerSelect = selects[selects.length - 1]
    await userEvent.selectOptions(beerSelect, 'b1')
    const quantityInputs = screen.getAllByPlaceholderText(/qty/i)
    await userEvent.type(quantityInputs[0], '10')
    await userEvent.click(screen.getByRole('button', { name: /submit|create|save/i }))

    await waitFor(() => expect(mockCreateSalesOrder).toHaveBeenCalledOnce())
    expect(onSuccess).toHaveBeenCalledOnce()
  })

  it('showsValidationErrorForMissingOrderNumber', async () => {
    render(<CreateOrderForm onSuccess={vi.fn()} onCancel={vi.fn()} />)
    await userEvent.click(screen.getByRole('button', { name: /add row/i }))
    const selects = screen.getAllByRole('combobox')
    await userEvent.selectOptions(selects[selects.length - 1], 'b1')
    await userEvent.selectOptions(screen.getByLabelText(/customer/i), 'c1')
    // Don't fill order number → submit should show error
    // submit is disabled when rows=0, so we need a row. But can't submit with empty order number
    // The submit button is enabled with a row, but validation fires first
    const quantityInputs = screen.getAllByPlaceholderText(/qty/i)
    await userEvent.type(quantityInputs[0], '5')
    await userEvent.click(screen.getByRole('button', { name: /submit|create|save/i }))
    expect(screen.getByText(/order number.*required|required.*order number/i)).toBeInTheDocument()
  })

  it('showsValidationErrorForMissingCustomer', async () => {
    render(<CreateOrderForm onSuccess={vi.fn()} onCancel={vi.fn()} />)
    await userEvent.type(screen.getByLabelText(/order number/i), 'ORD-X')
    await userEvent.click(screen.getByRole('button', { name: /add row/i }))
    const selects = screen.getAllByRole('combobox')
    await userEvent.selectOptions(selects[selects.length - 1], 'b1')
    const quantityInputs = screen.getAllByPlaceholderText(/qty/i)
    await userEvent.type(quantityInputs[0], '5')
    await userEvent.click(screen.getByRole('button', { name: /submit|create|save/i }))
    expect(screen.getByText(/customer.*required|required.*customer/i)).toBeInTheDocument()
  })

  it('showsApiErrorOnFailure', async () => {
    mockCreateSalesOrder.mockRejectedValue(new Error('Server rejected'))
    render(<CreateOrderForm onSuccess={vi.fn()} onCancel={vi.fn()} />)
    await userEvent.type(screen.getByLabelText(/order number/i), 'ORD-FAIL')
    await userEvent.selectOptions(screen.getByLabelText(/customer/i), 'c1')
    await userEvent.click(screen.getByRole('button', { name: /add row/i }))
    const selects = screen.getAllByRole('combobox')
    await userEvent.selectOptions(selects[selects.length - 1], 'b1')
    const quantityInputs = screen.getAllByPlaceholderText(/qty/i)
    await userEvent.type(quantityInputs[0], '5')
    await userEvent.click(screen.getByRole('button', { name: /submit|create|save/i }))
    await waitFor(() => expect(screen.getByText('Server rejected')).toBeInTheDocument())
  })

  it('staysOnFormAfterApiError', async () => {
    const onSuccess = vi.fn()
    mockCreateSalesOrder.mockRejectedValue(new Error('API failure'))
    render(<CreateOrderForm onSuccess={onSuccess} onCancel={vi.fn()} />)
    await userEvent.type(screen.getByLabelText(/order number/i), 'ORD-X')
    await userEvent.selectOptions(screen.getByLabelText(/customer/i), 'c1')
    await userEvent.click(screen.getByRole('button', { name: /add row/i }))
    const selects = screen.getAllByRole('combobox')
    await userEvent.selectOptions(selects[selects.length - 1], 'b1')
    const quantityInputs = screen.getAllByPlaceholderText(/qty/i)
    await userEvent.type(quantityInputs[0], '5')
    await userEvent.click(screen.getByRole('button', { name: /submit|create|save/i }))
    await waitFor(() => expect(screen.getByText('API failure')).toBeInTheDocument())
    expect(onSuccess).not.toHaveBeenCalled()
  })
})
