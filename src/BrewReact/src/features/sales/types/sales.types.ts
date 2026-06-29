export interface Price {
  value: number
  currency: string
}

export interface Quantity {
  value: number
  unitOfMeasure: string
}

export interface SalesOrderRow {
  beerId: string
  beerName: string
  quantity: Quantity
  price: Price
}

export interface SalesOrder {
  id: string
  orderNumber: string
  orderDate: string
  deliveryDate?: string
  customerId: string
  customerName: string
  rows: SalesOrderRow[]
  status: string
}

export interface Customer {
  customerId: string
  ragioneSociale: string
}

export interface Beer {
  beerId: string
  beerName: string
  price: Price
}

export interface PagedResult<T> {
  page: number
  pageSize: number
  totalRecords: number
  results: T[]
}

export interface CreateSalesOrderRowPayload {
  beerId: string
  beerName: string
  quantity: Quantity
  price: Price
}

export interface CreateSalesOrderPayload {
  orderNumber: string
  orderDate: string
  deliveryDate?: string
  customerId: string
  customerName: string
  rows: CreateSalesOrderRowPayload[]
}

export interface OrderRowDraft {
  id: string
  beerId: string
  beerName: string
  quantityValue: string
  unitOfMeasure: string
  priceValue: string
  currency: string
}
