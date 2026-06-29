export interface SalesByCustomer {
  customerId: string
  customerName: string
  year: string
  totalSales: number
  currency: string
}

export interface SalesByProduct {
  productId: string
  productName: string
  year: string
  totalSales: number
  currency: string
  quantity: number
  unitOfMeasure: string
}

export interface PagedResult<T> {
  page: number
  pageSize: number
  totalRecords: number
  results: T[]
}

export type HubConnectionState = 'idle' | 'connected' | 'reconnecting' | 'offline'
