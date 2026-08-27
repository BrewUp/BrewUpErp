import { useState, useId } from 'react'
import { useCustomers } from '../hooks/useCustomers'
import { useBeers } from '../hooks/useBeers'
import { createSalesOrder } from '../services/salesApiService'
import type { OrderRowDraft, CreateSalesOrderPayload } from '../types/sales.types'

interface CreateOrderFormProps {
  onSuccess: () => void
  onCancel: () => void
}

function defaultOrderDate(): string {
  return new Date().toISOString().slice(0, 16)
}

function generateId(): string {
  return Math.random().toString(36).slice(2)
}

function emptyRow(): OrderRowDraft {
  return {
    id: generateId(),
    beerId: '',
    beerName: '',
    quantityValue: '',
    unitOfMeasure: '',
    priceValue: '',
    currency: '',
  }
}

export function CreateOrderForm({ onSuccess, onCancel }: CreateOrderFormProps) {
  const { customers } = useCustomers()
  const { beers } = useBeers()

  const [orderNumber, setOrderNumber] = useState('')
  const [orderDate, setOrderDate] = useState(defaultOrderDate)
  const [deliveryDate, setDeliveryDate] = useState('')
  const [customerId, setCustomerId] = useState('')
  const [rows, setRows] = useState<OrderRowDraft[]>([])
  const [errors, setErrors] = useState<Record<string, string>>({})
  const [apiError, setApiError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const orderNumberId = useId()
  const orderDateId = useId()
  const deliveryDateId = useId()
  const customerSelectId = useId()

  function validate(): Record<string, string> {
    const errs: Record<string, string> = {}
    if (!orderNumber.trim()) errs.orderNumber = 'Order Number is required'
    if (!customerId) errs.customerId = 'Customer is required'
    return errs
  }

  function addRow() {
    setRows((prev) => [...prev, emptyRow()])
  }

  function removeRow(id: string) {
    setRows((prev) => prev.filter((r) => r.id !== id))
  }

  function updateRow(id: string, field: keyof OrderRowDraft, value: string) {
    setRows((prev) =>
      prev.map((r) => {
        if (r.id !== id) return r
        if (field === 'beerId') {
          const beer = beers.find((b) => b.beerId === value)
          return {
            ...r,
            beerId: value,
            beerName: beer?.beerName ?? '',
            priceValue: beer ? String(beer.price.value) : '',
            currency: beer?.price.currency || 'EUR',
          }
        }
        return { ...r, [field]: value }
      })
    )
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    const errs = validate()
    if (Object.keys(errs).length > 0) {
      setErrors(errs)
      return
    }
    setErrors({})
    setApiError(null)

    const customer = customers.find((c) => c.customerId === customerId)
    const payload: CreateSalesOrderPayload = {
      orderNumber: orderNumber.trim(),
      orderDate: orderDate ? `${orderDate}:00Z` : '',
      deliveryDate: deliveryDate ? `${deliveryDate}:00Z` : undefined,
      customerId,
      customerName: customer?.ragioneSociale ?? '',
      rows: rows.map((r) => ({
        beerId: r.beerId,
        beerName: r.beerName,
        quantity: { value: parseFloat(r.quantityValue) || 0, unitOfMeasure: r.unitOfMeasure },
        price: { value: parseFloat(r.priceValue) || 0, currency: r.currency },
      })),
    }

    try {
      setIsSubmitting(true)
      await createSalesOrder(payload)
      onSuccess()
    } catch (err: unknown) {
      setApiError(err instanceof Error ? err.message : 'Communication error')
    } finally {
      setIsSubmitting(false)
    }
  }

  const canSubmit = rows.length > 0 && !isSubmitting

  return (
    <form className="order-form" onSubmit={handleSubmit} noValidate>
      <h2>New Sales Order</h2>

      {apiError && <p className="order-form__error">{apiError}</p>}

      <div className="form-field">
        <label htmlFor={orderNumberId}>Order Number</label>
        <input
          id={orderNumberId}
          type="text"
          value={orderNumber}
          onChange={(e) => setOrderNumber(e.target.value)}
          aria-label="Order Number"
        />
        {errors.orderNumber && <span className="form-field--error">{errors.orderNumber}</span>}
      </div>

      <div className="form-field">
        <label htmlFor={orderDateId}>Order Date</label>
        <input
          id={orderDateId}
          type="datetime-local"
          value={orderDate}
          onChange={(e) => setOrderDate(e.target.value)}
          aria-label="Order Date"
        />
      </div>

      <div className="form-field">
        <label htmlFor={deliveryDateId}>Delivery Date</label>
        <input
          id={deliveryDateId}
          type="datetime-local"
          value={deliveryDate}
          min={orderDate}
          onChange={(e) => setDeliveryDate(e.target.value)}
          aria-label="Delivery Date"
        />
      </div>

      <div className="form-field">
        <label htmlFor={customerSelectId}>Customer</label>
        <select
          id={customerSelectId}
          value={customerId}
          onChange={(e) => setCustomerId(e.target.value)}
          aria-label="Customer"
        >
          <option value="">— Select customer —</option>
          {customers.map((c) => (
            <option key={c.customerId} value={c.customerId}>
              {c.ragioneSociale}
            </option>
          ))}
        </select>
        {errors.customerId && <span className="form-field--error">{errors.customerId}</span>}
      </div>

      <div className="order-form__rows">
        <h3>Order Rows</h3>
        {rows.map((row) => (
          <div key={row.id} className="order-form__row">
            <select
              value={row.beerId}
              onChange={(e) => updateRow(row.id, 'beerId', e.target.value)}
              aria-label="Beer"
            >
              <option value="">— Select beer —</option>
              {beers.map((b) => (
                <option key={b.beerId} value={b.beerId}>
                  {b.beerName}
                </option>
              ))}
            </select>
            <input
              type="number"
              value={row.quantityValue}
              placeholder="Qty"
              onChange={(e) => updateRow(row.id, 'quantityValue', e.target.value)}
              aria-label="Quantity"
            />
            <input
              type="text"
              value={row.unitOfMeasure}
              placeholder="UoM"
              onChange={(e) => updateRow(row.id, 'unitOfMeasure', e.target.value)}
              aria-label="Unit of Measure"
            />
            <input
              type="number"
              value={row.priceValue}
              placeholder="Price"
              readOnly
              aria-label="Price"
            />
            <input
              type="text"
              value={row.currency}
              placeholder="Currency"
              readOnly
              aria-label="Currency"
            />
            <button type="button" onClick={() => removeRow(row.id)} aria-label="Remove row">
              Remove
            </button>
          </div>
        ))}
        <button type="button" onClick={addRow} aria-label="Add row">
          Add Row
        </button>
      </div>

      <div className="order-form__actions">
        <button type="submit" disabled={!canSubmit}>
          Create Order
        </button>
        <button type="button" onClick={onCancel}>
          Cancel
        </button>
      </div>
    </form>
  )
}

export default CreateOrderForm
