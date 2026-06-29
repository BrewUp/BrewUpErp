import type { AxiosError } from 'axios'
import salesApiClient from '@shared/api/salesApiClient'
import type {
  PagedResult,
  SalesOrder,
  Customer,
  Beer,
  CreateSalesOrderPayload,
} from '../types/sales.types'

interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
}

function mapError(error: unknown): string {
  const axiosError = error as AxiosError<ProblemDetails>
  if (axiosError.response?.data) {
    const { detail, title } = axiosError.response.data
    return detail ?? title ?? 'Communication error'
  }
  return 'Communication error'
}

export async function getSalesOrders(pageNumber: number): Promise<PagedResult<SalesOrder>> {
  try {
    const { data } = await salesApiClient.get<PagedResult<SalesOrder>>('/v1/sales', {
      params: { pageNumber },
    })
    return data
  } catch (error) {
    throw new Error(mapError(error), { cause: error })
  }
}

export async function createSalesOrder(payload: CreateSalesOrderPayload): Promise<void> {
  try {
    await salesApiClient.post('/v1/sales', payload)
  } catch (error) {
    throw new Error(mapError(error), { cause: error })
  }
}

export async function getCustomers(): Promise<PagedResult<Customer>> {
  try {
    const { data } = await salesApiClient.get<PagedResult<Customer>>('/v1/masterdata/customers', {
      params: { pageNumber: 1, pageSize: 100 },
    })
    return data
  } catch (error) {
    throw new Error(mapError(error), { cause: error })
  }
}

export async function getBeers(): Promise<PagedResult<Beer>> {
  try {
    const { data } = await salesApiClient.get<PagedResult<Beer>>('/v1/masterdata/beers', {
      params: { pageNumber: 1, pageSize: 100 },
    })
    return data
  } catch (error) {
    throw new Error(mapError(error), { cause: error })
  }
}
